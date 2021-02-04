namespace Equinor.ProCoSys.IPO.Query.GetInvitationById
{
    public class McPkgScopeDto
    {
        public McPkgScopeDto(
            string mcPkgNo,
            string description,
            string commPkgNo)
        {
            McPkgNo = mcPkgNo;
            Description = description;
            CommPkgNo = commPkgNo;
        }

        public string McPkgNo { get; }
        public string Description { get; }
        public string CommPkgNo { get; }
    }
}
