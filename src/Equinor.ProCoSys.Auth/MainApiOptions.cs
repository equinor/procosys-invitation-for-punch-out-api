namespace Equinor.ProCoSys.Auth
{
    public class MainApiOptions
    {
        public string ApiVersion { get; set; }
        public string BaseAddress { get; set; }
        public string ClientFriendlyName { get; set; } = "ProCoSys - External API";
    }
}
