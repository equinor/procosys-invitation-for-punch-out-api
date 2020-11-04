using Fusion.Integration.Meeting;

namespace Equinor.ProCoSys.IPO.Query
{
    public class PersonDto
    {
        public PersonDto(
            int id,
            string firstName,
            string lastName,
            string azureOid,
            string email)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            AzureOid = azureOid;
            Email = email;
        }

        public int Id { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string AzureOid { get; }
        public string Email { get; }
        public bool Required { get; set; }
        public OutlookResponse? Response { get; set; }
    }
}
