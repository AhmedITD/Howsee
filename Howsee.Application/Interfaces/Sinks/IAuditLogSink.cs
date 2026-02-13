using Howsee.Domain.Entities;

namespace Howsee.Application.Interfaces.Sinks;

public interface IAuditLogSink
{
    Task WriteLogsAsync(IEnumerable<AuditLog> logs, CancellationToken cancellationToken = default);
}
