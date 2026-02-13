using Howsee.Domain.Entities.Common;
using Howsee.Domain.Interfaces;

namespace Howsee.Domain.Entities;

public class Tour : BaseEntity, IAuditable
{
    public int OwnerId { get; set; }
    public required string Title { get; set; }
    public required string MatterportModelId { get; set; }
    public string? StartSweepId { get; set; }
    public string? PasswordHash { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    /// <summary>Unique token for the share link (e.g. /tour/{ShareToken}). Set on create.</summary>
    public required string ShareToken { get; set; }

    public int? UpdatedBy { get; set; }

    public User Owner { get; set; } = null!;
}
