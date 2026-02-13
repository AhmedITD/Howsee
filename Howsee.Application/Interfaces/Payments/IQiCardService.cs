using Howsee.Application.DTOs.requests.Payments;
using Howsee.Application.DTOs.responses.Payments;

namespace Howsee.Application.Interfaces.Payments;

public interface IQiCardService
{
    Task<QiCardResponse> InitiatePaymentAsync(QiCardPaymentRequest request, CancellationToken cancellationToken = default);
    Task<QiCardResponse> VerifyPaymentAsync(string paymentId, CancellationToken cancellationToken = default);
    Task<QiCardResponse> CancelPaymentAsync(string paymentId);
    bool VerifyWebhookSignature(string rawBody, string? signatureFromHeader);
}
