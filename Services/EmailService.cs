using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

public class EmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var emailSettings = _config.GetSection("EmailSettings");
        string smtpServer = emailSettings["SMTPServer"];
        int smtpPort = int.Parse(emailSettings["SMTPPort"]);
        string senderEmail = emailSettings["SenderEmail"];
        string senderPassword = emailSettings["SenderPassword"];
        bool enableSSL = bool.Parse(emailSettings["EnableSSL"]);

        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("Your Name", senderEmail));
        email.To.Add(new MailboxAddress("", toEmail));
        email.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = body };
        email.Body = bodyBuilder.ToMessageBody();

        try
        {
            using (var client = new SmtpClient())
            {
                // Ensure a secure connection using StartTLS
                await client.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.StartTls);

                // Authenticate using sender email and password
                await client.AuthenticateAsync(senderEmail, senderPassword);

                // Send the email
                await client.SendAsync(email);

                // Disconnect the client safely
                await client.DisconnectAsync(true);
            }
        }
        catch (Exception ex)
        {
            // Log the error to a file for debugging
            System.IO.File.WriteAllText("email_error_log.txt", ex.ToString());
            throw; // Rethrow exception for debugging in Development mode
        }
    }
}
