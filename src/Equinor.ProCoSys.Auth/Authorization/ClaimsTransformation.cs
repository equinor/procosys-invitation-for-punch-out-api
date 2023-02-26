using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Authentication;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Misc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Auth.Authorization
{
    /// <summary>
    /// Implement IClaimsTransformation to extend the ClaimsPrincipal with claims to be used during authorization
    /// Claims added only for authenticated and existing users, for requests handling a valid plant for user
    /// These types of claims are added:
    ///  * ClaimTypes.Role claim for each permission found in IPermissionCache
    ///  * ClaimTypes.UserData claim for each project user has access too. Claim name start with ProjectPrefix
    ///  * ClaimTypes.UserData claim for each restriction role for user. Claim name start with ContentRestrictionPrefix
    ///         Restriction role = "%" means "User has no restriction roles"
    /// </summary>
    public class ClaimsTransformation : IClaimsTransformation
    {
        public static string ClaimsIssuer = "ProCoSys";
        public static string ProjectPrefix = "PCS_Project##";
        public static string ContentRestrictionPrefix = "PCS_ContentRestriction##";
        public static string NoRestrictions = "%";

        private readonly IPersonCache _personCache;
        private readonly IPlantProvider _plantProvider;
        private readonly IPermissionCache _permissionCache;
        private readonly ILogger<ClaimsTransformation> _logger;
        private readonly IAuthenticatorOptions _authenticatorOptions;

        public ClaimsTransformation(
            IPersonCache personCache,
            IPlantProvider plantProvider,
            IPermissionCache permissionCache,
            ILogger<ClaimsTransformation> logger,
            IAuthenticatorOptions authenticatorOptions)
        {
            _personCache = personCache;
            _plantProvider = plantProvider;
            _permissionCache = permissionCache;
            _logger = logger;
            _authenticatorOptions = authenticatorOptions;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            _logger.LogInformation($"----- {GetType().Name} start");
            // Can't use CurrentUserProvider here. Middleware setting current user not called yet. 
            var userOid = principal.Claims.TryGetOid();
            if (!userOid.HasValue)
            {
                _logger.LogInformation($"----- {GetType().Name} early exit, not authenticated yet");
                return principal;
            }

            if (!await _personCache.ExistsAsync(userOid.Value))
            {
                _logger.LogInformation($"----- {GetType().Name} early exit, {userOid} don't exists in ProCoSys");
                return principal;
            }

            var plantId = _plantProvider.Plant;

            if (string.IsNullOrEmpty(plantId))
            {
                _logger.LogInformation($"----- {GetType().Name} early exit, not a plant request");
                return principal;
            }

            if (!await _permissionCache.HasUserAccessToPlantAsync(plantId, userOid.Value))
            {
                _logger.LogInformation($"----- {GetType().Name} early exit, not a valid plant for user");
                return principal;
            }

            var claimsIdentity = GetOrCreateClaimsIdentityForThisIssuer(principal);

            await AddRoleForAllPermissionsToIdentityAsync(claimsIdentity, plantId, userOid.Value);
            if (!_authenticatorOptions.DisableProjectUserDataClaims)
            {
                await AddUserDataClaimForAllOpenProjectsToIdentityAsync(claimsIdentity, plantId, userOid.Value);
            }
            if (!_authenticatorOptions.DisableRestrictionRoleUserDataClaims)
            { 
                await AddUserDataClaimForAllContentRestrictionsToIdentityAsync(claimsIdentity, plantId, userOid.Value);
            }

            _logger.LogInformation($"----- {GetType().Name} completed");
            return principal;
        }

        public static string GetProjectClaimValue(string projectName) => $"{ProjectPrefix}{projectName}";

        public static string GetContentRestrictionClaimValue(string contentRestriction) => $"{ContentRestrictionPrefix}{contentRestriction}";

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

        private async Task AddUserDataClaimForAllOpenProjectsToIdentityAsync(ClaimsIdentity claimsIdentity, string plantId, Guid userOid)
        {
            var projectNames = await _permissionCache.GetProjectsForUserAsync(plantId, userOid);
            projectNames?.ToList().ForEach(projectName => claimsIdentity.AddClaim(CreateClaim(ClaimTypes.UserData, GetProjectClaimValue(projectName))));
        }

        private async Task AddUserDataClaimForAllContentRestrictionsToIdentityAsync(ClaimsIdentity claimsIdentity, string plantId, Guid userOid)
        {
            var contentRestrictions = await _permissionCache.GetContentRestrictionsForUserAsync(plantId, userOid);
            contentRestrictions?.ToList().ForEach(
                contentRestriction => claimsIdentity.AddClaim(CreateClaim(ClaimTypes.UserData, GetContentRestrictionClaimValue(contentRestriction))));
        }

        private static Claim CreateClaim(string claimType, string claimValue)
            => new Claim(claimType, claimValue, null, ClaimsIssuer);
    }
}
