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
    IWaylPaymentService waylPaymentService,
    IConfiguration configuration) : IInvoiceService
{
    public async Task<ApiResponse<InvoiceResponse>> CreateInvoice(InvoiceRequest request, CancellationToken cancellationToken = default)
    {
        var plan = await dbContext.PricingPlans
            .Include(p => p.Currency)
            .FirstOrDefaultAsync(p => p.Key == request.PlanKey && p.IsActive, cancellationToken);
        if (plan == null)
            return ApiResponse<InvoiceResponse>.ErrorResponse("Invalid or inactive plan.", code: ErrorCodes.InvalidOrInactivePlan);

        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            UserId = currentUser.Id,
            TotalAmount = plan.Amount,
            CurrencyId = plan.CurrencyId,
            Description = request.Description ?? plan.Name,
            Status = InvoiceStatus.Draft,
            PricingPlanId = plan.Id
        };

        dbContext.Invoices.Add(invoice);
        await dbContext.SaveChangesAsync(cancellationToken);

        var appUrl = configuration["APP_URL"];
        var webhookUrl = string.IsNullOrEmpty(appUrl) ? null : $"{appUrl}/api/payments/webhook/wayl";
        var webhookSecret = configuration["Wayl:WebhookSecret"];

        var waylRequest = new WaylCreateLinkRequest
        {
            ReferenceId = invoice.Id.ToString(),
            Total = invoice.TotalAmount,
            Currency = plan.Currency.Code,
            WebhookUrl = webhookUrl,
            WebhookSecret = webhookSecret,
            RedirectionUrl = request.FinishUrl
        };

        var waylResponse = await waylPaymentService.CreateLinkAsync(waylRequest, cancellationToken);

        if (!waylResponse.Success)
            return ApiResponse<InvoiceResponse>.ErrorResponse(waylResponse.Error ?? "Payment link creation failed.", code: ErrorCodes.PaymentInitiationFailed);

        invoice.WaylPaymentId = waylResponse.LinkId;
        await dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse<InvoiceResponse>.SuccessResponse(new InvoiceResponse
        {
            InvoiceId = invoice.Id,
            PaymentUrl = waylResponse.PaymentUrl ?? string.Empty
        });
    }

    public async Task<ApiResponse<bool>> MarkAsPaid(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await dbContext.Invoices
            .Include(i => i.PricingPlan)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);
        if (invoice == null)
            return ApiResponse<bool>.ErrorResponse("Invoice not found.", code: ErrorCodes.InvoiceNotFound);
        if (invoice.Status == InvoiceStatus.Paid)
            return ApiResponse<bool>.SuccessResponse(true);

        invoice.Status = InvoiceStatus.Paid;
        invoice.PaidAt = DateTime.UtcNow;

        if (invoice.PricingPlanId.HasValue && invoice.PricingPlan != null)
        {
            if (string.Equals(invoice.PricingPlan.Unit, "month", StringComparison.OrdinalIgnoreCase))
                await CreateOrExtendSubscriptionAsync(invoice, cancellationToken);

            if (invoice.PricingPlan.Role.HasValue)
            {
                var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == invoice.UserId, cancellationToken);
                if (user != null)
                {
                    user.Role = invoice.PricingPlan.Role.Value;
                    user.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true);
    }

    private async Task CreateOrExtendSubscriptionAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        var paidAt = invoice.PaidAt ?? DateTime.UtcNow;
        var userId = invoice.UserId;
        var planId = invoice.PricingPlanId!.Value;

        var existing = await dbContext.Subscriptions
            .Where(s => s.UserId == userId && s.PricingPlanId == planId && s.Status == SubscriptionStatus.Active && s.EndDate >= paidAt.Date)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing != null)
        {
            existing.EndDate = existing.EndDate.AddMonths(1);
            existing.InvoiceId = invoice.Id;
        }
        else
        {
            var startDate = paidAt.Date;
            var endDate = startDate.AddMonths(1);
            dbContext.Subscriptions.Add(new Subscription
            {
                UserId = userId,
                PricingPlanId = planId,
                StartDate = startDate,
                EndDate = endDate,
                Status = SubscriptionStatus.Active,
                InvoiceId = invoice.Id
            });
        }
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
            if (!string.IsNullOrWhiteSpace(inv.WaylPaymentId))
                _ = await waylPaymentService.InvalidateLinkIfPendingAsync(inv.Id.ToString(), cancellationToken);
            inv.Status = InvoiceStatus.Cancelled;
        }

        if (otherPending.Count > 0)
            await dbContext.SaveChangesAsync(cancellationToken);
    }
}
