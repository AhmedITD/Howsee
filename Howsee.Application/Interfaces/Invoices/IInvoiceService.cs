using Howsee.Application.DTOs.requests.Invoice;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Invoice;

namespace Howsee.Application.Interfaces.Invoices;

public interface IInvoiceService
{
    Task<ApiResponse<InvoiceResponse>> CreateInvoice(InvoiceRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> MarkAsPaid(Guid invoiceId, CancellationToken cancellationToken = default);
    Task CancelOtherPendingPaymentsForUser(Guid paidInvoiceId, CancellationToken cancellationToken = default);
}
