namespace Howsee.Application.DTOs.responses.Payments;

public class QiCardResponse
{
    public bool Success { get; set; }
    public object? Data { get; set; }
    public string? PaymentUrl { get; set; }
    public string? PaymentId { get; set; }
    public string? Error { get; set; }
    public string? ErrorCode { get; set; }
    public int? StatusCode { get; set; }
}
