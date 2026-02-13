using Howsee.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Howsee.Application.Interfaces;

public interface IHowseeDbContext
{
    DbSet<User> Users { get; set; }
    DbSet<RefreshToken> RefreshTokens { get; set; }
    DbSet<AuditLog> AuditLogs { get; set; }
    DbSet<PhoneVerificationCode> PhoneVerificationCodes { get; set; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    int SaveChanges();
}
