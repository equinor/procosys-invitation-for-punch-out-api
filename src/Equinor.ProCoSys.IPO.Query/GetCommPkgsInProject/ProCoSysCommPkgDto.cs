using System;

namespace Equinor.ProCoSys.IPO.Query.GetCommPkgsInProject
{
    public class ProCoSysCommPkgDto
    {
        public ProCoSysCommPkgDto(
            long id,
            string commPkgNo,
            string description,
            string status,
            string system,
            string operationHandoverStatus,
            DateTime? rfocAcceptedAt)
        {
            Id = id;
            CommPkgNo = commPkgNo;
            Description = description;
            Status = status;
            System = system;
            OperationHandoverStatus = operationHandoverStatus;
            RfocAcceptedAt = rfocAcceptedAt;
        }

        public long Id { get; }
        public string CommPkgNo { get; }
        public string Description { get; }
        public string Status { get; }
        public string System { get; }
        public string OperationHandoverStatus { get; }
        public DateTime? RfocAcceptedAt { get; }
    }
}
