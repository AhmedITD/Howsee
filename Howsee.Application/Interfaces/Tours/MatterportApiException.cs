namespace Howsee.Application.Interfaces.Tours;

/// <summary>
/// Thrown when the Matterport API returns an error (GraphQL errors or HTTP failure).
/// Code maps to Matterport error codes: not.found, request.unauthorized, request.rate.exceeded, etc.
/// </summary>
public class MatterportApiException : Exception
{
    /// <summary>Matterport error code (e.g. NOT_FOUND, UNAUTHORIZED).</summary>
    public string? ErrorCode { get; }

    [Obsolete("Use ErrorCode instead.")]
    public string? Code => ErrorCode;

    public MatterportApiException(string message, string? code = null) : base(message)
    {
        ErrorCode = code;
    }

    public MatterportApiException(string message, string? code, Exception inner) : base(message, inner)
    {
        ErrorCode = code;
    }
}

/// <summary>Thrown when Matterport returns UNAUTHORIZED or request.unauthenticated/request.unauthorized.</summary>
public class MatterportAuthException : MatterportApiException
{
    public MatterportAuthException(string message) : base(message, "UNAUTHORIZED") { }
}

/// <summary>Thrown when Matterport returns NOT_FOUND or not.found.</summary>
public class MatterportNotFoundException : MatterportApiException
{
    public MatterportNotFoundException(string message) : base(message, "NOT_FOUND") { }
}

/// <summary>Thrown when Matterport returns RATE_LIMIT_EXCEEDED or request.rate.exceeded.</summary>
public class MatterportRateLimitException : MatterportApiException
{
    public MatterportRateLimitException(string message) : base(message, "RATE_LIMIT_EXCEEDED") { }
}
