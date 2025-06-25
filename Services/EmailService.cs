using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MyProject.Domain.Emails;

namespace MyProject.Services;

public class EmailService(IOptions<EmailSettings> emailSettings)
{
    private readonly EmailSettings _emailSettings = emailSettings.Value;
    public async Task SendEmailAsync(EmailRequest request)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_emailSettings.Name, _emailSettings.FromEmail));
        email.To.Add(new MailboxAddress("", request.ToEmail));

        if (request.Cc.Count != 0)
        {
            foreach (var ccEmail in request.Cc)
            {
                email.Cc.Add(new MailboxAddress("", ccEmail));
            }
        }

        if (request.Bcc.Count != 0)
        {
            foreach (var bccEmail in request.Bcc)
            {
                email.Bcc.Add(new MailboxAddress("", bccEmail));
            }
        }

        
        email.Subject = request.Subject;

        email.Body = new TextPart("html")
        {
            Text = request.Body
        };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_emailSettings.FromEmail, _emailSettings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
