using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
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
            var upn = claims.SingleOrDefault(c => c.Type == ClaimTypes.Upn);
            var claimValue = upn?.Value;
            // Note: MailAddress.TryCreate(...) throws exception on null or empty string
            if (!string.IsNullOrWhiteSpace(claimValue) && MailAddress.TryCreate(claimValue, out var email))
            {
                return email.User;
            }
            return null;
        }

        public static string TryGetEmail(this IEnumerable<Claim> claims)
        {
            var upn = claims.SingleOrDefault(c => c.Type == ClaimTypes.Upn);
            var claimValue = upn?.Value;
            // Note: MailAddress.TryCreate(...) throws exception on null or empty string
            if (!string.IsNullOrWhiteSpace(claimValue) && MailAddress.TryCreate(claimValue, out var email))
            {
                return email.Address;
            }
            return null;
        }
    }
}
