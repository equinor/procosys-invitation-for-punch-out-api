using ClosedXML.Excel;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class XLFile
    {
        public XLWorkbook Workbook { get; set; }
        public string ContentType { get; set; }
    }
}
