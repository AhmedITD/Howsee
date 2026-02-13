using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Howsee.Api.Common;
using Howsee.Application.Common;
using Howsee.Application.DTOs.requests.Payments;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Payments;
using Howsee.Application.Interfaces.Payments;

namespace Howsee.Api.Controllers;

[Route("payments")]
public class PaymentsController(IQiCardService qiCardService) : BaseController
{
    [HttpPost("initiate")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<QiCardResponse>>> InitiatePayment([FromBody] QiCardPaymentRequest request, CancellationToken cancellationToken = default)
    {
        var result = await qiCardService.InitiatePaymentAsync(request, cancellationToken);
        return result.Success ? Ok(ApiResponse<QiCardResponse>.SuccessResponse(result)) : BadRequest(ApiResponse<QiCardResponse>.ErrorResponse(result.Error ?? "Payment initiation failed", code: ErrorCodes.PaymentInitiationFailed));
    }

    [HttpGet("{paymentId}/verify")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<QiCardResponse>>> VerifyPayment(string paymentId, CancellationToken cancellationToken = default)
    {
        var result = await qiCardService.VerifyPaymentAsync(paymentId, cancellationToken);
        return result.Success ? Ok(ApiResponse<QiCardResponse>.SuccessResponse(result)) : BadRequest(ApiResponse<QiCardResponse>.ErrorResponse(result.Error ?? "Verification failed", code: ErrorCodes.PaymentVerificationFailed));
    }

    [HttpPost("{paymentId}/cancel")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<QiCardResponse>>> CancelPayment(string paymentId)
    {
        var result = await qiCardService.CancelPaymentAsync(paymentId);
        return result.Success ? Ok(ApiResponse<QiCardResponse>.SuccessResponse(result)) : BadRequest(ApiResponse<QiCardResponse>.ErrorResponse(result.Error ?? "Cancellation failed", code: ErrorCodes.PaymentCancellationFailed));
    }

    [HttpPost("webhook/qicard")]
    public async Task<IActionResult> QiCardWebhook(CancellationToken cancellationToken = default)
    {
        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(cancellationToken);
        Request.Body.Position = 0;

        var signature = Request.Headers["X-Signature"].FirstOrDefault();
        if (!qiCardService.VerifyWebhookSignature(rawBody, signature))
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid webhook signature.", code: ErrorCodes.InvalidWebhookSignature));

        return Ok();
    }
}
