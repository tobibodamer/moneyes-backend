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

        public AuthController(ITokenAuthenticateService tokenAuthenticateService)
        {
            _tokenAuthenticateService = tokenAuthenticateService;
        }

        /// <summary>
        /// Gets a valid access token for the signed in user to authenticate and authorize with.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="TokenAuthenticateResult"/> consisting of an access and refresh token.</returns>
        [Authorize]
        [HttpGet("token")]
        public async Task<TokenAuthenticateResult> GetToken(string? appId = null, CancellationToken cancellationToken = default)
        {
            return await _tokenAuthenticateService.Authenticate(User, appId, cancellationToken);
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
                return BadRequest("No access token available.");
            }

            try
            {
                // Return new access and refresh token
                return await _tokenAuthenticateService.Reauthenticate(accessToken!, refreshToken, cancellationToken);
            }
            catch (SecurityTokenException)
            {
                return Unauthorized();
            }
        }
    }
}
