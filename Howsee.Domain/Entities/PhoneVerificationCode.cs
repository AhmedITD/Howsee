using Howsee.Domain.Entities.Common;
using Howsee.Domain.Enums;

namespace Howsee.Domain.Entities;

public class PhoneVerificationCode : BaseEntity
{
    public required string PhoneNumber { get; set; }
    /// <summary>Plain code (legacy). Prefer verifying via CodeHash.</summary>
    public string? Code { get; set; }
    /// <summary>Hashed OTP code (aligned with docs otp_codes.code_hash).</summary>
    public string? CodeHash { get; set; }
    public OtpPurpose? Purpose { get; set; }
    public DateTime ExpiresAt { get; set; }
    /// <summary>When the code was used (docs: used_at).</summary>
    public DateTime? UsedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public bool IsUsed { get; set; }
    public string? IpAddress { get; set; }

    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
    public bool IsValid() => !IsUsed && !IsExpired();
}
