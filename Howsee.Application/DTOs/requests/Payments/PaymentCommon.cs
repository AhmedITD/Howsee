namespace Howsee.Application.DTOs.requests.Payments;

public class BrowserInfo
{
    public string? BrowserAcceptHeader { get; set; }
    public string? BrowserIp { get; set; }
    public bool BrowserJavaEnabled { get; set; }
    public string? BrowserLanguage { get; set; }
    public string? BrowserUserAgent { get; set; }
}
