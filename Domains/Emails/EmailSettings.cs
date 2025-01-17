namespace MyProject.Domain.Emails;

public class EmailSettings
{
    public string Name { get; set; }
    public string FromEmail { get; set; }
    public string Password { get; set; }
    public string SmtpServer { get; set; }
    public int Port { get; set; }
}