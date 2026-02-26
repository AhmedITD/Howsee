using Howsee.Application.DTOs.requests.Payments;
using Howsee.Application.DTOs.responses.Payments;

namespace Howsee.Application.Interfaces.Payments;

/// <summary>Wayl payment gateway: create payment links, invalidate pending links, and handle webhooks.</summary>
public interface IWaylPaymentService
{
    /// <summary>Create a payment link (POST /api/v1/links). Returns the payment URL and link id.</summary>
    Task<WaylLinkResponse> CreateLinkAsync(WaylCreateLinkRequest request, CancellationToken cancellationToken = default);

    /// <summary>Invalidate a payment link if it is still pending (POST /api/v1/links/{referenceId}/invalidate-if-pending).</summary>
    Task<WaylLinkResponse> InvalidateLinkIfPendingAsync(string referenceId, CancellationToken cancellationToken = default);
}
