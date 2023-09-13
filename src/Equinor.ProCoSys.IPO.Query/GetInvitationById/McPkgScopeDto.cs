namespace Equinor.ProCoSys.IPO.Query.GetInvitationById
{
    public class McPkgScopeDto
    {
        public McPkgScopeDto(
            string mcPkgNo,
            string description,
            string commPkgNo,
            string system,
            bool rfocAccepted)
        {
            McPkgNo = mcPkgNo;
            Description = description;
            CommPkgNo = commPkgNo;
            System = system;
            RfocAccepted = rfocAccepted;
        }

        public string McPkgNo { get; }
        public string Description { get; }
        public string CommPkgNo { get; }
        public string System { get; }
        public bool RfocAccepted { get; }
    }
}
