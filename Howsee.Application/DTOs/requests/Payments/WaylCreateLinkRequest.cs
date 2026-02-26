namespace Howsee.Application.DTOs.requests.Payments;

/// <summary>Request to create a Wayl payment link (POST /api/v1/links).</summary>
public class WaylCreateLinkRequest
{ 
    public required string ReferenceId { get; set; }
    public decimal Total { get; set; }
    public string Currency { get; set; } = "IQD";
    public string? WebhookUrl { get; set; }
    public string? WebhookSecret { get; set; }
    public string? RedirectionUrl { get; set; }
}
