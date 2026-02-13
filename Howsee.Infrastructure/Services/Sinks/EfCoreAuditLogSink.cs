using Howsee.Application.Interfaces;
using Howsee.Application.Interfaces.Sinks;
using Howsee.Domain.Entities;

namespace Howsee.Infrastructure.Services.Sinks;

public class EfCoreAuditLogSink(IHowseeDbContext context) : IAuditLogSink
{
    public async Task WriteLogsAsync(IEnumerable<AuditLog> logs, CancellationToken cancellationToken = default)
    {
        await context.AuditLogs.AddRangeAsync(logs, cancellationToken);
    }
}
