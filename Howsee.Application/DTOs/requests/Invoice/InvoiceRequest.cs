using System.Text.Json.Serialization;
using Howsee.Application.DTOs.requests.Payments;

namespace Howsee.Application.DTOs.requests.Invoice;

public class InvoiceRequest
{
    public string? Description { get; set; }
    public required string FinishUrl { get; set; }
    /// <summary>Plan key (e.g. Price_Pro). Amount is resolved from the plan.</summary>
    public required string PlanKey { get; set; }

    [JsonIgnore]
    public BrowserInfo? BrowserInfo { get; set; }
}
