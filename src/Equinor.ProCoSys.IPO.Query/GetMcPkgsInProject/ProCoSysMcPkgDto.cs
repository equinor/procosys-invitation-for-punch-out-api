namespace Equinor.ProCoSys.IPO.Query.GetMcPkgsInProject
{
    public class ProCoSysMcPkgDto
    {
        public ProCoSysMcPkgDto(
            long id,
            string mcPkgNo,
            string description)
        {
            Id = id;
            McPkgNo = mcPkgNo;
            Description = description;
        }

        public long Id { get; }
        public string McPkgNo { get; }
        public string Description { get; }
    }
}
