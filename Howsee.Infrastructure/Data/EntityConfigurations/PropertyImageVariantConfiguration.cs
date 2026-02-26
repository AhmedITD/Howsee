using Howsee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Howsee.Infrastructure.Data.EntityConfigurations;

public class PropertyImageVariantConfiguration : IEntityTypeConfiguration<PropertyImageVariant>
{
    public void Configure(EntityTypeBuilder<PropertyImageVariant> builder)
    {
        builder.ToTable("PropertyImageVariants");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PropertyImageId).IsRequired();
        builder.Property(x => x.Resolution).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Url).IsRequired();

        builder.HasOne(x => x.PropertyImage)
            .WithMany(pi => pi.Variants)
            .HasForeignKey(x => x.PropertyImageId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.PropertyImageId, x.Resolution })
            .IsUnique()
            .HasDatabaseName("uniq_property_image_id_resolution");
    }
}
