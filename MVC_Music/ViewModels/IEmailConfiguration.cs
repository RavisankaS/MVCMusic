namespace MVC_Music.ViewModels
{
    public interface IEmailConfiguration
    {
        string SmtpServer { get; set; }
        int SmtpPort { get; set; }
        string SmtpFromName { get; set; }
        string SmtpUsername { get; set; }
        string SmtpPassword { get; set; }
    }
}
