namespace Howsee.Application.DTOs.responses.Payments;

/// <summary>Response from Wayl create-link or get-link. Aligns with Wayl API /api/v1/links response data.</summary>
public class WaylLinkResponse
{
    public bool Success { get; set; }
    public string? PaymentUrl { get; set; }
    public string? LinkId { get; set; }
    public string? ReferenceId { get; set; }
    public string? Status { get; set; }
    public string? Error { get; set; }
    public int? StatusCode { get; set; }
}
