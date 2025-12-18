using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Domain.AggregateModels.SettingAggregate;
using Microsoft.EntityFrameworkCore;

namespace Equinor.ProCoSys.IPO.Infrastructure.Repositories
{
    public class SettingRepository : RepositoryBase<Setting>, ISettingRepository
    {
        public SettingRepository(IPOContext context) : base(context, context.Setting)
        {
        }

        public Task<Setting> GetByCodeAsync(string settingCode)
            => DefaultQuery.SingleOrDefaultAsync(r => r.Code == settingCode);
    }
}
