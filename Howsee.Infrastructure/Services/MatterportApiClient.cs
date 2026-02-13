using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Howsee.Application.Interfaces.Tours;
using Microsoft.Extensions.Options;

namespace Howsee.Infrastructure.Services;

public class MatterportApiClient : IMatterportApiClient
{
    private const string Endpoint = "https://api.matterport.com/api/models/graph";
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };
    private readonly HttpClient _httpClient;
    private readonly MatterportApiOptions _options;

    public MatterportApiClient(HttpClient httpClient, IOptions<MatterportApiOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    private void SetAuth(HttpRequestMessage request)
    {
        if (!IsConfigured) return;
        var credentials = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{_options.ApiTokenId}:{_options.ApiTokenSecret}"));
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_options.ApiTokenId) &&
        !string.IsNullOrWhiteSpace(_options.ApiTokenSecret);

    /// <summary>
    /// Parses GraphQL errors from response JSON and throws MatterportApiException with first error code/message.
    /// </summary>
    private static void ThrowIfErrors(JsonDocument doc)
    {
        if (!doc.RootElement.TryGetProperty("errors", out var errors) || errors.ValueKind != JsonValueKind.Array)
            return;
        var arr = errors.EnumerateArray();
        if (!arr.MoveNext()) return;
        var first = arr.Current;
        var code = first.TryGetProperty("extensions", out var ext) && ext.TryGetProperty("code", out var c)
            ? c.GetString()
            : null;
        var message = first.TryGetProperty("message", out var m) ? m.GetString() : "Matterport API error.";
        throw new MatterportApiException(code, message ?? "Matterport API error.");
    }

    public async Task<bool> ValidateModelExistsAsync(string modelId, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(modelId))
            return true;

        const string gql = """
            query GetModel($id: ID!) {
                model(id: $id) {
                    id
                }
            }
            """;
        var body = new { query = gql, variables = new { id = modelId } };
        var content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint) { Content = content };
        SetAuth(request);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(json);
        ThrowIfErrors(doc);

        if (!response.IsSuccessStatusCode)
            throw new MatterportApiException(null, $"Matterport API returned {response.StatusCode}.");

        if (doc.RootElement.TryGetProperty("data", out var data) && data.TryGetProperty("model", out var model))
        {
            if (model.ValueKind == JsonValueKind.Null)
                throw new MatterportApiException("not.found", "Model not found or not accessible.");
            return true;
        }
        return false;
    }

    public async Task<MatterportModelListResult> ListModelsAsync(string query = "*", int pageSize = 50, string? offset = null, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured)
            return new MatterportModelListResult { Results = Array.Empty<MatterportModelInfo>(), NextOffset = null };

        var gql = """
            query ListModels($query: String, $pageSize: Int, $offset: String) {
                models(query: $query, pageSize: $pageSize, offset: $offset) {
                    nextOffset
                    results {
                        id
                        name
                        description
                        visibility
                        created
                        modified
                    }
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

        var body = new { query = gql, variables };
        var content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint) { Content = content };
        SetAuth(request);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(json);
        ThrowIfErrors(doc);

        if (!response.IsSuccessStatusCode)
            throw new MatterportApiException(null, $"Matterport API returned {response.StatusCode}.");

        string? nextOffset = null;
        var list = new List<MatterportModelInfo>();

        if (doc.RootElement.TryGetProperty("data", out var data) && data.TryGetProperty("models", out var models))
        {
            if (models.TryGetProperty("nextOffset", out var no) && no.ValueKind != JsonValueKind.Null)
                nextOffset = no.GetString();
            if (models.TryGetProperty("results", out var results) && results.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in results.EnumerateArray())
                {
                    var id = item.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                    if (string.IsNullOrEmpty(id)) continue;
                    var info = new MatterportModelInfo
                    {
                        Id = id,
                        Name = item.TryGetProperty("name", out var nameProp) ? nameProp.GetString() : null,
                        Description = item.TryGetProperty("description", out var descProp) ? descProp.GetString() : null,
                        Visibility = item.TryGetProperty("visibility", out var visProp) ? visProp.GetString() : null
                    };
                    if (item.TryGetProperty("created", out var c) && c.ValueKind != JsonValueKind.Null && DateTime.TryParse(c.GetString(), out var created))
                        info.Created = created;
                    if (item.TryGetProperty("modified", out var m) && m.ValueKind != JsonValueKind.Null && DateTime.TryParse(m.GetString(), out var modified))
                        info.Modified = modified;
                    list.Add(info);
                }
            }
        }
        return new MatterportModelListResult { Results = list, NextOffset = nextOffset };
    }

    public async Task<MatterportModelDetails?> GetModelDetailsAsync(string modelId, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(modelId))
            return null;

        const string gql = """
            query GetModelDetails($id: ID!) {
                model(id: $id) {
                    id
                    name
                    internalId
                    visibility
                    created
                    modified
                    description
                    address { address locality administrativeArea countryCode postalCode }
                }
            }
            """;
        var body = new { query = gql, variables = new { id = modelId } };
        var content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint) { Content = content };
        SetAuth(request);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(json);
        ThrowIfErrors(doc);

        if (!response.IsSuccessStatusCode)
            return null;

        if (!doc.RootElement.TryGetProperty("data", out var data) || !data.TryGetProperty("model", out var model) || model.ValueKind == JsonValueKind.Null)
            return null;

        return ParseModelDetails(model);
    }

    public async Task<IReadOnlyList<MatterportLocationInfo>> GetModelLocationsAsync(string modelId, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured || string.IsNullOrWhiteSpace(modelId))
            return Array.Empty<MatterportLocationInfo>();

        const string gql = """
            query GetModelLocations($id: ID!) {
                model(id: $id) {
                    locations {
                        id
                        label
                        floor { id }
                        room { id }
                    }
                }
            }
            """;
        var body = new { query = gql, variables = new { id = modelId } };
        var content = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Post, Endpoint) { Content = content };
        SetAuth(request);
        var response = await _httpClient.SendAsync(request, cancellationToken);
        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.TryGetProperty("errors", out _))
            return Array.Empty<MatterportLocationInfo>();
        if (!response.IsSuccessStatusCode)
            return Array.Empty<MatterportLocationInfo>();

        var list = new List<MatterportLocationInfo>();
        if (!doc.RootElement.TryGetProperty("data", out var data) || !data.TryGetProperty("model", out var model) || model.ValueKind == JsonValueKind.Null)
            return list;
        if (!model.TryGetProperty("locations", out var locations) || locations.ValueKind != JsonValueKind.Array)
            return list;

        foreach (var loc in locations.EnumerateArray())
        {
            var id = loc.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
            if (string.IsNullOrEmpty(id)) continue;
            var roomId = loc.TryGetProperty("room", out var room) && room.ValueKind != JsonValueKind.Null && room.TryGetProperty("id", out var rid) ? rid.GetString() : null;
            var floorId = loc.TryGetProperty("floor", out var floor) && floor.ValueKind != JsonValueKind.Null && floor.TryGetProperty("id", out var fid) ? fid.GetString() : null;
            list.Add(new MatterportLocationInfo
            {
                Id = id,
                Label = loc.TryGetProperty("label", out var lbl) ? lbl.GetString() : null,
                RoomId = roomId,
                FloorId = floorId
            });
        }
        return list;
    }

    private static MatterportModelDetails ParseModelDetails(JsonElement model)
    {
        var details = new MatterportModelDetails
        {
            Id = model.TryGetProperty("id", out var id) ? id.GetString() ?? "" : "",
            Name = model.TryGetProperty("name", out var n) ? n.GetString() : null,
            InternalId = model.TryGetProperty("internalId", out var iid) ? iid.GetString() : null,
            Visibility = model.TryGetProperty("visibility", out var v) ? v.GetString() : null,
            Description = model.TryGetProperty("description", out var d) ? d.GetString() : null
        };
        if (model.TryGetProperty("created", out var c) && c.ValueKind != JsonValueKind.Null && DateTime.TryParse(c.GetString(), out var created))
            details.Created = created;
        if (model.TryGetProperty("modified", out var m) && m.ValueKind != JsonValueKind.Null && DateTime.TryParse(m.GetString(), out var modified))
            details.Modified = modified;
        if (model.TryGetProperty("address", out var addr) && addr.ValueKind != JsonValueKind.Null)
        {
            details.Address = new MatterportAddress
            {
                Address = addr.TryGetProperty("address", out var a) ? a.GetString() : null,
                Locality = addr.TryGetProperty("locality", out var l) ? l.GetString() : null,
                AdministrativeArea = addr.TryGetProperty("administrativeArea", out var aa) ? aa.GetString() : null,
                CountryCode = addr.TryGetProperty("countryCode", out var cc) ? cc.GetString() : null,
                PostalCode = addr.TryGetProperty("postalCode", out var pc) ? pc.GetString() : null
            };
        }
        if (model.TryGetProperty("geolocation", out var geo) && geo.ValueKind != JsonValueKind.Null)
        {
            details.Geolocation = new MatterportGeoLocation();
            if (geo.TryGetProperty("lat", out var lat) && lat.ValueKind != JsonValueKind.Null)
                details.Geolocation.Lat = lat.GetDouble();
            if (geo.TryGetProperty("lng", out var lng) && lng.ValueKind != JsonValueKind.Null)
                details.Geolocation.Lng = lng.GetDouble();
        }
        return details;
    }
}

public class MatterportApiOptions
{
    public const string SectionName = "Matterport";
    public string ApiTokenId { get; set; } = "";
    public string ApiTokenSecret { get; set; } = "";
}
