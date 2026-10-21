using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;

namespace TecNM.Api.Core.Security;

public interface ICurrentUser
{
    long? Id { get; }
    string? Username { get; }
    bool IsAuthenticated { get; }
}

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    public long? Id
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            var sub = user?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return long.TryParse(sub, CultureInfo.InvariantCulture, out var id) ? id : null;
        }
    }

    public string? Username
    {
        get
        {
            var user = httpContextAccessor.HttpContext?.User;
            return user?.FindFirst("username")?.Value ?? user?.Identity?.Name;
        }
    }

    public bool IsAuthenticated => httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
