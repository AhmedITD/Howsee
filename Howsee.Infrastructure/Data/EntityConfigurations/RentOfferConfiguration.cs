using Howsee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Howsee.Infrastructure.Data.EntityConfigurations;

public class RentOfferConfiguration : IEntityTypeConfiguration<RentOffer>
{
    public void Configure(EntityTypeBuilder<RentOffer> builder)
    {
        builder.ToTable("RentOffers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ListingId).IsRequired();
        builder.Property(x => x.TenantId).IsRequired();
        builder.Property(x => x.OfferedPrice).HasPrecision(18, 2).IsRequired();
        builder.Property(x => x.CurrencyId).IsRequired();
        builder.Property(x => x.DurationMinutes).IsRequired();
        builder.Property(x => x.ProposedStartDate).IsRequired();
        builder.Property(x => x.ProposedEndDate).IsRequired();
        builder.Property(x => x.Status).IsRequired();

        builder.HasOne(x => x.Listing)
            .WithMany()
            .HasForeignKey(x => x.ListingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Tenant)
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Currency)
            .WithMany(c => c.RentOffers)
            .HasForeignKey(x => x.CurrencyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
