using System.Security.Claims;

namespace Moneyes.Server.Services;

/// <summary>
/// Interface for authentication.
/// </summary>
public interface ITokenAuthenticateService
{
    /// <summary>
    /// Authenticates user.
    /// Takes responsibilities to generate access and refresh token, save refresh token in database
    /// and return instance of <see cref="AuthenticateResponse"/> class. 
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">Instance of <see cref="CancellationToken"/>.</param>
    Task<TokenAuthenticateResult> Authenticate(ClaimsPrincipal user, CancellationToken cancellationToken);
}
