using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Threading;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.IPO.Command.Validators.SavedFilterValidators
{
    public class SavedFilterValidator : ISavedFilterValidator
    {
        private readonly IReadOnlyContext _context;
        private readonly ICurrentUserProvider _currentUserProvider;

        public SavedFilterValidator(
            IReadOnlyContext context,
            ICurrentUserProvider currentUserProvider)
        {
            _context = context;
            _currentUserProvider = currentUserProvider;
        }

        public async Task<bool> ExistsWithSameTitleForPersonInProjectAsync(
            string title,
            string projectName,
            CancellationToken cancellationToken)
        {
            var currentUserOid = _currentUserProvider.GetCurrentUserOid();

            var project = await _context.QuerySet<Project>().SingleOrDefaultAsync(x => x.Name.Equals(projectName), cancellationToken);

            return await (from s in _context.QuerySet<SavedFilter>()
                join p in _context.QuerySet<Person>() on EF.Property<int>(s, "PersonId") equals p.Id
                where p.Oid == currentUserOid
                      && s.Title == title
                      && project != null && s.ProjectId == project.Id
                select s).AnyAsync(cancellationToken);
        }

        public async Task<bool> ExistsAnotherWithSameTitleForPersonInProjectAsync(int savedFilterId, string title,
            CancellationToken cancellationToken)
        {
            var currentUserOid = _currentUserProvider.GetCurrentUserOid();
            var projectForSavedFilter = await (from s in _context.QuerySet<SavedFilter>()
                    join p in _context.QuerySet<Project>() on s.ProjectId equals p.Id
                    where s.Id == savedFilterId
                    select p)
                .SingleOrDefaultAsync(cancellationToken);

            return await (from s in _context.QuerySet<SavedFilter>()
                join p in _context.QuerySet<Person>() on EF.Property<int>(s, "PersonId") equals p.Id
                where p.Oid == currentUserOid
                      && s.Title == title
                      && projectForSavedFilter != null && s.ProjectId == projectForSavedFilter.Id
                      && s.Id != savedFilterId
                select s).AnyAsync(cancellationToken);
        }

        public async Task<bool> ExistsAsync(int savedFilterId, CancellationToken cancellationToken) =>
            await (from sf in _context.QuerySet<SavedFilter>()
                where sf.Id == savedFilterId
                select sf).AnyAsync(cancellationToken);
    }
}
