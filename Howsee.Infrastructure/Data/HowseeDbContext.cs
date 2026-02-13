using System.Linq.Expressions;
using Howsee.Application.Interfaces;
using Howsee.Domain.Entities;
using Howsee.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Howsee.Infrastructure.Data;

public class HowseeDbContext : DbContext, IHowseeDbContext
{
    public HowseeDbContext(DbContextOptions<HowseeDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<PhoneVerificationCode> PhoneVerificationCodes { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Tour> Tours { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HowseeDbContext).Assembly);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType)) continue;
            modelBuilder.Entity(entityType.ClrType)
                .Property("IsDeleted")
                .HasDefaultValue(false);

            var parameter = Expression.Parameter(entityType.ClrType, "e");
            var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
            var falseConstant = Expression.Constant(false);
            var binaryExpression = Expression.Equal(property, falseConstant);
            var filter = Expression.Lambda(binaryExpression, parameter);

            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);

            foreach (var index in entityType.GetIndexes())
            {
                if (string.IsNullOrEmpty(index.GetFilter()))
                    index.SetFilter("\"IsDeleted\" = false");
            }
        }
    }
}
