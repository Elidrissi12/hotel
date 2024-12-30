using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MailKit.Net.Smtp;
using MimeKit;


public class EmailService
{
    private const string SmtpServer = "smtp.gmail.com"; // Serveur SMTP de Gmail
    private const int SmtpPort = 587; // Port SMTP pour TLS
    private const string SmtpUser = "elidrissiabdallah689@gmail.com"; // Votre email
    private const string SmtpPassword = "unqn bqbf zaeh egbz"; // Votre mot de passe

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(new MailboxAddress("HOTEL IMAR", SmtpUser));
        email.To.Add(new MailboxAddress("", toEmail));
        email.Subject = subject;

        email.Body = new TextPart("html")
        {
            Text = body
        };

        using var smtpClient = new SmtpClient();
        try
        {
            await smtpClient.ConnectAsync(SmtpServer, SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await smtpClient.AuthenticateAsync(SmtpUser, SmtpPassword);
            await smtpClient.SendAsync(email);
        }
        catch
        {
            throw; // Gérer les exceptions (exemple : logs)
        }
        finally
        {
            await smtpClient.DisconnectAsync(true);
        }
    }
}

