namespace Moneyes.Server.Services;

/// <summary>
/// Represents a token authentication result.
/// </summary>
public class TokenAuthenticateResult
{
    public string AccessToken { get; init; }

    public string RefreshToken { get; init; }
}