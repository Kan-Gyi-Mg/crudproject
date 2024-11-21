using MailKit.Net.Smtp;
using MimeKit;

public class EmailService
{
    private readonly string _fromEmail = "darkangyimg15@gmail.com";
    private readonly string _fromEmailPassword = "vqtmmyryzryecwfr";
    private readonly string _smtpHost = "smtp.gmail.com";
    private readonly int _smtpPort = 587;

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("CoCo", _fromEmail));
        emailMessage.To.Add(new MailboxAddress("", toEmail));
        emailMessage.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = message };
        emailMessage.Body = bodyBuilder.ToMessageBody();

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_smtpHost, _smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_fromEmail, _fromEmailPassword);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}
