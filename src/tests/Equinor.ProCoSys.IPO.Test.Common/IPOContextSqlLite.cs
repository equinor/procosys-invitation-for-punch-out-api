using System;
using Dapper;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.IPO.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.Test.Common
{
    public class IPOContextSqlLite : IPOContext
    {
        public IPOContextSqlLite(
            DbContextOptions<IPOContext> options,
            IPlantProvider plantProvider,
            IEventDispatcher eventDispatcher,
            ICurrentUserProvider currentUserProvider)
            : base(options, plantProvider, eventDispatcher, currentUserProvider)
        {
            SqlMapper.AddTypeHandler(typeof(Guid), GuidTypeHandler.Default);
        }

        protected override void UpdateConcurrencyToken()
        {
            //Do nothing
        }
    }
}
