using System.Net.Http.Headers;
using System.Text;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Howsee.Application.Interfaces.Tours;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Howsee.Infrastructure.Services;

/// <summary>
/// Low-level GraphQL client for Matterport Model API. Used by <see cref="MatterportApiClient"/>.
/// Aligns with spec: GraphQLHttpClient, typed error mapping, no raw HttpClient.
/// </summary>
public class MatterportModelClient
{
    private readonly GraphQLHttpClient _client;
    private readonly ILogger<MatterportModelClient> _logger;
    private readonly MatterportApiOptions _options;

    public MatterportModelClient(IOptions<MatterportApiOptions> options, ILogger<MatterportModelClient> logger)
    {
        _options = options.Value;
        _logger = logger;
        var opt = _options;
        var httpOptions = new GraphQLHttpClientOptions
        {
            EndPoint = new Uri(string.IsNullOrWhiteSpace(opt.ModelApiEndpoint) ? "https://api.matterport.com/api/models/graph" : opt.ModelApiEndpoint)
        };
        _client = new GraphQLHttpClient(httpOptions, new NewtonsoftJsonSerializer());
        if (!string.IsNullOrWhiteSpace(opt.ApiToken))
            _client.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", opt.ApiToken);
        else if (!string.IsNullOrWhiteSpace(opt.ApiTokenId) && !string.IsNullOrWhiteSpace(opt.ApiTokenSecret))
        {
            var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{opt.ApiTokenId}:{opt.ApiTokenSecret}"));
            _client.HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        }
        _client.HttpClient.Timeout = TimeSpan.FromSeconds(Math.Clamp(opt.TimeoutSeconds, 5, 120));
    }

    /// <summary>Sends a GraphQL query/mutation and returns the data. Throws typed exceptions on API errors.</summary>
    public async Task<T> SendQueryAsync<T>(GraphQLRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _client.SendQueryAsync<T>(request, cancellationToken);
        if (response.Errors != null && response.Errors.Length > 0)
        {
            var first = response.Errors[0];
            var msg = first.Message ?? "Matterport API error.";
            _logger.LogError("Matterport GraphQL error: {Message}", msg);
            ThrowTypedError(first);
        }
        if (response.Data == null)
            throw new MatterportApiException("No data returned.", null);
        return response.Data;
    }

    private static void ThrowTypedError(GraphQLError error)
    {
        var code = error.Extensions != null && error.Extensions.TryGetValue("code", out var c)
            ? c?.ToString()
            : null;
        var message = error.Message ?? "Matterport API error.";
        switch (code?.ToUpperInvariant())
        {
            case "UNAUTHORIZED":
            case "REQUEST.UNAUTHENTICATED":
            case "REQUEST.UNAUTHORIZED":
                throw new MatterportAuthException(message);
            case "NOT_FOUND":
            case "NOT.FOUND":
                throw new MatterportNotFoundException(message);
            case "RATE_LIMIT_EXCEEDED":
            case "REQUEST.RATE.EXCEEDED":
                throw new MatterportRateLimitException(message);
            default:
                throw new MatterportApiException(message, code);
        }
    }
}

/// <summary>GraphQL response wrapper for query { model(id: $id) { ... } }</summary>
public class GetModelGraphResponse
{
    [JsonProperty("model")]
    public ModelGraphData? Model { get; set; }
}

public class ModelGraphData
{
    [JsonProperty("id")]
    public string? Id { get; set; }
    [JsonProperty("name")]
    public string? Name { get; set; }
    [JsonProperty("internalId")]
    public string? InternalId { get; set; }
    [JsonProperty("visibility")]
    public string? Visibility { get; set; }
    [JsonProperty("created")]
    public string? Created { get; set; }
    [JsonProperty("modified")]
    public string? Modified { get; set; }
    [JsonProperty("description")]
    public string? Description { get; set; }
    [JsonProperty("address")]
    public AddressGraphData? Address { get; set; }
    [JsonProperty("geolocation")]
    public GeolocationGraphData? Geolocation { get; set; }
    [JsonProperty("locations")]
    public List<LocationGraphData>? Locations { get; set; }
}

public class AddressGraphData
{
    [JsonProperty("address")]
    public string? Address { get; set; }
    [JsonProperty("locality")]
    public string? Locality { get; set; }
    [JsonProperty("administrativeArea")]
    public string? AdministrativeArea { get; set; }
    [JsonProperty("countryCode")]
    public string? CountryCode { get; set; }
    [JsonProperty("postalCode")]
    public string? PostalCode { get; set; }
}

public class GeolocationGraphData
{
    [JsonProperty("lat")]
    public double? Lat { get; set; }
    [JsonProperty("lng")]
    public double? Lng { get; set; }
}

public class LocationGraphData
{
    [JsonProperty("id")]
    public string? Id { get; set; }
    [JsonProperty("label")]
    public string? Label { get; set; }
    [JsonProperty("floor")]
    public IdWrapper? Floor { get; set; }
    [JsonProperty("room")]
    public IdWrapper? Room { get; set; }
}

public class IdWrapper
{
    [JsonProperty("id")]
    public string? Id { get; set; }
}

/// <summary>GraphQL response for models(query, pageSize, offset)</summary>
public class ListModelsGraphResponse
{
    [JsonProperty("models")]
    public ModelsListGraphData? Models { get; set; }
}

public class ModelsListGraphData
{
    [JsonProperty("nextOffset")]
    public string? NextOffset { get; set; }
    [JsonProperty("results")]
    public List<ModelGraphData>? Results { get; set; }
}
