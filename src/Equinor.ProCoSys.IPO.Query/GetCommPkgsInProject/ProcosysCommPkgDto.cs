namespace Equinor.ProCoSys.IPO.Query.GetCommPkgsInProject
{
    public class ProCoSysCommPkgDto
    {
        public ProCoSysCommPkgDto(
            long id,
            string commPkgNo,
            string description,
            string status,
            string system)
        {
            Id = id;
            CommPkgNo = commPkgNo;
            Description = description;
            Status = status;
            System = system;
        }

        public long Id { get; }
        public string CommPkgNo { get; }
        public string Description { get; }
        public string Status { get; }
        public string System { get; }
    }
}
