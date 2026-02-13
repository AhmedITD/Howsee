using System.Text.Json.Serialization;
using Howsee.Application.DTOs.requests.Payments;

namespace Howsee.Application.DTOs.requests.Invoice;

public class InvoiceRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IQD";
    public string? Description { get; set; }
    public required string FinishUrl { get; set; }

    [JsonIgnore]
    public BrowserInfo? BrowserInfo { get; set; }
}
