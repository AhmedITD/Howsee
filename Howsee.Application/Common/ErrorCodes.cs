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
    public const string InvoiceNotFound = "INVOICE_NOT_FOUND";
    public const string TourNotFound = "TOUR_NOT_FOUND";
    public const string TourAccessDenied = "TOUR_ACCESS_DENIED";
    public const string TourExpired = "TOUR_EXPIRED";
    public const string InvalidTourToken = "INVALID_TOUR_TOKEN";
    public const string InvalidMatterportModel = "INVALID_MATTERPORT_MODEL";
    /// <summary>Matterport model exists but is locked; unlock in Matterport account to access.</summary>
    public const string MatterportModelLocked = "MATTERPORT_MODEL_LOCKED";
    public const string PropertyNotFound = "PROPERTY_NOT_FOUND";
}
