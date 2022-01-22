using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace Moneyes.Server.Services;

public class JwtAccessTokenService : IAccessTokenService
{
    private readonly ITokenGenerator _tokenGenerator;
    private readonly JwtSettings _jwtSettings;

    public JwtAccessTokenService(ITokenGenerator tokenGenerator, JwtSettings jwtSettings) =>
        (_tokenGenerator, _jwtSettings) = (tokenGenerator, jwtSettings);

    public string Generate(IEnumerable<Claim>? claims = null) =>
        _tokenGenerator.Generate(
            _jwtSettings.AccessTokenSecret,
            _jwtSettings.Issuer, _jwtSettings.Audience,
            _jwtSettings.AccessTokenExpirationMinutes,
            claims)
        .Token;

    public (string Token, string Id) GenerateWithId(IEnumerable<Claim>? claims = null) =>
        _tokenGenerator.Generate(
            _jwtSettings.AccessTokenSecret,
            _jwtSettings.Issuer, _jwtSettings.Audience,
            _jwtSettings.AccessTokenExpirationMinutes,
            claims);
}
