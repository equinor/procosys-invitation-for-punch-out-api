using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Command;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Query;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations
{
    public class AccessValidator : IAccessValidator
    {
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IProjectAccessChecker _projectAccessChecker;
        private readonly ILogger<AccessValidator> _logger;

        public AccessValidator(
            ICurrentUserProvider currentUserProvider, 
            IProjectAccessChecker projectAccessChecker,
            ILogger<AccessValidator> logger)
        {
            _currentUserProvider = currentUserProvider;
            _projectAccessChecker = projectAccessChecker;
            _logger = logger;
        }

        public async Task<bool> ValidateAsync<TRequest>(TRequest request) where TRequest : IBaseRequest
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var userOid = _currentUserProvider.GetCurrentUserOid();
            if (request is IProjectRequest projectRequest && !_projectAccessChecker.HasCurrentUserAccessToProject(projectRequest.ProjectName))
            {
                _logger.LogWarning($"Current user {userOid} don't have access to project {projectRequest.ProjectName}");
                return false;
            }


            if (request is IIPOCommandRequest ipoCommandRequest)
            {
                var projectName = "Todo"; // todo await _ipoHelper.GetProjectNameAsync(ipoCommandRequest.IPOId);
                var accessToProject = _projectAccessChecker.HasCurrentUserAccessToProject(projectName);

                if (!accessToProject)
                {
                    _logger.LogWarning($"Current user {userOid} don't have access to project {projectName}");
                }
                return accessToProject;
            }

            if (request is IIPOQueryRequest ipoQueryRequest)
            {
                var projectName = "Todo"; // todo await _ipoHelper.GetProjectNameAsync(ipoCommandRequest.IPOId);
                if (!_projectAccessChecker.HasCurrentUserAccessToProject(projectName))
                {
                    _logger.LogWarning($"Current user {userOid} don't have access to project {projectName}");
                    return false;
                }
            }

            return true;
        }
    }
}
