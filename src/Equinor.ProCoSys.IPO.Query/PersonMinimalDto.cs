namespace Equinor.ProCoSys.IPO.Query
{
    public class PersonMinimalDto
    {
        public PersonMinimalDto(
            int id,
            string firstName,
            string lastName)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
        }

        public int Id { get; }
        public string FirstName { get; }
        public string LastName { get; }
    }
}
