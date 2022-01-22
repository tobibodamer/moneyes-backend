using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Moneyes.Server.Services;

public class JwtRefreshTokenService : IRefreshTokenService
{
    private readonly ITokenGenerator _tokenGenerator;
    private readonly JwtSettings _jwtSettings;

    public JwtRefreshTokenService(ITokenGenerator tokenGenerator, JwtSettings jwtSettings) =>
        (_tokenGenerator, _jwtSettings) = (tokenGenerator, jwtSettings);

    public string Generate(IEnumerable<Claim>? claims = null) => 
        _tokenGenerator.Generate(
            _jwtSettings.RefreshTokenSecret,
            _jwtSettings.Issuer, _jwtSettings.Audience,
            _jwtSettings.RefreshTokenExpirationMinutes)
        .Serialized;

    public (string Token, string Id, DateTime ExpiresAt) GenerateWithInfo(IEnumerable<Claim>? claims = null)
    {
        (string Serialized, SecurityToken Token) = _tokenGenerator.Generate(
            _jwtSettings.RefreshTokenSecret,
            _jwtSettings.Issuer, _jwtSettings.Audience,
            _jwtSettings.RefreshTokenExpirationMinutes);

        return (Serialized, Token.Id, Token.ValidTo);
    }
}