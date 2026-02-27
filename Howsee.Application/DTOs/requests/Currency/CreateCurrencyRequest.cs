namespace Howsee.Application.DTOs.requests.Currency;

public class CreateCurrencyRequest
{
    public required string Code { get; set; }
    public required string Name { get; set; }
    public required string Symbol { get; set; }
    public bool IsActive { get; set; } = true;
}
