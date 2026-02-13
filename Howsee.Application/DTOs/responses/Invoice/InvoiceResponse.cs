namespace Howsee.Application.DTOs.responses.Invoice;

public class InvoiceResponse
{
    public Guid InvoiceId { get; set; }
    public required string PaymentUrl { get; set; }
}
