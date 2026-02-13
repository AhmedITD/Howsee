namespace Howsee.Application.DTOs.requests.Payments;

public class QiCardPaymentRequest
{
    public string RequestId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "IQD";
    public string? FinishPaymentUrl { get; set; }
    public string? NotificationUrl { get; set; }
    public CustomerInfo? CustomerInfo { get; set; }
    public BrowserInfo? BrowserInfo { get; set; }
    public string? Description { get; set; }
}

public class CustomerInfo
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
}

public class BrowserInfo
{
    public string? BrowserAcceptHeader { get; set; }
    public string? BrowserIp { get; set; }
    public bool BrowserJavaEnabled { get; set; }
    public string? BrowserLanguage { get; set; }
    public string? BrowserUserAgent { get; set; }
}
