using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.CommPkg;
using Equinor.ProCoSys.IPO.ForeignApi.MainApi.McPkg;
using Fusion.Integration.Meeting;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.InvitationCommands.FillRfocStatuses
{
    public class FillRfocStatusesCommandHandler : IRequestHandler<FillRfocStatusesCommand, Result<string>>
    {
        private readonly IInvitationRepository _invitationRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IMcPkgApiService _mcPkgApiService;
        private readonly ICommPkgApiService _commPkgApiService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly ILogger<FillRfocStatusesCommandHandler> _logger;

        public FillRfocStatusesCommandHandler(
            IInvitationRepository invitationRepository,
            IPersonRepository personRepository,
            IUnitOfWork unitOfWork,
            ICurrentUserProvider currentUserProvider,
            ILogger<FillRfocStatusesCommandHandler> logger,
            IProjectRepository projectRepository)
        {
            _invitationRepository = invitationRepository;
            _personRepository = personRepository;
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
            _logger = logger;
            _projectRepository = projectRepository;
        }

        public async Task<Result<string>> Handle(FillRfocStatusesCommand request, CancellationToken cancellationToken)
        {
            // THIS CODE WAS WRITTEN TO RUN A ONETIME TRANSFORMATION WHEN WE INTRODUCED RfocAccepted on McPkgs and CommPkgs,
            // and new status (ScopeHandedOver) on invitation
            // WE KEEP THE CODE ... MAYBE WE WANT TO DO SIMILAR STUFF LATER

            var allProjects = await _projectRepository.GetAllAsync();
            var invitations = await _invitationRepository.GetAllAsync();

            var count = 0;

            
            return new SuccessResult<Unit>(Unit.Value);
        }
    }
}
