using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Howsee.Api.Common;
using Howsee.Application.DTOs.requests.Invoice;
using Howsee.Application.DTOs.requests.Payments;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Invoice;
using Howsee.Application.Interfaces.Invoices;

namespace Howsee.Api.Controllers;

[ApiController]
[Route("invoices")]
public class InvoiceController(IInvoiceService invoiceService) : BaseController
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<InvoiceResponse>>> CreateInvoice([FromBody] InvoiceRequest request, CancellationToken cancellationToken = default)
    {
        request.BrowserInfo = HttpContext.BuildBrowserInfo();
        var result = await invoiceService.CreateInvoice(request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize("Administrator")]
    [HttpPost("{id:guid}/mark-paid")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsPaid(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await invoiceService.MarkAsPaid(id, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
