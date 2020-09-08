namespace Equinor.ProCoSys.IPO.WebApi.Authorizations
{
    public interface IProjectAccessChecker
    {
        bool HasCurrentUserAccessToProject(string projectName);
    }
}
