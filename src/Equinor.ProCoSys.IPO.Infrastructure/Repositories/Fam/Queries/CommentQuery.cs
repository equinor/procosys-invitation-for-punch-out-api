namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.Fam.Queries;

public static class CommentQuery
{
    public static string Query => @$"
            SELECT 
                  c.CommentText,
                  c.CreatedAtUtc,
                  c.CreatedById,
                  i.Guid as InvitationGuid,
                  c.Plant,
	              project.Name as ProjectName,
                  c.Guid as ProCoSysGuid,
	              person.Guid as CreatedByOid
              FROM Comments c
              JOIN Invitations i on i.Id = c.InvitationId
              JOIN Projects project on project.Id = c.InvitationId
              JOIN Persons person on person.Id = c.CreatedById
            ";
}
