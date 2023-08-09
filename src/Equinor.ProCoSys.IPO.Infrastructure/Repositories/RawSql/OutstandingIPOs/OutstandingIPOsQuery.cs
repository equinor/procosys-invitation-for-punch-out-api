namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories.RawSql.OutstandingIPOs
{
    public static class OutstandingIPOsQuery
    {
        private static string CreateBaseQuery()
        {
            var query = @$"
                SELECT [i].[Id], [i].[Description], [i].[Status],
                [p].[Id] AS [ParticipantId], [p].[AzureOid], [p].[FunctionalRoleCode], [p].[Organization], [p].[SignedAtUtc], [p].[SortKey], [p].[Type] 
                FROM [Invitations] AS [i]

                INNER JOIN [Participants] AS [p] ON [i].[Id] = [p].[InvitationId] AND [p].[Plant] = @plant

                INNER JOIN [Projects] AS [pro]  ON [i].[ProjectId] = [pro].[Id] AND [pro].[Plant] = @plant             

                WHERE [i].[Plant] = @plant
                AND [i].[Status]<>(3) AND [i].[Status]<>(4)
                AND [pro].[IsClosed] = 0
                AND (
                    ([p].[SignedAtUtc] IS NULL AND [p].[Organization] <> 5 AND [p].[Organization] <> 6 AND [p].[SortKey] <> 1)
                    OR 
                    ([p].[SortKey] = 1 AND [i].[Status] = 1)
                    )";
            return query;
        }

        public static string CreateAzureOidQuery()
        {
            var query = CreateBaseQuery();
            query += @$"
                        AND [p].[AzureOid] = @azureOid AND [p].[FunctionalRoleCode] IS NULL ORDER BY [i].[Id]";
            return query;
        }

        public static string CreateFunctionalRoleQuery()
        {
            var query = CreateBaseQuery();
            query += @$" 
                        AND [p].[Type] = 1 AND [p].[FunctionalRoleCode] IN @functionalRoleCodes ORDER BY [i].[Id]";
            return query;
        }
    }
}
