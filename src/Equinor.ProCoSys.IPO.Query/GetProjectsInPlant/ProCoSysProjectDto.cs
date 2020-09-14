namespace Equinor.ProCoSys.IPO.Query.GetProjectsInPlant
{
    public class ProCoSysProjectDto
    {
        public ProCoSysProjectDto(
            int id,
            string name,
            string description)
        {
            Id = id;
            Name = name;
            Description = description;
        }

        public int Id { get; }
        public string Name { get; }
        public string Description { get; }
    }
}
