namespace Keroosha.GraphQL.Web.Config
{
    public class SmtpOptions
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 587;
        public bool Auth { get; set; } = false;
        public string Username { get; set; } = "postmaster@example.org";
        public string Password { get; set; } = "test";
    }

    public class EmailOptions
    {
        public string FromEmail { get; set; } = "noreply@example.org";
        public string Name { get; set; } = "App";
        public string BaseUrl { get; set; } = "https://example.org";
        public SmtpOptions SMTP { get; set; } = new();
    }
}