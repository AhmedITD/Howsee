namespace Howsee.Application.Common;

public static class ErrorCodes
{
    public const string PhoneAlreadyExists = "PHONE_ALREADY_EXISTS";
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string InvalidVerificationCode = "INVALID_VERIFICATION_CODE";
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string ResourceNotFound = "RESOURCE_NOT_FOUND";
    public const string InvalidRefreshToken = "INVALID_REFRESH_TOKEN";
    public const string PaymentInitiationFailed = "PAYMENT_INITIATION_FAILED";
    public const string PaymentVerificationFailed = "PAYMENT_VERIFICATION_FAILED";
    public const string PaymentCancellationFailed = "PAYMENT_CANCELLATION_FAILED";
    public const string InvalidWebhookSignature = "INVALID_WEBHOOK_SIGNATURE";
    public const string InternalError = "INTERNAL_ERROR";
}
