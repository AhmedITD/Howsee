using System.Text.Json;
using Howsee.Application.Interfaces;
using Howsee.Application.Interfaces.Auth;
using Howsee.Application.Interfaces.Sinks;
using Howsee.Domain.Entities;

namespace Howsee.Infrastructure.Services.Sinks;

public class AuditEventLogger(IHowseeDbContext context, ICurrentUser currentUser) : IAuditEventLogger
{
    public Task LogAsync(string entityType, string entityId, string action, object? changes = null, CancellationToken cancellationToken = default)
    {
        var log = new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.Id,
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            Changes = changes != null ? JsonSerializer.Serialize(changes) : "{}",
            Timestamp = DateTime.UtcNow
        };
        context.AuditLogs.Add(log);
        return Task.CompletedTask;
    }
}
