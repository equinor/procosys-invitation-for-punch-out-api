namespace Equinor.ProCoSys.IPO.WebApi.IntegrationTests
{
    public class TestProfile
    {
        public string Oid { get; set; }
        public string FullName { get; set; }
        public bool IsAppToken { get; set; } = false;
     
        public override string ToString() => $"{FullName} {Oid}";
    }
}
