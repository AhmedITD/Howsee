namespace Howsee.Application.DTOs.requests.Currency;

public class UpdateCurrencyRequest
{
    public string? Name { get; set; }
    public string? Symbol { get; set; }
    public bool? IsActive { get; set; }
}
