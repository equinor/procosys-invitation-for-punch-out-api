namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.RawSql.OutstandingIPOs
{
    public static class OutstandingIPOsQuery
    {

        private static string CreateBaseQuery()
        {
            var query = @$"
                SELECT [i].[Id], [i].[Description], [i].[Status], [p].[Id] AS [ParticipantId], [p].[AzureOid], [p].[FunctionalRoleCode], [p].[Organization], [p].[SignedAtUtc], [p].[SortKey], [p].[Type] 
                FROM [Invitations] AS [i]

                INNER JOIN (
                    SELECT [p].[Id], [p].[AzureOid], [p].[FunctionalRoleCode], [p].[InvitationId], [p].[Organization], [p].[SignedAtUtc], [p].[SortKey], [p].[Type]
                    FROM [Participants] 
                    AS [p]
                    WHERE [p].[Plant] = @plant
                ) AS [p] 
                ON [i].[Id] = [p].[InvitationId] 

                INNER JOIN (
                    SELECT [p0].[Id], [p0].[IsClosed] FROM [Projects] AS [p0] WHERE [p0].[Plant] = @plant
                ) AS [pro] 
                ON [i].[ProjectId] = [pro].[Id]

                WHERE [i].[Plant] = @plant
                AND [i].[Status]<>(3) AND [i].[Status]<>(4) AND [pro].[IsClosed] = CAST(0 AS bit) AND ((([p].[SignedAtUtc] IS NULL) AND [p].[Organization] <> 5 AND [p].[Organization] <> 6 AND [p].[SortKey] <> 1) OR ([p].[SortKey] = 1 AND [i].[Status] = 1)) ";
            return query;
        }

        public static string CreateAzureOidQuery()
        {
            var query = CreateBaseQuery();
            query += $" AND [p].[AzureOid] = @azureOid AND [p].[FunctionalRoleCode] IS NULL ORDER BY [i].[Id]";
            return query;
        }

        public static string CreateFunctionalRoleQuery()
        {
            var query = CreateBaseQuery();
            query += $" AND [p].[Type] = 1 AND [p].[FunctionalRoleCode] IN @functionalRoleCodes ORDER BY [i].[Id]";
            return query;
        }
    }
}
