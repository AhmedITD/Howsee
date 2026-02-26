namespace Howsee.Application.DTOs.requests.Payments;

/// <summary>Payload from Wayl when payment status changes.</summary>
public class WaylWebhookPayload
{
    public string? ReferenceId { get; set; }
    public string? Status { get; set; }
    public string? OrderId { get; set; }
}
