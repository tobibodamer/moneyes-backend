using Microsoft.IdentityModel.Tokens;

namespace Moneyes.Server.Services;

/// <summary>
/// Interface for validating refresh token.
/// </summary>
public interface IRefreshTokenValidator
{
    /// <summary>
    /// Validates refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <returns>True if token is valid,otherwise false.</returns>
    bool Validate(string refreshToken, out SecurityToken? validatedToken);
}