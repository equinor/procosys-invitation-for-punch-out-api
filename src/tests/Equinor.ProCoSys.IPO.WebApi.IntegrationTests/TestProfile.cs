namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests
{
    public class TestProfile
    {
        public string Oid { get; set; }
        public string FullName { get; set; }
        public bool IsAppToken { get; set; } = false;
        public string UserName { get; set; }
        public string Email { get; set; }

        public override string ToString() => $"{FullName} {Oid}";
    }
}
