using System.Security.Claims;

namespace Moneyes.Server.Services;

/// <summary>
/// Provides methods for active refresh tokens.
/// </summary>
public interface IActiveRefreshTokenService
{
    /// <summary>
    /// Cleans up all expired refresh tokens of the given <paramref name="user"/> from the database.
    /// </summary>
    /// <param name="user">The user which has to have a <see cref="Claim"/> for <see cref="ClaimTypes.NameIdentifier"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>The number of deleted tokens.</returns>
    Task<int> CleanupExpiredTokens(ClaimsPrincipal user, CancellationToken cancellationToken = default);


    /// <summary>
    /// Revokes all active refresh tokens of the given <paramref name="user"/> from the database.
    /// </summary>
    /// <param name="user">The user which has to have a <see cref="Claim"/> for <see cref="ClaimTypes.NameIdentifier"/>.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
    /// <returns>The number of revoked tokens.</returns>
    Task<int> RevokeAllTokens(ClaimsPrincipal user, string? appId = null, CancellationToken cancellationToken = default);
}
