namespace Equinor.ProCoSys.IPO.Query.GetMcPkgsUnderCommPkgInProject
{
    public class ProCoSysMcPkgDto
    {
        public ProCoSysMcPkgDto(
            long id,
            string mcPkgNo,
            string description,
            string disciplineCode,
            string commPkgNo,
            string system)
        {
            Id = id;
            McPkgNo = mcPkgNo;
            Description = description;
            DisciplineCode = disciplineCode;
            System = system;
        }

        public long Id { get; }
        public string McPkgNo { get; }
        public string Description { get; }
        public string DisciplineCode { get; }
        public string System { get; }
    }
}
