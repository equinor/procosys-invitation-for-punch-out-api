﻿namespace Equinor.ProCoSys.IPO.Query.GetCommPkgsInProject
{
    public class ProcosysCommPkgDto
    {
        public ProcosysCommPkgDto(
            long id,
            string commPkgNo,
            string description,
            string status)
        {
            Id = id;
            CommPkgNo = commPkgNo;
            Description = description;
            Status = status;
        }

        public long Id { get; }
        public string CommPkgNo { get; }
        public string Description { get; }
        public string Status { get; }
    }
}