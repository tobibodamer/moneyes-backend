using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Moneyes.Server.Services;

public class CustomCookieAuthenticationEvents : CookieAuthenticationEvents
{
    private readonly ITokenAuthenticateService _tokenAuthenticateService;
    public CustomCookieAuthenticationEvents(ITokenAuthenticateService tokenAuthenticateService)
    {
        _tokenAuthenticateService = tokenAuthenticateService;
        OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync;
    }

    public override async Task SignedIn(CookieSignedInContext context)
    {
        if (context.Principal != null)
        {
            if (context.HttpContext.Items.TryGetValue("generate-jwt", out var generateJwt)
                && (generateJwt?.Equals(true) ?? false))
                {

                context.HttpContext.Items.TryGetValue("appId", out var appId);

                var tokenResult = await _tokenAuthenticateService.Authenticate(
                    context.Principal, appId?.ToString(), default);

                //context.Response.AddAuthTokenCookie(tokenResult);

                await context.Response.WriteAsJsonAsync(tokenResult);
            }
        }

        await base.SignedIn(context);
    }
}