using System.Linq;
using System.Net.Sockets;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Threading;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;

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
            CancellationToken token)
        {
            var currentUserOid = _currentUserProvider.GetCurrentUserOid();

            return await (from s in _context.QuerySet<SavedFilter>()
                join p in _context.QuerySet<Person>() on EF.Property<int>(s, "PersonId") equals p.Id
                where p.Oid == currentUserOid
                      && s.Title == title
                      && s.ProjectName == projectName
                select s).AnyAsync(token);
        }
        public async Task<bool> ExistsAnotherWithSameTitleForPersonInProjectAsync(int savedFilterId, string title,
            CancellationToken token)
        {
            var currentUserOid = _currentUserProvider.GetCurrentUserOid();
            var projectName = await (from s in _context.QuerySet<SavedFilter>() 
                    where s.Id == savedFilterId select s.ProjectName).SingleOrDefaultAsync(token);

            return await (from s in _context.QuerySet<SavedFilter>()
                join p in _context.QuerySet<Person>() on EF.Property<int>(s, "PersonId") equals p.Id
                where p.Oid == currentUserOid
                      && s.Title == title
                      && s.ProjectName == projectName
                      && s.Id != savedFilterId
                select s).AnyAsync(token);
        }

        public async Task<bool> ExistsAsync(int savedFilterId, CancellationToken token) =>
            await (from sf in _context.QuerySet<SavedFilter>()
                where sf.Id == savedFilterId
                select sf).AnyAsync(token);
    }
}
