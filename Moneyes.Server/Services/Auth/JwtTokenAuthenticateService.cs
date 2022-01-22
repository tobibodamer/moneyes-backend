using Microsoft.AspNetCore.Identity;
using Moneyes.Server.Data;
using System.Security.Claims;

namespace Moneyes.Server.Services;

public class JwtTokenAuthenticateService : ITokenAuthenticateService
{
    private readonly IAccessTokenService _accessTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ApplicationDbContext _context;
    public JwtTokenAuthenticateService(
        IAccessTokenService accessTokenService, 
        IRefreshTokenService refreshTokenService, 
        ApplicationDbContext context)
    {
        _accessTokenService = accessTokenService;
        _refreshTokenService = refreshTokenService;
        _context = context;
    }

    public async Task<TokenAuthenticateResult> Authenticate(ClaimsPrincipal user, string? appId = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<Claim> claims = user.Claims;

        if (appId != null)
        {
            claims = claims.Concat(new Claim[] { new("appid", appId) });
        }

        var refreshToken = _refreshTokenService.Generate(claims);

        var userId = user.FindFirst(claim => claim.Type == ClaimTypes.NameIdentifier)!.Value;

        var existingRefreshToken = await _context.RefreshTokens.FindAsync(new[] { refreshToken, userId }, cancellationToken);

        if (existingRefreshToken == null)
        {
            await _context.RefreshTokens.AddAsync(new RefreshToken(userId, refreshToken), cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new()
        {
            AccessToken = _accessTokenService.Generate(claims),
            RefreshToken = refreshToken
        };
    }
}