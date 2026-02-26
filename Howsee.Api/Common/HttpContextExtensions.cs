using Howsee.Application.DTOs.requests.Payments;
using Microsoft.AspNetCore.Http;

namespace Howsee.Api.Common;

public static class HttpContextExtensions
{
    public static BrowserInfo BuildBrowserInfo(this HttpContext context)
    {
        var request = context.Request;
        return new BrowserInfo
        {
            BrowserUserAgent = request.Headers["User-Agent"].FirstOrDefault(),
            BrowserAcceptHeader = request.Headers["Accept"].FirstOrDefault(),
            BrowserLanguage = request.Headers["Accept-Language"].FirstOrDefault(),
            BrowserIp = request.Headers["X-Forwarded-For"].FirstOrDefault(),
            BrowserJavaEnabled = false
        };
    }
}
