using System.Security.Cryptography;
using Howsee.Application.Interfaces.Tours;
using Microsoft.Extensions.Options;

namespace Howsee.Infrastructure.Services;

public class TourLinkTokenService(IOptions<TourLinkOptions> options) : ITourLinkTokenService
{
    public string Generate(int tourId)
    {
        var key = options.Value.SigningKey;
        if (string.IsNullOrEmpty(key))
            throw new InvalidOperationException("TourLink:SigningKey is not configured.");

        var payload = tourId.ToString();
        var payloadBytes = System.Text.Encoding.UTF8.GetBytes(payload);
        var payloadB64 = Base64UrlEncode(payloadBytes);
        var keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payloadB64));
        var sig = Base64UrlEncode(hash);
        return payloadB64 + "." + sig;
    }

    public int? Validate(string token)
    {
        if (string.IsNullOrEmpty(token)) return null;
        var parts = token.Split('.');
        if (parts.Length != 2) return null;

        var key = options.Value.SigningKey;
        if (string.IsNullOrEmpty(key)) return null;

        var payloadB64 = parts[0];
        var sigB64 = parts[1];
        var keyBytes = System.Text.Encoding.UTF8.GetBytes(key);
        using var hmac = new HMACSHA256(keyBytes);
        var expectedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(payloadB64));
        var expectedSig = Base64UrlEncode(expectedHash);
        if (expectedSig != sigB64) return null;

        try
        {
            var payloadBytes = Base64UrlDecode(payloadB64);
            var payload = System.Text.Encoding.UTF8.GetString(payloadBytes);
            return int.TryParse(payload, out var id) ? id : null;
        }
        catch
        {
            return null;
        }
    }

    private static string Base64UrlEncode(byte[] data)
    {
        var b64 = Convert.ToBase64String(data);
        return b64.TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static byte[] Base64UrlDecode(string input)
    {
        var b64 = input.Replace('-', '+').Replace('_', '/');
        switch (b64.Length % 4)
        {
            case 2: b64 += "=="; break;
            case 3: b64 += "="; break;
        }
        return Convert.FromBase64String(b64);
    }
}

public class TourLinkOptions
{
    public const string SectionName = "TourLink";
    public string SigningKey { get; set; } = "";
}
