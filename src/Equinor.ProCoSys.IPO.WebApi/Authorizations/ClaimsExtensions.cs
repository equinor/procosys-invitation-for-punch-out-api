using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations
{
    public static class ClaimsExtensions
    {
        public const string Oid = "http://schemas.microsoft.com/identity/claims/objectidentifier";

        public static Guid? TryGetOid(this IEnumerable<Claim> claims)
        {
            var oidClaim = claims.SingleOrDefault(c => c.Type == Oid);
            if (Guid.TryParse(oidClaim?.Value, out var oid))
            {
                return oid;
            }

            return null;
        }

        public static string TryGetGivenName(this IEnumerable<Claim> claims)
        {
            var givenName = claims.SingleOrDefault(c => c.Type == ClaimTypes.GivenName);
            return givenName?.Value;
        }

        public static string TryGetSurName(this IEnumerable<Claim> claims)
        {
            var surName = claims.SingleOrDefault(c => c.Type == ClaimTypes.Surname);
            return surName?.Value;
        }

        public static string TryGetUserName(this IEnumerable<Claim> claims)
        {
            var userName = claims.SingleOrDefault(c => c.Type == ClaimTypes.Upn);
            return userName?.Value;
        }

        public static string TryGetEmail(this IEnumerable<Claim> claims)
        {
            var email = claims.SingleOrDefault(c => c.Type == ClaimTypes.Email);
            return email?.Value;
        }
    }
}
