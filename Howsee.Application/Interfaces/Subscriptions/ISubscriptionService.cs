using Howsee.Application.DTOs.responses.Pricing;

namespace Howsee.Application.Interfaces.Subscriptions;

public interface ISubscriptionService
{
    Task<SubscriptionDto?> GetCurrentSubscriptionAsync(int userId, CancellationToken cancellationToken = default);
}
