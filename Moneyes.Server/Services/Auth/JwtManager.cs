using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Moneyes.Server
{
    /// <summary>
    /// Manager for creation of JWT tokens.
    /// </summary>
    public class JwtManager
    {
        private readonly IConfiguration _configuration;

        public JwtManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateJwtToken(HttpContext httpContext)
        {
            return GenerateJwtToken(httpContext.User.Claims);
        }

        /// <summary>
        /// Generate JWT Token after successful login.
        /// </summary>
        /// <returns></returns>
        public string GenerateJwtToken(IEnumerable<Claim> claims)
        {
            var (token, handler) = GenerateJwtToken2(claims);

            return handler.WriteToken(token);
        }

        /// <summary>
        /// Generate JWT Token after successful login.
        /// </summary>
        /// <returns></returns>
        public (SecurityToken? token, JwtSecurityTokenHandler handler) GenerateJwtToken2(IEnumerable<Claim> claims)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            ClaimsIdentity subject = new(claims);

            subject.AddClaim(new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = subject,
                Expires = DateTime.UtcNow.AddHours(1),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = creds
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return (token, tokenHandler);
        }
    }
}
