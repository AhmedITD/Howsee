namespace Howsee.Application.Interfaces.Tours;

/// <summary>
/// Thrown when the Matterport API returns an error (GraphQL errors or HTTP failure).
/// Code maps to Matterport error codes: not.found, not.unique, request.invalid, request.unsupported, request.rate.exceeded, request.unauthenticated, request.unauthorized, etc.
/// </summary>
public class MatterportApiException : Exception
{
    public string? Code { get; }

    public MatterportApiException(string? code, string message) : base(message)
    {
        Code = code;
    }

    public MatterportApiException(string? code, string message, Exception inner) : base(message, inner)
    {
        Code = code;
    }
}
