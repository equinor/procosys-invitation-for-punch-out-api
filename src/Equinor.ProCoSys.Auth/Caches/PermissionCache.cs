using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Misc;
using Equinor.ProCoSys.Auth.Permission;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Auth.Caches
{
    public class PermissionCache : IPermissionCache
    {
        private readonly ICacheManager _cacheManager;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IPermissionApiService _permissionApiService;
        private readonly IOptionsMonitor<CacheOptions> _options;

        public PermissionCache(
            ICacheManager cacheManager,
            ICurrentUserProvider currentUserProvider,
            IPermissionApiService permissionApiService,
            IOptionsMonitor<CacheOptions> options)
        {
            _cacheManager = cacheManager;
            _currentUserProvider = currentUserProvider;
            _permissionApiService = permissionApiService;
            _options = options;
        }

        public async Task<IList<string>> GetPlantIdsWithAccessForUserAsync(Guid userOid)
        {
            var allPlants = await GetAllPlantsForUserAsync(userOid);
            return allPlants?.Where(p => p.HasAccess).Select(p => p.Id).ToList();
        }

        public async Task<bool> HasUserAccessToPlantAsync(string plantId, Guid userOid)
        {
            var plantIds = await GetPlantIdsWithAccessForUserAsync(userOid);
            return plantIds.Contains(plantId);
        }

        public async Task<bool> HasCurrentUserAccessToPlantAsync(string plantId)
        {
            var userOid = _currentUserProvider.GetCurrentUserOid();

            return await HasUserAccessToPlantAsync(plantId, userOid);
        }

        public async Task<bool> IsAValidPlantAsync(string plantId)
        {
            var userOid = _currentUserProvider.GetCurrentUserOid();
            var allPlants = await GetAllPlantsForUserAsync(userOid);
            return allPlants != null && allPlants.Any(p => p.Id == plantId);
        }

        public async Task<string> GetPlantTitleAsync(string plantId)
        {
            var userOid = _currentUserProvider.GetCurrentUserOid();
            var allPlants = await GetAllPlantsForUserAsync(userOid);
            return allPlants?.Where(p => p.Id == plantId).SingleOrDefault()?.Title;
        }

        public async Task<IList<string>> GetPermissionsForUserAsync(string plantId, Guid userOid)
            => await _cacheManager.GetOrCreate(
                PermissionsCacheKey(plantId, userOid),
                async () => await _permissionApiService.GetPermissionsAsync(plantId),
                CacheDuration.Minutes,
                _options.CurrentValue.PermissionCacheMinutes);

        public async Task<IList<string>> GetProjectsForUserAsync(string plantId, Guid userOid)
        {
            var allProjects = await GetAllProjectsForUserAsync(plantId, userOid);
            return allProjects?.Where(p => p.HasAccess).Select(p => p.Name).ToList();
        }

        public async Task<bool> IsAValidProjectAsync(string plantId, Guid userOid, string projectName)
        {
            var allProjects = await GetAllProjectsForUserAsync(plantId, userOid);
            return allProjects != null && allProjects.Any(p => p.Name == projectName);
        }

        public async Task<IList<string>> GetContentRestrictionsForUserAsync(string plantId, Guid userOid)
            => await _cacheManager.GetOrCreate(
                ContentRestrictionsCacheKey(plantId, userOid),
                async () => await _permissionApiService.GetContentRestrictionsAsync(plantId),
                CacheDuration.Minutes,
                _options.CurrentValue.PermissionCacheMinutes);

        public void ClearAll(string plantId, Guid userOid)
        {
            _cacheManager.Remove(PlantsCacheKey(userOid));
            _cacheManager.Remove(ProjectsCacheKey(plantId, userOid));
            _cacheManager.Remove(PermissionsCacheKey(plantId, userOid));
            _cacheManager.Remove(ContentRestrictionsCacheKey(plantId, userOid));
        }

        private async Task<IList<ProCoSysProject>> GetAllProjectsForUserAsync(string plantId, Guid userOid)
            => await _cacheManager.GetOrCreate(
                ProjectsCacheKey(plantId, userOid),
                async () => await GetAllOpenProjectsAsync(plantId),
                CacheDuration.Minutes,
                _options.CurrentValue.PermissionCacheMinutes);

        private async Task<IList<ProCoSysPlant>> GetAllPlantsForUserAsync(Guid userOid)
            => await _cacheManager.GetOrCreate(
                PlantsCacheKey(userOid),
                async () =>
                {
                    var plants = await _permissionApiService.GetAllPlantsForUserAsync(userOid);
                    return plants;
                },
                CacheDuration.Minutes,
                _options.CurrentValue.PlantCacheMinutes);

        private string PlantsCacheKey(Guid userOid)
            => $"PLANTS_{userOid.ToString().ToUpper()}";

        private string ProjectsCacheKey(string plantId, Guid userOid)
        {
            if (userOid == Guid.Empty)
            {
                throw new Exception("Illegal userOid for cache");
            }
            return $"PROJECTS_{userOid.ToString().ToUpper()}_{plantId}";
        }

        private static string PermissionsCacheKey(string plantId, Guid userOid)
        {
            if (userOid == Guid.Empty)
            {
                throw new Exception("Illegal userOid for cache");
            }
            return $"PERMISSIONS_{userOid.ToString().ToUpper()}_{plantId}";
        }

        private static string ContentRestrictionsCacheKey(string plantId, Guid userOid)
        {
            if (userOid == Guid.Empty)
            {
                throw new Exception("Illegal userOid for cache");
            }
            return $"CONTENTRESTRICTIONS_{userOid.ToString().ToUpper()}_{plantId}";
        }

        private async Task<IList<ProCoSysProject>> GetAllOpenProjectsAsync(string plantId)
            => await _permissionApiService.GetAllOpenProjectsAsync(plantId);
    }
}
