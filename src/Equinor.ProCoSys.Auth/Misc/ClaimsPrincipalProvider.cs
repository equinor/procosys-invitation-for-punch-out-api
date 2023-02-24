using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Equinor.ProCoSys.Auth.Misc
{
    public class ClaimsPrincipalProvider : IClaimsPrincipalProvider
    {
        private readonly ClaimsPrincipal principal;

        public ClaimsPrincipalProvider(IHttpContextAccessor accessor) => principal = accessor?.HttpContext?.User ?? new ClaimsPrincipal();

        public ClaimsPrincipal GetCurrentClaimsPrincipal() => principal;
    }
}
