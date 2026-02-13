namespace Howsee.Domain.Enums;

public enum InvoiceStatus
{
    Draft = 1,
    Sent = 2,
    Paid = 3,
    Overdue = 4,
    Cancelled = 5
}

public static class InvoiceStatusExtensions
{
    public static string ToStringValue(this InvoiceStatus status)
    {
        return status switch
        {
            InvoiceStatus.Draft => "draft",
            InvoiceStatus.Sent => "sent",
            InvoiceStatus.Paid => "paid",
            InvoiceStatus.Overdue => "overdue",
            InvoiceStatus.Cancelled => "cancelled",
            _ => "draft"
        };
    }

    public static InvoiceStatus FromString(string? value)
    {
        return value?.ToLower() switch
        {
            "draft" => InvoiceStatus.Draft,
            "sent" => InvoiceStatus.Sent,
            "paid" => InvoiceStatus.Paid,
            "overdue" => InvoiceStatus.Overdue,
            "cancelled" => InvoiceStatus.Cancelled,
            _ => InvoiceStatus.Draft
        };
    }
}
