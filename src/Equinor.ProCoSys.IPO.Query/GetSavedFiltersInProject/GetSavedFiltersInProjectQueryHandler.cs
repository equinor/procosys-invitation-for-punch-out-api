using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
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

            if(request.ProjectName == null)
            {
                var savedFilterDtosWithNoProjectCriteria = person.SavedFilters.Where(sf => sf.ProjectId == null)
                .Select(savedFilter => new SavedFilterDto(
                    savedFilter.Id,
                    savedFilter.Title,
                    savedFilter.Criteria,
                    savedFilter.DefaultFilter,
                    savedFilter.CreatedAtUtc,
                    savedFilter.RowVersion.ConvertToString())).ToList();

                return new SuccessResult<List<SavedFilterDto>>(savedFilterDtosWithNoProjectCriteria);
            }

            var projectFromRequest = await (from pro in _context.QuerySet<Project>() 
                where pro.Name.Equals(request.ProjectName)
                select pro).SingleOrDefaultAsync(cancellationToken);

            if (projectFromRequest is null)
            {
                return EmptyList();
            }

            var savedFilterDtos = person.SavedFilters.Where(sf => sf.ProjectId == projectFromRequest.Id)
                .Select(savedFilter => new SavedFilterDto(
                    savedFilter.Id, 
                    savedFilter.Title, 
                    savedFilter.Criteria, 
                    savedFilter.DefaultFilter, 
                    savedFilter.CreatedAtUtc, 
                    savedFilter.RowVersion.ConvertToString())).ToList();

            return new SuccessResult<List<SavedFilterDto>>(savedFilterDtos);
        }

        private static SuccessResult<List<SavedFilterDto>> EmptyList() => new(new List<SavedFilterDto>());
    }
}
