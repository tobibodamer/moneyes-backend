using System.Security.Claims;

namespace Moneyes.Server.Services;

public interface IAccessTokenValidator
{
    ClaimsPrincipal? Validate(string accessToken);
}
