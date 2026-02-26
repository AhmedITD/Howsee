using System.Text;
using System.Text.Json;
using Howsee.Application.DTOs.requests.Payments;
using Howsee.Application.DTOs.responses.Payments;
using Howsee.Application.Interfaces.Payments;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Howsee.Infrastructure.Services;

public class WaylPaymentService : IWaylPaymentService
{
    private const string LinksPath = "api/v1/links";
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WaylPaymentService> _logger;
    private readonly string _baseUrl;
    private readonly string _apiKey;

    public WaylPaymentService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<WaylPaymentService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _baseUrl = _configuration["Wayl:BaseUrl"]?.TrimEnd('/') ?? "https://api.thewayl.com";
        _apiKey = _configuration["Wayl:ApiKey"] ?? string.Empty;
    }

    public async Task<WaylLinkResponse> CreateLinkAsync(WaylCreateLinkRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                referenceId = request.ReferenceId,
                total = (double)request.Total,
                currency = request.Currency,
                webhookUrl = request.WebhookUrl,
                webhookSecret = request.WebhookSecret,
                redirectionUrl = request.RedirectionUrl
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-WAYL-AUTHENTICATION", _apiKey);

            var response = await _httpClient.PostAsync($"{_baseUrl}/{LinksPath}", content, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;
                if (root.TryGetProperty("data", out var data))
                {
                    var url = GetString(data, "url");
                    var id = GetString(data, "id");
                    var referenceId = GetString(data, "referenceId");
                    var status = GetString(data, "status");

                    _logger.LogInformation("Wayl link created. ReferenceId: {ReferenceId}, LinkId: {LinkId}", referenceId, id);

                    return new WaylLinkResponse
                    {
                        Success = true,
                        PaymentUrl = url,
                        LinkId = id,
                        ReferenceId = referenceId,
                        Status = status
                    };
                }
            }

            var errorMessage = "Payment link creation failed";
            try
            {
                var errDoc = JsonDocument.Parse(responseBody);
                if (errDoc.RootElement.TryGetProperty("message", out var msg))
                    errorMessage = msg.GetString() ?? errorMessage;
            }
            catch { /* ignore */ }

            _logger.LogError("Wayl CreateLink failed. Status: {Status}, Response: {Response}", response.StatusCode, responseBody);
            return new WaylLinkResponse
            {
                Success = false,
                Error = errorMessage,
                StatusCode = (int)response.StatusCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Wayl CreateLink exception");
            return new WaylLinkResponse
            {
                Success = false,
                Error = ex.Message
            };
        }
    }

    public async Task<WaylLinkResponse> InvalidateLinkIfPendingAsync(string referenceId, CancellationToken cancellationToken = default)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-WAYL-AUTHENTICATION", _apiKey);

            var response = await _httpClient.PostAsync(
                $"{_baseUrl}/{LinksPath}/{Uri.EscapeDataString(referenceId)}/invalidate-if-pending",
                null,
                cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Wayl link invalidated (if pending). ReferenceId: {ReferenceId}", referenceId);
                return new WaylLinkResponse { Success = true, ReferenceId = referenceId };
            }

            var errorMessage = "Invalidate failed";
            try
            {
                var errDoc = JsonDocument.Parse(responseBody);
                if (errDoc.RootElement.TryGetProperty("message", out var msg))
                    errorMessage = msg.GetString() ?? errorMessage;
            }
            catch { /* ignore */ }

            _logger.LogWarning("Wayl InvalidateIfPending failed. ReferenceId: {ReferenceId}, Status: {Status}", referenceId, response.StatusCode);
            return new WaylLinkResponse
            {
                Success = false,
                Error = errorMessage,
                StatusCode = (int)response.StatusCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Wayl InvalidateIfPending exception");
            return new WaylLinkResponse { Success = false, Error = ex.Message };
        }
    }

    private static string? GetString(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var prop))
            return prop.GetString();
        return null;
    }
}
