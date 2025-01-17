namespace MyProject.Domain.Emails;

public class EmailRequest
{
    public string ToEmail { get; set; }
    public List<string> Cc { get; set; } = new List<string>();
    public List<string> Bcc { get; set; } = new List<string>();
    public string Subject { get; set; }
    public string Body { get; set; }
}