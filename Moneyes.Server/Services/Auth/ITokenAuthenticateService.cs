using System.Security.Claims;

namespace Moneyes.Server.Services;

/// <summary>
/// Interface for authentication.
/// </summary>
public interface ITokenAuthenticateService
{
    /// <summary>
    /// Reauthenticates a user by refreshing an expired <paramref name="accessToken"/> using a <paramref name="refreshToken"/>. <br></br>
    /// Takes responsibilities to validate both tokens, revocate the old refresh token and generate a new pair.
    /// </summary>
    /// <param name="accessToken">The (expired) access token.</param>
    /// <param name="refreshToken">The corresponding refresh token.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the process.</param>
    /// <returns>Instance of <see cref="TokenAuthenticateResult"/>.</returns>
    Task<TokenAuthenticateResult> Reauthenticate(
        string accessToken, 
        string refreshToken, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates user.
    /// Takes responsibilities to generate access and refresh token, save refresh token in database
    /// and return instance of <see cref="TokenAuthenticateResult"/> class. 
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="appId">The application id to include in the token.</param>
    /// <param name="cancellationToken">Instance of <see cref="CancellationToken"/>.</param>
    Task<TokenAuthenticateResult> Authenticate(
        ClaimsPrincipal user, 
        string? appId = null,
        CancellationToken cancellationToken = default);
}
