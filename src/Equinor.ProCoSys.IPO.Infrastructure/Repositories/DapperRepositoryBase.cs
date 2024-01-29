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
