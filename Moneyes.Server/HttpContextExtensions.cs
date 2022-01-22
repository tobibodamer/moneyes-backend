namespace Moneyes.Server
{
    public static class HttpContextExtensions
    {
        public static void AddJwtTokenCookie(this HttpContext context, JwtManager jwtManager)
        {
            var (token, tokenHandler) = jwtManager.GenerateJwtToken2(context.User.Claims);

            if (token == null)
            {
                return;
            }

            context.Response.AddJwtTokenCookie(token, tokenHandler);
        }
    }
}
