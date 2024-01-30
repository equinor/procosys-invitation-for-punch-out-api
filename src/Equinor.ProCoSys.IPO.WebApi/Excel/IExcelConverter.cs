using System.IO;
using Equinor.ProCoSys.IPO.Query.GetInvitationsQueries.GetInvitationsForExport;

namespace Equinor.ProCoSys.IPO.WebApi.Excel
{
    public interface IExcelConverter
    {
        MemoryStream Convert(ExportDto dto);
        string GetFileName();
    }
}
