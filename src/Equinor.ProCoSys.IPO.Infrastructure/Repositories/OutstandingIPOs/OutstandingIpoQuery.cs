namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.OutstandingIPOs
{
    public static class OutstandingIpoQuery
    {
        private static string CreateBaseQuery()
        {
            var query = @$"
                SELECT
                    i.Id,
                    i.Description,
                    i.Status,
                    p.Id AS ParticipantId,
                    p.AzureOid,
                    p.FunctionalRoleCode,
                    p.Organization,
                    p.SignedAtUtc,
                    p.SignedBy,
                    p.SortKey,
                    p.Type 
                FROM Invitations AS i
                    JOIN Participants AS p ON i.Id = p.InvitationId AND p.Plant = @plant
                    JOIN Projects AS pro  ON i.ProjectId = pro.Id AND pro.Plant = @plant
                WHERE i.Plant = @plant
                AND i.[Status] NOT IN (3,4)
                AND pro.IsClosed = 0
                AND (
                    (p.SignedAtUtc IS NULL AND p.Organization NOT IN(5,6) AND p.SortKey <> 1)
                    OR 
                    (p.SortKey = 1 AND i.[Status] = 1)
                    )";
            return query;
        }

        public static string CreateQueryFilteredByAzureOid()
        {
            var query = CreateBaseQuery();
            query += @$"
                        AND p.AzureOid = @azureOid
                        AND p.FunctionalRoleCode IS NULL";
            return query;
        }

        public static string CreateQueryFilteredByFunctionalRole()
        {
            var query = CreateBaseQuery();
            query += @$" 
                        AND p.[Type] = 1
                        AND p.FunctionalRoleCode IN @functionalRoleCodes";
            return query;
        }
    }
}
