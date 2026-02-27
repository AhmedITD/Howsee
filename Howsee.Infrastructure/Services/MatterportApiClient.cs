using GraphQL;
using Howsee.Application.Interfaces.Tours;
using Microsoft.Extensions.Options;

namespace Howsee.Infrastructure.Services;

/// <summary>
/// Wraps Matterport Model API via GraphQL (using MatterportModelClient). Aligns with spec: no direct HttpClient.
/// </summary>
public class MatterportApiClient : IMatterportApiClient
{
    private readonly MatterportModelClient _modelClient;
    private readonly MatterportApiOptions _options;

    public MatterportApiClient(MatterportModelClient modelClient, IOptions<MatterportApiOptions> options)
    {
        _modelClient = modelClient;
        _options = options.Value;
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.ApiToken) ||
        (!string.IsNullOrWhiteSpace(_options.ApiTokenId) && !string.IsNullOrWhiteSpace(_options.ApiTokenSecret));

    public async Task<bool> ValidateModelExistsAsync(string modelId, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(modelId))
            return true;

        const string gql = """
            query GetModel($id: ID!) {
                model(id: $id) { id }
            }
            """;
        var request = new GraphQLRequest
        {
            Query = gql,
            Variables = new { id = modelId }
        };
        try
        {
            var data = await _modelClient.SendQueryAsync<GetModelGraphResponse>(request, cancellationToken);
            if (data?.Model == null)
                throw new MatterportNotFoundException("Model not found or not accessible.");
            return true;
        }
        catch (MatterportNotFoundException)
        {
            throw;
        }
        catch (MatterportApiException)
        {
            throw;
        }
    }

    public async Task<MatterportModelListResult> ListModelsAsync(string query = "*", int pageSize = 50, string? offset = null, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            return new MatterportModelListResult { Results = Array.Empty<MatterportModelInfo>(), NextOffset = null };

        const string gql = """
            query ListModels($query: String, $pageSize: Int, $offset: String) {
                models(query: $query, pageSize: $pageSize, offset: $offset) {
                    nextOffset
                    results { id name description visibility created modified }
                }
            }
            """;
        var variables = new Dictionary<string, object?>
        {
            ["query"] = string.IsNullOrWhiteSpace(query) ? "*" : query,
            ["pageSize"] = Math.Clamp(pageSize, 1, 1000)
        };
        if (!string.IsNullOrEmpty(offset))
            variables["offset"] = offset;

        var request = new GraphQLRequest { Query = gql, Variables = variables };
        var data = await _modelClient.SendQueryAsync<ListModelsGraphResponse>(request, cancellationToken);
        var list = new List<MatterportModelInfo>();
        if (data?.Models?.Results != null)
        {
            foreach (var r in data.Models.Results)
            {
                if (string.IsNullOrEmpty(r?.Id)) continue;
                list.Add(new MatterportModelInfo
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    Visibility = r.Visibility,
                    Created = ParseDateTime(r.Created),
                    Modified = ParseDateTime(r.Modified)
                });
            }
        }
        return new MatterportModelListResult
        {
            Results = list,
            NextOffset = data?.Models?.NextOffset
        };
    }

    public async Task<MatterportModelDetails?> GetModelDetailsAsync(string modelId, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(modelId))
            return null;

        const string gql = """
            query GetModelDetails($id: ID!) {
                model(id: $id) {
                    id name internalId visibility created modified description
                    address { address locality administrativeArea countryCode postalCode }
                    geolocation { lat lng }
                }
            }
            """;
        var request = new GraphQLRequest { Query = gql, Variables = new { id = modelId } };
        try
        {
            var data = await _modelClient.SendQueryAsync<GetModelGraphResponse>(request, cancellationToken);
            if (data?.Model == null) return null;
            return ToModelDetails(data.Model);
        }
        catch (MatterportNotFoundException)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<MatterportLocationInfo>> GetModelLocationsAsync(string modelId, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(modelId))
            return Array.Empty<MatterportLocationInfo>();

        const string gql = """
            query GetModelLocations($id: ID!) {
                model(id: $id) {
                    locations { id label floor { id } room { id } }
                }
            }
            """;
        var request = new GraphQLRequest { Query = gql, Variables = new { id = modelId } };
        try
        {
            var data = await _modelClient.SendQueryAsync<GetModelGraphResponse>(request, cancellationToken);
            if (data?.Model?.Locations == null || data.Model.Locations.Count == 0)
                return Array.Empty<MatterportLocationInfo>();
            return data.Model.Locations
                .Where(l => !string.IsNullOrEmpty(l?.Id))
                .Select(l => new MatterportLocationInfo
                {
                    Id = l!.Id!,
                    Label = l.Label,
                    FloorId = l.Floor?.Id,
                    RoomId = l.Room?.Id
                })
                .ToList();
        }
        catch (MatterportApiException)
        {
            return Array.Empty<MatterportLocationInfo>();
        }
    }

    private static DateTime? ParseDateTime(string? s)
    {
        if (string.IsNullOrEmpty(s)) return null;
        return DateTime.TryParse(s, out var d) ? d : null;
    }

    private static MatterportModelDetails ToModelDetails(ModelGraphData m)
    {
        var details = new MatterportModelDetails
        {
            Id = m.Id ?? "",
            Name = m.Name,
            InternalId = m.InternalId,
            Visibility = m.Visibility,
            Description = m.Description,
            Created = ParseDateTime(m.Created),
            Modified = ParseDateTime(m.Modified)
        };
        if (m.Address != null)
        {
            details.Address = new MatterportAddress
            {
                Address = m.Address.Address,
                Locality = m.Address.Locality,
                AdministrativeArea = m.Address.AdministrativeArea,
                CountryCode = m.Address.CountryCode,
                PostalCode = m.Address.PostalCode
            };
        }
        if (m.Geolocation != null)
        {
            details.Geolocation = new MatterportGeoLocation
            {
                Lat = m.Geolocation.Lat,
                Lng = m.Geolocation.Lng
            };
        }
        return details;
    }
}

public class MatterportApiOptions
{
    public const string SectionName = "Matterport";

    /// <summary>Optional. If set, used as Bearer token. Otherwise Basic auth with ApiTokenId:ApiTokenSecret is used.</summary>
    public string ApiToken { get; set; } = "";

    public string ApiTokenId { get; set; } = "";
    public string ApiTokenSecret { get; set; } = "";

    /// <summary>Model API GraphQL endpoint. Default: https://api.matterport.com/api/models/graph</summary>
    public string ModelApiEndpoint { get; set; } = "https://api.matterport.com/api/models/graph";

    /// <summary>Account API GraphQL endpoint (for future use).</summary>
    public string AccountApiEndpoint { get; set; } = "https://api.matterport.com/api/accounts/graph";

    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetries { get; set; } = 3;

    /// <summary>SDK application key returned to frontend for the 3D viewer. Not used for server-side API calls.</summary>
    public string ApplicationKey { get; set; } = "";
}
