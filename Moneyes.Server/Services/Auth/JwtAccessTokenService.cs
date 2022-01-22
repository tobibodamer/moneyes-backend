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

    public string Generate(IEnumerable<Claim> claims)
    {
        ClaimsIdentity subject = new(claims);

        subject.AddClaim(new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        
        return _tokenGenerator.Generate(_jwtSettings.AccessTokenSecret, _jwtSettings.Issuer, _jwtSettings.Audience,
            _jwtSettings.AccessTokenExpirationMinutes, subject.Claims);
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false, //you might want to validate the audience and issuer depending on your use case
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("the server key used to sign the JWT token is here, use more than 16 chars")),
            ValidateLifetime = false //here we are saying that we don't care about the token's expiration date
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken securityToken;
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
        var jwtSecurityToken = securityToken as JwtSecurityToken;
        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");

        return principal;
    }
}
