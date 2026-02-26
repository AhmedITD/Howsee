using Howsee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Howsee.Infrastructure.Data.EntityConfigurations;

public class PropertyCategoryConfiguration : IEntityTypeConfiguration<PropertyCategoryLookup>
{
    public void Configure(EntityTypeBuilder<PropertyCategoryLookup> builder)
    {
        builder.ToTable("PropertyCategories");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
    }
}
