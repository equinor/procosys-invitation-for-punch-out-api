using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories;

public class DapperRepositoryBase
{
    // HACK: This should normally be private since we do not want to expose DbContext internals.
    // However, we here have one case where we need to use some of the Context methods to be able to call and handle results from a specific stored procedure.
    // FFE: We can evaluate if it is possible to move sufficient amount of logic into this class from ExportIpoRepository to make IPOContext private again.
    public readonly IPOContext Context;

    public DapperRepositoryBase(IPOContext context) => Context = context;

    protected async Task<IEnumerable<T>> QueryAsync<T>(string query, DynamicParameters parameters)
    {
        var connection = Context.Database.GetDbConnection();
        var connectionWasClosed = connection.State != ConnectionState.Open;

        try
        {
            return await connection.QueryAsync<T>(query, parameters);
        }
        catch (TimeoutException ex)
        {
            throw new Exception($"{GetType().FullName} Query timed out. See inner exception for details", ex);
        }
        catch (SqlException ex)
        {
            throw new Exception($"{GetType().FullName} SqlException. See inner exception for details.", ex);
        }
        finally
        {
            //If we open it, we have to close it.
            if (connectionWasClosed)
            {
                await Context.Database.CloseConnectionAsync();
            }
        }
    }
}
