using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Howsee.Application.Common;
using Howsee.Application.DTOs.requests.Invoice;
using Howsee.Application.DTOs.requests.Payments;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Invoice;
using Howsee.Application.Interfaces;
using Howsee.Application.Interfaces.Auth;
using Howsee.Application.Interfaces.Invoices;
using Howsee.Application.Interfaces.Payments;
using Howsee.Domain.Entities;
using Howsee.Domain.Enums;

namespace Howsee.Application.Services;

public class InvoiceService(
    IHowseeDbContext dbContext,
    ICurrentUser currentUser,
    IQiCardService qiCardService,
    IConfiguration configuration) : IInvoiceService
{
    public async Task<ApiResponse<InvoiceResponse>> CreateInvoice(InvoiceRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Amount <= 0)
            return ApiResponse<InvoiceResponse>.ErrorResponse("Amount must be greater than zero.", code: ErrorCodes.ValidationFailed);

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.Id,
            TotalAmount = request.Amount,
            Currency = request.Currency ?? "IQD",
            Description = request.Description,
            Status = InvoiceStatus.Draft
        };

        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);

        var user = await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == currentUser.Id)
            .Select(u => new { u.FullName })
            .FirstOrDefaultAsync(cancellationToken);

        var customerInfo = new CustomerInfo
        {
            FirstName = user?.FullName ?? "Customer",
            LastName = null,
            Email = null
        };

        var appUrl = configuration["APP_URL"]?.TrimEnd('/') ?? "";
        var notificationUrl = string.IsNullOrEmpty(appUrl)
            ? null
            : $"{appUrl}/payments/webhook/qicard";

        var qiCardPaymentRequest = new QiCardPaymentRequest
        {
            RequestId = invoice.Id.ToString(),
            Amount = invoice.TotalAmount,
            Currency = invoice.Currency,
            FinishPaymentUrl = request.FinishUrl,
            NotificationUrl = notificationUrl,
            CustomerInfo = customerInfo,
            BrowserInfo = request.BrowserInfo,
            Description = invoice.Description
        };

        var qiResponse = await qiCardService.InitiatePaymentAsync(qiCardPaymentRequest, cancellationToken);

        if (!qiResponse.Success)
            return ApiResponse<InvoiceResponse>.ErrorResponse(qiResponse.Error ?? "Payment initiation failed.", code: ErrorCodes.PaymentInitiationFailed);

        invoice.QiPaymentId = qiResponse.PaymentId;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<InvoiceResponse>.SuccessResponse(new InvoiceResponse
        {
            InvoiceId = invoice.Id,
            PaymentUrl = qiResponse.PaymentUrl ?? string.Empty
        });
    }

    public async Task<ApiResponse<bool>> MarkAsPaid(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await dbContext.Invoices.FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);
        if (invoice == null)
            return ApiResponse<bool>.ErrorResponse("Invoice not found.", code: ErrorCodes.InvoiceNotFound);
        if (invoice.Status == InvoiceStatus.Paid)
            return ApiResponse<bool>.SuccessResponse(true);

        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidAt = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task CancelOtherPendingPaymentsForUser(Guid paidInvoiceId, CancellationToken cancellationToken = default)
    {
        var paidInvoice = await dbContext.Invoices
            .FirstOrDefaultAsync(i => i.Id == paidInvoiceId, cancellationToken);
        if (paidInvoice == null) return;

        var userId = paidInvoice.UserId;

        var otherPending = await dbContext.Invoices
            .Where(i =>
                i.UserId == userId
                && i.Id != paidInvoiceId
                && (i.Status == InvoiceStatus.Draft || i.Status == InvoiceStatus.Sent))
            .ToListAsync(cancellationToken);

        foreach (var inv in otherPending)
        {
            if (!string.IsNullOrWhiteSpace(inv.QiPaymentId))
                _ = await qiCardService.CancelPaymentAsync(inv.QiPaymentId);
            inv.Status = InvoiceStatus.Cancelled;
        }

        if (otherPending.Count > 0)
            await dbContext.SaveChangesAsync(cancellationToken);
    }
}
