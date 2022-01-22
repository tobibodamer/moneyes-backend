using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Moneyes.Server.Data;
using System.Security.Claims;

namespace Moneyes.Server.Services;

public class JwtTokenAuthenticateService : ITokenAuthenticateService
{
    private readonly IAccessTokenService _accessTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IAccessTokenValidator _accessTokenValidator;
    private readonly IRefreshTokenValidator _refreshTokenValidator;
    private readonly ApplicationDbContext _context;
    public JwtTokenAuthenticateService(
        IAccessTokenService accessTokenService,
        IRefreshTokenService refreshTokenService,
        IAccessTokenValidator accessTokenValidator,
        IRefreshTokenValidator refreshTokenValidator,
        ApplicationDbContext context)
    {
        _accessTokenService = accessTokenService;
        _refreshTokenService = refreshTokenService;
        _accessTokenValidator = accessTokenValidator;
        _refreshTokenValidator = refreshTokenValidator;
        _context = context;
    }

    public async Task<TokenAuthenticateResult> Reauthenticate(string accessToken, string refreshToken, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(accessToken);

        // Validate refresh token

        var isValidRefreshToken = _refreshTokenValidator.Validate(refreshToken, out var validatedRefreshToken);

        if (!isValidRefreshToken)
        {
            throw new SecurityTokenException("Invalid refresh token");
        }

        // Validate access token and retrieve principal
        var user = _accessTokenValidator.Validate(accessToken);

        if (user is null)
        {
            throw new SecurityTokenException("Invalid access token");
        }

        string userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        // Get refresh token object from database

        var refreshTokenObject = await _context.RefreshTokens
            .FirstOrDefaultAsync(x =>
                x.Token == validatedRefreshToken!.Id &&
                x.User == userId,
                cancellationToken);

        if (refreshTokenObject is null)
        {
            throw new SecurityTokenException("Invalid refresh token");
        }

        // Remove old refresh token from database
        _context.RefreshTokens.Remove(refreshTokenObject!);

        await _context.SaveChangesAsync(cancellationToken);

        string appId = user.FindFirstValue("appid");

        // Return new access and refresh token
        return await Authenticate(user, appId, cancellationToken);

    }

    public async Task<TokenAuthenticateResult> Authenticate(ClaimsPrincipal user, string? appId = null, CancellationToken cancellationToken = default)
    {
        IEnumerable<Claim> claims = user.Claims;

        // Add claim for app id
        if (appId != null)
        {
            claims = claims.Concat(new Claim[] { new("appid", appId) });
        }

        var refreshToken = _refreshTokenService.GenerateWithInfo(claims);

        string? userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            throw new ArgumentException("Invalid username", nameof(user));
        }

        // Add refresh token to database when not existing

        RefreshToken? existingRefreshToken = await _context.RefreshTokens.FindAsync(
            new[] { refreshToken.Id, userId }, 
            cancellationToken);

        if (existingRefreshToken == null)
        {
            await _context.RefreshTokens.AddAsync(
                new RefreshToken(userId, refreshToken.Id, refreshToken.ExpiresAt),
                cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);

        return new()
        {
            AccessToken = _accessTokenService.Generate(claims),
            RefreshToken = refreshToken.Token
        };
    }
}