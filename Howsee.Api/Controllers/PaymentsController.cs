using Microsoft.AspNetCore.Mvc;
using Howsee.Api.Common;
using Howsee.Application.DTOs.requests.Payments;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.Interfaces.Invoices;

namespace Howsee.Api.Controllers;

/// <summary>Payment webhooks (e.g. Wayl). No auth - verified by provider.</summary>
[ApiController]
[Route("api/payments")]
public class PaymentsController(IInvoiceService invoiceService) : BaseController
{
    /// <summary>Wayl webhook: called when payment status changes. Expects body with referenceId (invoice id) and status (Complete or Delivered = paid).</summary>
    [HttpPost("webhook/wayl")]
    public async Task<IActionResult> WaylWebhook([FromBody] WaylWebhookPayload payload, CancellationToken cancellationToken)
    {
        var status = payload.Status?.Trim();
        if (status is not "Complete" and not "Delivered")
            return Ok(ApiResponse<object>.SuccessResponse(new { received = true }, "Status not paid."));

        var invoiceId = Guid.Parse(payload.ReferenceId!);
        var result = await invoiceService.MarkAsPaid(invoiceId, cancellationToken);
        return result.Success ? Ok(ApiResponse<object>.SuccessResponse(new { markedPaid = true })) : BadRequest(result);
    }
}
