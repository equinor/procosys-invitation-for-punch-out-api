namespace Equinor.ProCoSys.IPO.Command.Email
{
    public class SmtpOptions
    {
        public string Server { get; set; }
        public int Port { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool EnableSSL { get; set; }
        public bool FakeEmail { get; set; }
    }
}
