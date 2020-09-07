namespace Equinor.Procosys.CPO.WebApi.Authorizations
{
    public interface IContentRestrictionsChecker
    {
        bool HasCurrentUserExplicitNoRestrictions();
        bool HasCurrentUserExplicitAccessToContent(string responsibleCode);
    }
}
