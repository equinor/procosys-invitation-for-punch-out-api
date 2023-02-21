using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Command.PersonCommands.UpdateSavedFilter
{
    public class UpdateSavedFilterCommandHandler : IRequestHandler<UpdateSavedFilterCommand, Result<string>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserProvider _currentUserProvider;
        private readonly IPersonRepository _personRepository;
        private readonly IProjectRepository _projectRepository;

        public UpdateSavedFilterCommandHandler(
            IUnitOfWork unitOfWork,
            ICurrentUserProvider currentUserProvider,
            IPersonRepository personRepository,
            IProjectRepository projectRepository)
        {
            _unitOfWork = unitOfWork;
            _currentUserProvider = currentUserProvider;
            _personRepository = personRepository;
            _projectRepository = projectRepository;
        }

        public async Task<Result<string>> Handle(UpdateSavedFilterCommand request, CancellationToken cancellationToken)
        {
            var currentUserOid = _currentUserProvider.GetCurrentUserOid();
            var person = await _personRepository.GetWithSavedFiltersByOidAsync(currentUserOid);
            var savedFilter = person.SavedFilters.Single(sf => sf.Id == request.SavedFilterId);

            if (request.DefaultFilter == true)
            {
                var project = await _projectRepository.GetByIdAsync(savedFilter.ProjectId);

                var currentDefaultFilter = person.GetDefaultFilter(project);
                if (currentDefaultFilter != null)
                {
                    currentDefaultFilter.DefaultFilter = false;
                }
            }

            savedFilter.Title = request.Title;
            savedFilter.Criteria = request.Criteria;
            if (request.DefaultFilter.HasValue)
            {
                savedFilter.DefaultFilter = request.DefaultFilter.Value;
            }
            savedFilter.SetRowVersion(request.RowVersion);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new SuccessResult<string>(savedFilter.RowVersion.ConvertToString());
        }
    }
}
