namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Invitation
{
    public class PersonDto
    {
        public string AzureOid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool Cc { get; set; }
    }
}
