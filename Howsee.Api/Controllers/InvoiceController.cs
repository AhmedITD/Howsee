using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Howsee.Api.Common;
using Howsee.Application.DTOs.requests.Invoice;
using Howsee.Application.DTOs.requests.Payments;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Invoice;
using Howsee.Application.Interfaces.Invoices;

namespace Howsee.Api.Controllers;

[Route("invoices")]
public class InvoiceController(IInvoiceService invoiceService) : BaseController
{
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<InvoiceResponse>>> CreateInvoice([FromBody] InvoiceRequest request, CancellationToken cancellationToken = default)
    {
        request.BrowserInfo = BuildBrowserInfoFromContext(HttpContext);
        var result = await invoiceService.CreateInvoice(request, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [Authorize]
    [HttpPost("{id:guid}/mark-paid")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsPaid(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await invoiceService.MarkAsPaid(id, cancellationToken);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private static BrowserInfo BuildBrowserInfoFromContext(HttpContext context)
    {
        var request = context.Request;
        return new BrowserInfo
        {
            BrowserUserAgent = request.Headers["User-Agent"].FirstOrDefault() ?? "Mozilla/5.0",
            BrowserAcceptHeader = request.Headers["Accept"].FirstOrDefault() ?? "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
            BrowserLanguage = request.Headers["Accept-Language"].FirstOrDefault() ?? "en-US",
            BrowserIp = request.Headers["X-Forwarded-For"].FirstOrDefault() ?? context.Connection.RemoteIpAddress?.ToString(),
            BrowserJavaEnabled = false
        };
    }
}
