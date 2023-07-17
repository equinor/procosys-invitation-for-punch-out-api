namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests.Invitations.GetInvitation
{
    public class CommPkgScopeDto
    {
        public string CommPkgNo { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string System { get; set; }
        public bool RfocAccepted { get; }
    }
}
