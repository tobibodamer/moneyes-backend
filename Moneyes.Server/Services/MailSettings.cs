namespace Moneyes.Server.Services
{
    /// <summary>
    /// Represents the email settings provided by the configuration.
    /// Will be used to send confirmation email etc.
    /// </summary>
    public class MailSettings
    {
        /// <summary>
        /// The from email address, which will also be the user name.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The password for the SMTP server.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The SMTP server host name.
        /// </summary>
        public string SmtpServer { get; set; }

        /// <summary>
        /// The SMTP server port.
        /// </summary>
        public int SmtpPort { get; set; }
    }
}
