using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate;
using MediatR;
using Microsoft.EntityFrameworkCore;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetComments
{
    public class GetCommentsQueryHandler : IRequestHandler<GetCommentsQuery, Result<List<CommentDto>>>
    {
        private readonly IReadOnlyContext _context;

        public GetCommentsQueryHandler(IReadOnlyContext context) 
            => _context = context;

        public async Task<Result<List<CommentDto>>> Handle(
            GetCommentsQuery request,
            CancellationToken cancellationToken)
        {
            var invitation = await
                (from inv in _context.QuerySet<Invitation>()
                        .Include(i => i.Comments)
                        .Where(i => i.Id == request.InvitationId)
                 select inv).SingleOrDefaultAsync(cancellationToken);

            if (invitation == null)
            {
                return new NotFoundResult<List<CommentDto>>(Strings.EntityNotFound(nameof(Invitation), request.InvitationId));
            }
            var personIds = invitation.Comments.Select(x => x.CreatedById).Distinct();
            var persons = await (from p in _context.QuerySet<Person>()
                where personIds.Contains(p.Id)
                select p).ToListAsync(cancellationToken);

            var comments = invitation.Comments
                .Select(c => new CommentDto(
                    c.Id,
                    c.CommentText,
                    persons.Select(p => new PersonDto(
                        p.Id,
                        p.FirstName,
                        p.LastName,
                        p.UserName,
                        p.Oid,
                        p.Email,
                        p.RowVersion.ConvertToString())).Single(p => p.Id == c.CreatedById),
                    c.CreatedAtUtc,
                    c.RowVersion.ConvertToString())
                )
                .OrderByDescending(c => c.CreatedAtUtc)
                .ToList();

            return new SuccessResult<List<CommentDto>>(comments);
        }
    }
}
