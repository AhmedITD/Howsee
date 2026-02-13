using Howsee.Application.DTOs.responses.Pricing;
using Howsee.Application.Interfaces;
using Howsee.Application.Interfaces.Subscriptions;
using Howsee.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Howsee.Application.Services;

public class SubscriptionService(IHowseeDbContext dbContext) : ISubscriptionService
{
    public async Task<SubscriptionDto?> GetCurrentSubscriptionAsync(int userId, CancellationToken cancellationToken = default)
    {
        var today = DateTime.UtcNow.Date;

        var sub = await dbContext.Subscriptions
            .AsNoTracking()
            .Where(s => s.UserId == userId && s.Status == SubscriptionStatus.Active && s.EndDate >= today)
            .OrderByDescending(s => s.EndDate)
            .Select(s => new SubscriptionDto
            {
                Id = s.Id,
                PlanKey = s.PricingPlan.Key,
                PlanName = s.PricingPlan.Name,
                EndDate = s.EndDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        return sub;
    }
}
