namespace Equinor.ProCoSys.IPO.Query.GetMcPkgsUnderCommPkgInProject
{
    public class ProCoSysMcPkgDto
    {
        public ProCoSysMcPkgDto(
            long id,
            string mcPkgNo,
            string description,
            string disciplineCode)
        {
            Id = id;
            McPkgNo = mcPkgNo;
            Description = description;
            DisciplineCode = disciplineCode;
        }

        public long Id { get; }
        public string McPkgNo { get; }
        public string Description { get; }
        public string DisciplineCode { get; }
    }
}
