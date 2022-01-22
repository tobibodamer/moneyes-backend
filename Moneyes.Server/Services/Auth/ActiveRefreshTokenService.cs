using Microsoft.EntityFrameworkCore;
using Moneyes.Server.Data;
using System.Security.Claims;

namespace Moneyes.Server.Services;

/// <inheritdoc/>
public class ActiveRefreshTokenService : IActiveRefreshTokenService
{
    private readonly ApplicationDbContext _dbContext;
    public ActiveRefreshTokenService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CleanupExpiredTokens(ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        string userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        var expiredTokens = await _dbContext.RefreshTokens
            .Where(refreshToken =>
                refreshToken.User.Equals(userId) &&
                refreshToken.Expires < DateTimeOffset.UtcNow)
            .ToListAsync(cancellationToken);

        _dbContext.RefreshTokens.RemoveRange(expiredTokens);

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> RevokeAllTokens(ClaimsPrincipal user, string? appId = null, CancellationToken cancellationToken = default)
    {
        string userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        var query = _dbContext.RefreshTokens
            .Where(refreshToken => refreshToken.User.Equals(userId));

        if (appId != null)
        {
            query = query.Where(refreshToken =>
                refreshToken.AppId != null &&
                refreshToken.AppId.Equals(appId));
        }

        var refreshTokens = await query.ToListAsync(cancellationToken);

        _dbContext.RefreshTokens.RemoveRange(refreshTokens);

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }
}