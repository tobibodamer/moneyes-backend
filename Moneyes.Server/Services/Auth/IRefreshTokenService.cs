using System.Security.Claims;

namespace Moneyes.Server.Services
{
    /// <inheritdoc cref="ITokenService"/>
    public interface IRefreshTokenService : ITokenService
    {
        /// <summary>
        /// Generates token based on user information.
        /// </summary>
        /// <returns>Generated token and its id.</returns>
        (string Token, string Id) GenerateWithId(IEnumerable<Claim> claims);
    }
}