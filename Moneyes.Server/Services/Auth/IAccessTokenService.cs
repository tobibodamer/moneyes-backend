using System.Security.Claims;

namespace Moneyes.Server.Services
{
    /// <inheritdoc cref="ITokenService"/>
    public interface IAccessTokenService : ITokenService 
    {
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}