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

            var commentDtos = new List<CommentDto>();
            foreach (var comment in invitation.Comments)
            {
                var createdBy = await
                    (from person in _context.QuerySet<Person>()
                            .Where(p => p.Id == comment.CreatedById)
                     select person).SingleOrDefaultAsync(token);

                if (createdBy == null)
                {
                    return new NotFoundResult<List<CommentDto>>($"Person with ID {comment.CreatedById} not found");
                }

                commentDtos.Add(new CommentDto(
                    comment.Id,
                    comment.CommentText,
                    new PersonMinimalDto(createdBy.Id, createdBy.FirstName, createdBy.LastName),
                    comment.CreatedAtUtc));
            }

            var orderedCommentDtos = commentDtos.OrderByDescending(c => c.CreatedAtUtc).ToList();

            return new SuccessResult<List<CommentDto>>(orderedCommentDtos);
        }
    }
}
