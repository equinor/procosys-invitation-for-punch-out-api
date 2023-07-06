using System;

namespace Equinor.ProCoSys.IPO.Query.GetMcPkgsUnderCommPkgInProject
{
    public class ProCoSysMcPkgDto
    {
        public ProCoSysMcPkgDto(
            long id,
            string mcPkgNo,
            string description,
            string disciplineCode,
            string system,
            string operationHandoverStatus,
            DateTime? m01,
            DateTime? m02)
        {
            Id = id;
            McPkgNo = mcPkgNo;
            Description = description;
            DisciplineCode = disciplineCode;
            System = system;
            OperationHandoverStatus = operationHandoverStatus;
            M01 = m01;
            M02 = m02;
        }

        public long Id { get; }
        public string McPkgNo { get; }
        public string Description { get; }
        public string DisciplineCode { get; }
        public string System { get; }
        public string OperationHandoverStatus { get; }
        public DateTime? M01 { get; }
        public DateTime? M02 { get; }
    }
}
