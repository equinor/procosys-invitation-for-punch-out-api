using Microsoft.AspNetCore.Authorization;

namespace Equinor.ProCoSys.IPO.WebApi
{
    public class AuthorizeAnyAttribute : AuthorizeAttribute
    {
        public AuthorizeAnyAttribute()
        {
        }

        public AuthorizeAnyAttribute(params string[] permissions) => Roles = string.Join(",", permissions);
    }
}
