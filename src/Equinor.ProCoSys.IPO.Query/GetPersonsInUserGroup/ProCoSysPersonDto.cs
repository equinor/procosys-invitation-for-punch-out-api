namespace Equinor.ProCoSys.IPO.Query.GetPersonsInUserGroup
{
    public class ProCoSysPersonDto
    {
        public ProCoSysPersonDto(
            string oid,
            string userName,
            string firstName,
            string lastName,
            string email)
        {
            Oid = oid;
            UserName = userName;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }

        public string Oid { get; }
        public string UserName { get; }
        public string FirstName { get; }
        public string LastName { get; }
        public string Email { get; }
    }
}
