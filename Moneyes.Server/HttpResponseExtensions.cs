using Microsoft.IdentityModel.Tokens;
using Moneyes.Server.Services;
using System.IdentityModel.Tokens.Jwt;

namespace Moneyes.Server
{
    public static class HttpResponseExtensions
    {
        public static void AddJwtTokenCookie(this HttpResponse response,
            SecurityToken token, JwtSecurityTokenHandler tokenHandler)
        {
            string tokenString = tokenHandler.WriteToken(token);

            response.Cookies.Append("jwt_token", tokenString, new()
            {
                Expires = token.ValidTo,
                HttpOnly = true
            });

            //response.Headers.Add("access_token", tokenString);
            //response.Headers.Add("token_type", "JWT");
            //response.Headers.Add("expires_in", (DateTime.UtcNow - token.ValidTo).Seconds.ToString());
        }

        public static void AddAuthTokenCookie(this HttpResponse response,
            TokenAuthenticateResult tokenAuthenticateResult)
        {
            response.Cookies.Append("access_token", tokenAuthenticateResult.AccessToken, new()
            {
                HttpOnly = true
            });

            response.Cookies.Append("refresh_token", tokenAuthenticateResult.RefreshToken, new()
            {
                HttpOnly = true
            });
        }
    }
}
