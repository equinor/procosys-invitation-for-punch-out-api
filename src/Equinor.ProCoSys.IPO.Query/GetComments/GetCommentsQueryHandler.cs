using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
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
            CancellationToken token)
        {
            var invitation = await
                (from inv in _context.QuerySet<Invitation>()
                        .Include(i => i.Comments)
                        .Where(i => i.Id == request.InvitationId)
                 select inv).SingleOrDefaultAsync(token);

            if (invitation == null)
            {
                return new NotFoundResult<List<CommentDto>>($"Entity with ID {request.InvitationId} not found");
            }
            var personIds = invitation.Comments.Select(x => x.CreatedById).Distinct();
            var persons = await (from p in _context.QuerySet<Person>()
                where personIds.Contains(p.Id)
                select p).ToListAsync(token);

            var comments = invitation.Comments
                .Select(c => new CommentDto(
                    c.Id,
                    c.CommentText,
                    persons.Select(p => new PersonDto(
                        p.Id,
                        p.FirstName,
                        p.LastName,
                        p.Oid,
                        null,
                        p.RowVersion.ConvertToString())).Single(p => p.Id == c.CreatedById),
                    c.CreatedAtUtc)
                )
                .OrderByDescending(c => c.CreatedAtUtc)
                .ToList();

            return new SuccessResult<List<CommentDto>>(comments);
        }
    }
}
