namespace Howsee.Application.Interfaces.Tours;

public interface IMatterportApiClient
{
    /// <summary>
    /// Returns true if the client is configured (has token and secret).
    /// </summary>
    bool IsConfigured { get; }

    /// <summary>
    /// Validates that a Matterport model exists and is accessible. Returns true if found.
    /// Throws <see cref="MatterportApiException"/> on API error (e.g. not.found, request.unauthorized).
    /// </summary>
    Task<bool> ValidateModelExistsAsync(string modelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists models with cursor-based pagination. Use <paramref name="offset"/> from previous response's NextOffset for next page.
    /// Throws <see cref="MatterportApiException"/> on API error.
    /// </summary>
    Task<MatterportModelListResult> ListModelsAsync(string query = "*", int pageSize = 50, string? offset = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets model details (id, name, visibility, address, etc.). Throws <see cref="MatterportApiException"/> if not found or on API error.
    /// </summary>
    Task<MatterportModelDetails?> GetModelDetailsAsync(string modelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets locations (sweeps/waypoints) for a model for StartSweepId dropdown. Returns empty if model not found or not configured.
    /// </summary>
    Task<IReadOnlyList<MatterportLocationInfo>> GetModelLocationsAsync(string modelId, CancellationToken cancellationToken = default);
}

public class MatterportModelInfo
{
    public required string Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Visibility { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? Modified { get; set; }
}

/// <summary>
/// Cursor-based list result; pass NextOffset as offset for the next page (null when last page).
/// </summary>
public class MatterportModelListResult
{
    public required IReadOnlyList<MatterportModelInfo> Results { get; set; }
    public string? NextOffset { get; set; }
}

public class MatterportModelDetails
{
    public required string Id { get; set; }
    public string? Name { get; set; }
    public string? InternalId { get; set; }
    public string? Visibility { get; set; }
    public DateTime? Created { get; set; }
    public DateTime? Modified { get; set; }
    public string? Description { get; set; }
    public MatterportAddress? Address { get; set; }
    public MatterportGeoLocation? Geolocation { get; set; }
}

public class MatterportAddress
{
    public string? Address { get; set; }
    public string? Locality { get; set; }
    public string? AdministrativeArea { get; set; }
    public string? CountryCode { get; set; }
    public string? PostalCode { get; set; }
}

public class MatterportGeoLocation
{
    public double? Lat { get; set; }
    public double? Lng { get; set; }
}

public class MatterportLocationInfo
{
    public required string Id { get; set; }
    public string? Label { get; set; }
    public string? RoomId { get; set; }
    public string? FloorId { get; set; }
}
