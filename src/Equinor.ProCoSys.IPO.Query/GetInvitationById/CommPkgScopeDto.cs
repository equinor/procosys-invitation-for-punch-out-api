namespace Equinor.ProCoSys.IPO.Query.GetInvitationById
{
    public class CommPkgScopeDto
    {
        public CommPkgScopeDto(
            string commPkgNo,
            string description,
            string status)
        {
            CommPkgNo = commPkgNo;
            Description = description;
            Status = status;
        }

        public string CommPkgNo { get; }
        public string Description { get; }
        public string Status { get; }
    }
}
