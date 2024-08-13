namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.Fam.Queries;

public static class CommentQuery
{
    public static string Query => @$"
            SELECT 
                  c.Guid,
                  c.CommentText,
                  c.CreatedAtUtc,
	              person.Guid as CreatedByGuid,
                  i.Guid as InvitationGuid,
                  c.Plant,
	              project.Name as ProjectName                 
              FROM Comments c
              JOIN Invitations i on i.Id = c.InvitationId
              JOIN Projects project on project.Id = i.ProjectId
              JOIN Persons person on person.Id = c.CreatedById
            ";
}
