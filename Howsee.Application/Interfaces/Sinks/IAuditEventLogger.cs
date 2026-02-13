namespace Howsee.Application.Interfaces.Sinks;

public interface IAuditEventLogger
{
    Task LogAsync(string entityType, string entityId, string action, object? changes = null, CancellationToken cancellationToken = default);
}
