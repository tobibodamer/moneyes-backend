using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

public class CustomJwtBearerEvents : JwtBearerEvents
{
    public override Task TokenValidated(TokenValidatedContext context)
    {
        context.HttpContext.Items["Properties"] = context.Properties;
        context.HttpContext.Features.Set(context.Properties);

        return base.TokenValidated(context);
    }
    public override Task AuthenticationFailed(AuthenticationFailedContext context)
    {
        if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
        {
            context.Response.Headers.Add("Token-Expired", "true");
        }
        
        return base.AuthenticationFailed(context);
    }
}