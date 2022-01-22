using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Moneyes.Server.Data;
using Moneyes.Server.Services;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Moneyes.Server.Controllers
{
    [Route("api/{controller}")]
    public class AuthController : ControllerBase
    {
        private readonly ITokenAuthenticateService _tokenAuthenticateService;
        private readonly IRefreshTokenValidator _refreshTokenValidator;
        private readonly IAccessTokenValidator _accessTokenValidator;
        private readonly ApplicationDbContext _context;

        public AuthController(ITokenAuthenticateService tokenAuthenticateService,
            IRefreshTokenValidator refreshTokenValidator,
            IAccessTokenValidator accessTokenValidator,
            ApplicationDbContext context)
        {
            _tokenAuthenticateService = tokenAuthenticateService;
            _refreshTokenValidator = refreshTokenValidator;
            _accessTokenValidator = accessTokenValidator;
            _context = context;
        }

        /// <summary>
        /// Gets a valid access token for the signed in user to authenticate and authorize with.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="TokenAuthenticateResult"/> consisting of an access and refresh token.</returns>
        [Authorize]
        [HttpGet("token")]
        public async Task<TokenAuthenticateResult> GetToken(CancellationToken cancellationToken = default)
        {
            return await _tokenAuthenticateService.Authenticate(HttpContext.User, cancellationToken);
        }

        /// <summary>
        /// Refresh a given <paramref name="accessToken"/> using the corresponding <paramref name="refreshToken"/>.
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <param name="accessToken"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A new access and refresh token.</returns>
        [AllowAnonymous]
        [HttpPost("refreshToken")]
        public async Task<ActionResult<TokenAuthenticateResult>> RefreshToken(
            string refreshToken, 
            string? accessToken = null, 
            CancellationToken cancellationToken = default)
        {
            // Try get token from context (set by authentication middleware) when not supplied
            accessToken ??= await HttpContext.GetTokenAsync("access_token");

            if (accessToken is null)
            {
                return BadRequest("Invalid access token");
            }

            // Validate refresh token

            var isValidRefreshToken = _refreshTokenValidator.Validate(refreshToken);

            if (!isValidRefreshToken)
            {
                return BadRequest("Invalid refresh token");
            }

            // Validate access token and retrieve principal
            var user = _accessTokenValidator.Validate(accessToken);

            if (user is null)
            {
                return BadRequest("Invalid access token");
            }

            string userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

            // Get refresh token object from database

            var refreshTokenObject = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken && x.User == userId, cancellationToken);

            if (refreshTokenObject is null)
            {
                return BadRequest("Invalid refresh token");
            }

            // Remove old refresh token from database
            _context.RefreshTokens.Remove(refreshTokenObject!);

            await _context.SaveChangesAsync();

            // Return new access and refresh token
            return await _tokenAuthenticateService.Authenticate(user, cancellationToken);
        }
    }
}
