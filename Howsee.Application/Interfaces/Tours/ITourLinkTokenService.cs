namespace Howsee.Application.Interfaces.Tours;

public interface ITourLinkTokenService
{
    string Generate(int tourId);
    int? Validate(string token);
}
