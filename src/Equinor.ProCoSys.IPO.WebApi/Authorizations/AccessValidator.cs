using System;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Misc;
using Equinor.ProCoSys.IPO.Command;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Query;
using Equinor.ProCoSys.IPO.WebApi.Misc;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.IPO.WebApi.Authorizations
{
    public class AccessValidator : IAccessValidator
    {
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IProjectAccessChecker _projectAccessChecker;
        private readonly IInvitationHelper _invitationHelper;
        private readonly ILogger<AccessValidator> _logger;

        public AccessValidator(
            ICurrentUserProvider currentUserProvider,
            IProjectAccessChecker projectAccessChecker,
            IInvitationHelper invitationHelper,
            ILogger<AccessValidator> logger)
        {
            _currentUserProvider = currentUserProvider;
            _projectAccessChecker = projectAccessChecker;
            _invitationHelper = invitationHelper;
            _logger = logger;
        }

        public async Task<bool> ValidateAsync<TRequest>(TRequest request) where TRequest : IBaseRequest
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var userOid = _currentUserProvider.GetCurrentUserOid();
            if (request is IProjectRequest projectRequest &&
                projectRequest.ProjectName != null &&
                !_projectAccessChecker.HasCurrentUserAccessToProject(projectRequest.ProjectName))
            {
                _logger.LogWarning($"Current user {userOid} don't have access to project {projectRequest.ProjectName}");
                return false;
            }



            if (request is IInvitationCommandRequest invitationCommandRequest)
            {
                if (!await HasCurrentUserAccessToProjectAsync(invitationCommandRequest.InvitationId, userOid))
                {
                    return false;
                }
            }

            if (request is IInvitationQueryRequest invitationQueryRequest)
            {
                if (!await HasCurrentUserAccessToProjectAsync(invitationQueryRequest.InvitationId, userOid))
                {
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> HasCurrentUserAccessToProjectAsync(int invitationId, Guid userOid)
        {
            var projectName = await _invitationHelper.GetProjectNameAsync(invitationId);
            if (projectName != null)
            {
                var accessToProject = _projectAccessChecker.HasCurrentUserAccessToProject(projectName);

                if (!accessToProject)
                {
                    _logger.LogWarning($"Current user {userOid} don't have access to project {projectName}");
                    return false;
                }
            }

            return true;
        }
    }
}
