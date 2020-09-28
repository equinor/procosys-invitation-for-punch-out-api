namespace Equinor.ProCoSys.IPO.WebApi.Authentication
{
    public class AuthenticatorOptions
    {
        public string Instance { get; set; }

        public string IPOApiClientId { get; set; }
        public string IPOApiSecret { get; set; }

        public string MainApiClientId { get; set; }
        public string MainApiSecret { get; set; }
        public string MainApiScope { get; set; }

        public string LibraryApiScope { get; set; }
    }
}
