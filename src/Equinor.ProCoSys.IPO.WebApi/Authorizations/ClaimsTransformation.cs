using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.Plant;
using Microsoft.AspNetCore.Authentication;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations
{
    public class ClaimsTransformation : IClaimsTransformation
    {
        public static string ClaimsIssuer = "ProCoSys";
        public static string ProjectPrefix = "PCS_Project##";

        private readonly IPlantProvider _plantProvider;
        private readonly IPlantCache _plantCache;
        private readonly IPermissionCache _permissionCache;

        public ClaimsTransformation(IPlantProvider plantProvider, IPlantCache plantCache, IPermissionCache permissionCache)
        {
            _plantProvider = plantProvider;
            _plantCache = plantCache;
            _permissionCache = permissionCache;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var userOid = principal.Claims.TryGetOid();
            if (!userOid.HasValue)
            {
                // not add claims if no oid found (I.e user not authenticated yet)
                return principal;
            }

            var plantId = _plantProvider.Plant;

            if (string.IsNullOrEmpty(plantId))
            {
                // not add plant specific claims if no plant given in request
                return principal;
            }

            if (!await _plantCache.HasUserAccessToPlantAsync(plantId, userOid.Value))
            {
                // not add plant specific claims if plant not among plants for user
                return principal;
            }


            var claimsIdentity = GetOrCreateClaimsIdentityForThisIssuer(principal);

            await AddRoleForAllPermissionsToIdentityAsync(claimsIdentity, plantId, userOid.Value);
            await AddUserDataClaimForAllProjectsToIdentityAsync(claimsIdentity, plantId, userOid.Value);

            return principal;
        }

        public static string GetProjectClaimValue(string projectName) => $"{ProjectPrefix}{projectName}";

        private ClaimsIdentity GetOrCreateClaimsIdentityForThisIssuer(ClaimsPrincipal principal)
        {
            var identity = principal.Identities.SingleOrDefault(i => i.Label == ClaimsIssuer);
            if (identity == null)
            {
                identity = new ClaimsIdentity {Label = ClaimsIssuer};
                principal.AddIdentity(identity);
            }
            else
            {
                ClearOldClaims(identity);
            }

            return identity;
        }

        private void ClearOldClaims(ClaimsIdentity identity)
        {
            var oldClaims = identity.Claims.Where(c => c.Issuer == ClaimsIssuer).ToList();
            oldClaims.ForEach(identity.RemoveClaim);
        }

        private async Task AddRoleForAllPermissionsToIdentityAsync(ClaimsIdentity claimsIdentity, string plantId, Guid userOid)
        {
            var permissions = await _permissionCache.GetPermissionsForUserAsync(plantId, userOid);
            permissions?.ToList().ForEach(
                permission => claimsIdentity.AddClaim(CreateClaim(ClaimTypes.Role, permission)));
        }

        private async Task AddUserDataClaimForAllProjectsToIdentityAsync(ClaimsIdentity claimsIdentity, string plantId, Guid userOid)
        {
            var projectNames = await _permissionCache.GetProjectsForUserAsync(plantId, userOid);
            projectNames?.ToList().ForEach(projectName => claimsIdentity.AddClaim(CreateClaim(ClaimTypes.UserData, GetProjectClaimValue(projectName))));
        }

        private static Claim CreateClaim(string claimType, string claimValue)
            => new Claim(claimType, claimValue, null, ClaimsIssuer);
    }
}
