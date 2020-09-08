namespace Equinor.ProCoSys.IPO.WebApi.Authorizations
{
    public interface IContentRestrictionsChecker
    {
        bool HasCurrentUserExplicitNoRestrictions();
        bool HasCurrentUserExplicitAccessToContent(string responsibleCode);
    }
}
