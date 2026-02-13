using System.Text.Json.Serialization;
using Howsee.Domain.Entities.Common;
using Howsee.Domain.Enums;
using Howsee.Domain.Interfaces;

namespace Howsee.Domain.Entities;

public class User : BaseEntity, IAuditable, ISoftDeletable
{
    public required string FullName { get; set; }
    public required string Phone { get; set; }

    [JsonIgnore]
    public string PasswordHash { get; set; } = null!;
    public UserRole Role { get; set; }

    public int? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    public ICollection<Tour> Tours { get; set; } = new List<Tour>();
    public ICollection<Property> Properties { get; set; } = new List<Property>();
}
