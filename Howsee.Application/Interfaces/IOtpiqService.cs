using Howsee.Application.DTOs.responses.Otpiq;

namespace Howsee.Application.Interfaces;

public interface IOtpiqService
{
    Task<OtpiqResponse> SendVerificationCodeAsync(
        string phoneNumber,
        string verificationCode,
        string smsType = "verification",
        string provider = "whatsapp-sms");
}
