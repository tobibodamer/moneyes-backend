using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Moneyes.Server.Services;

/// <inheritdoc cref="ITokenGenerator"/>
public class JwtTokenGenerator : ITokenGenerator
{
    public (string Serialized, SecurityToken Token) Generate(
        string secretKey, string issuer, string audience, double expires, IEnumerable<Claim>? claims = null)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        List<Claim> claimsList = claims?.ToList() ?? new();

        if (!claimsList.Any(c => c.Type == JwtRegisteredClaimNames.Jti))
        {
            claimsList.Add(new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        }

        JwtSecurityToken securityToken = new(issuer, audience,
            claimsList,
            DateTime.UtcNow,
            DateTime.UtcNow.AddMinutes(expires),
            credentials);
        return (new JwtSecurityTokenHandler().WriteToken(securityToken), securityToken);
    }
}