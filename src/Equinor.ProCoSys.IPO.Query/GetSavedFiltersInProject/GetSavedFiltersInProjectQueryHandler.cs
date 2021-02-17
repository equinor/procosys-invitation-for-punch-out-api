using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetSavedFiltersInProject
{
    public class GetSavedFiltersInProjectQueryHandler: IRequestHandler<GetSavedFiltersInProjectQuery, Result<List<SavedFilterDto>>>
    {
        private readonly IReadOnlyContext _context;
        private readonly ICurrentUserProvider _currentUserProvider;

        public GetSavedFiltersInProjectQueryHandler(
            IReadOnlyContext context,
            ICurrentUserProvider currentUserProvider)
        {
            _context = context;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<Result<List<SavedFilterDto>>> Handle(GetSavedFiltersInProjectQuery request,
            CancellationToken cancellationToken)
        {
            var currentUserOid = _currentUserProvider.GetCurrentUserOid(); 
            var person = await (from p in _context.QuerySet<Person>().Include(p => p.SavedFilters) 
                where p.Oid == currentUserOid 
                select p).SingleAsync(cancellationToken);

            var savedFilterDtos = person.SavedFilters.Where(sf => sf.ProjectName == request.ProjectName)
                .Select(savedFilter => new SavedFilterDto(
                    savedFilter.Id, 
                    savedFilter.Title, 
                    savedFilter.Criteria, 
                    savedFilter.DefaultFilter, 
                    savedFilter.CreatedAtUtc, 
                    savedFilter.RowVersion.ConvertToString())).ToList();

            return new SuccessResult<List<SavedFilterDto>>(savedFilterDtos);
        }
    }
}
