namespace Equinor.ProCoSys.IPO.Query.GetPersons
{
    public class ProCoSysPersonDto
    {
        public ProCoSysPersonDto(
            string azureOid,
            string userName,
            string firstName,
            string lastName,
            string email)
        {
            AzureOid = azureOid;
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }

        public string AzureOid { get; }
        public string UserName { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Email { get; }
    }
}
