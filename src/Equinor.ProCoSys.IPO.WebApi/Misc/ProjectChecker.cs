using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.IPO.Domain;
using MediatR;

namespace Equinor.ProCoSys.IPO.WebApi.Misc
{
    public class ProjectChecker : IProjectChecker
    {
        private readonly IPlantProvider _plantProvider;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IPermissionCache _permissionCache;

        public ProjectChecker(IPlantProvider plantProvider,
            ICurrentUserProvider currentUserProvider,
            IPermissionCache permissionCache)
        {
            _plantProvider = plantProvider;
            _currentUserProvider = currentUserProvider;
            _permissionCache = permissionCache;
        }

        public async Task EnsureValidProjectAsync<TRequest>(TRequest request) where TRequest : IBaseRequest
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var plant = _plantProvider.Plant;
            var userOid = _currentUserProvider.GetCurrentUserOid();

            if (request is IProjectRequest projectRequest && projectRequest.ProjectName != null && !await _permissionCache.IsAValidProjectForUserAsync(plant, userOid, projectRequest.ProjectName))
            {
                throw new InValidProjectException($"Project '{projectRequest.ProjectName}' is not a valid project in '{plant}'");
            }
        }
    }
}
