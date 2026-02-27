using Howsee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Howsee.Application.Interfaces;

public interface IHowseeDbContext
{
    DbSet<Howsee.Domain.Entities.Currency> Currencies { get; set; }
    DbSet<User> Users { get; set; }
    DbSet<RefreshToken> RefreshTokens { get; set; }
    DbSet<AuditLog> AuditLogs { get; set; }
    DbSet<PhoneVerificationCode> PhoneVerificationCodes { get; set; }
    DbSet<Invoice> Invoices { get; set; }
    DbSet<Tour> Tours { get; set; }
    DbSet<Property> Properties { get; set; }
    DbSet<PricingPlan> PricingPlans { get; set; }
    DbSet<Subscription> Subscriptions { get; set; }
    DbSet<Image> Images { get; set; }
    DbSet<PropertyListing> PropertyListings { get; set; }
    DbSet<RentOffer> RentOffers { get; set; }
    DbSet<Save> Saves { get; set; }
    DbSet<PropertyCategoryLookup> PropertyCategories { get; set; }
    DbSet<PropertyImage> PropertyImages { get; set; }
    DbSet<PropertyImageVariant> PropertyImageVariants { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}
