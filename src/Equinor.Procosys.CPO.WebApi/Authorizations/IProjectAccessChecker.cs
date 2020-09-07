namespace Equinor.Procosys.CPO.WebApi.Authorizations
{
    public interface IProjectAccessChecker
    {
        bool HasCurrentUserAccessToProject(string projectName);
    }
}
