using Equinor.ProCoSys.IPO.Domain;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.IPO.Query.GetCommPkgsInProject
{
    public class GetCommPkgsInProjectQuery : IProjectRequest, IRequest<Result<ProCoSysCommPkgSearchDto>>
    {
        public GetCommPkgsInProjectQuery(
            string projectName,
            string startsWithCommPkgNo,
            int itemsPerPage,
            int currentPage)
        {
            ProjectName = projectName;
            StartsWithCommPkgNo = startsWithCommPkgNo;
            ItemsPerPage = itemsPerPage;
            CurrentPage = currentPage;
        }

        public string ProjectName { get; }
        public string StartsWithCommPkgNo { get; }
        public int ItemsPerPage { get; }
        public int CurrentPage { get; }
    }
}
