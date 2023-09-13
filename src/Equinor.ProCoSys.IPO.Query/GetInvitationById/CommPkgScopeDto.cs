namespace Equinor.ProCoSys.IPO.Query.GetInvitationById
{
    public class CommPkgScopeDto
    {
        public CommPkgScopeDto(
            string commPkgNo,
            string description,
            string status,
            string system,
            bool rfocAccepted)
        {
            CommPkgNo = commPkgNo;
            Description = description;
            Status = status;
            System = system;
            RfocAccepted = rfocAccepted;
        }

        public string CommPkgNo { get; }
        public string Description { get; }
        public string Status { get; }
        public string System { get; }
        public bool RfocAccepted { get; }
    }
}
