using Howsee.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Howsee.Infrastructure.Data.EntityConfigurations;

public class TourConfiguration : IEntityTypeConfiguration<Tour>
{
    public void Configure(EntityTypeBuilder<Tour> builder)
    {
        builder.ToTable("Tours");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OwnerId).IsRequired();
        builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MatterportModelId).HasMaxLength(100).IsRequired();
        builder.Property(x => x.StartSweepId).HasMaxLength(100);
        builder.Property(x => x.PasswordHash).HasMaxLength(500);
        builder.Property(x => x.IsActive).IsRequired();
        builder.Property(x => x.ShareToken).HasMaxLength(64).IsRequired();
        builder.HasIndex(x => x.ShareToken).IsUnique();

        builder.HasOne(x => x.Owner)
            .WithMany(u => u.Tours)
            .HasForeignKey(x => x.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
