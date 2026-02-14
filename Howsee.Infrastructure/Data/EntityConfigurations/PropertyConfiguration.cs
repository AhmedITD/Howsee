using Howsee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Howsee.Infrastructure.Data.EntityConfigurations;

public class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("Properties");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OwnerId).IsRequired();
        builder.Property(x => x.Category).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(2000);
        builder.Property(x => x.Area).HasPrecision(18, 2);
        builder.Property(x => x.Price).HasPrecision(18, 2);
        builder.Property(x => x.Active).IsRequired();

        builder.OwnsOne(x => x.Address, a =>
        {
            a.Property(x => x.Address).HasMaxLength(200);
            a.Property(x => x.Locality).HasMaxLength(100);
            a.Property(x => x.AdministrativeArea).HasMaxLength(100);
            a.Property(x => x.CountryCode).HasMaxLength(10);
            a.Property(x => x.PostalCode).HasMaxLength(20);
        });

        builder.HasOne(x => x.Owner)
            .WithMany(u => u.Properties)
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Tour)
            .WithMany()
            .HasForeignKey(x => x.TourId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.TourId).IsUnique().HasFilter("\"TourId\" IS NOT NULL");
    }
}
