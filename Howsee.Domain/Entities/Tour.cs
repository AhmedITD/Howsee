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

    public int? UpdatedBy { get; set; }

    public User Owner { get; set; } = null!;
}
