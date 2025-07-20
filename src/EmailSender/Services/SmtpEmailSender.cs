using EmailSender.Models;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Serilog;

namespace EmailSender.Services;
internal class SmtpEmailSender(IOptions<EmailOptions> options)
{
    public async Task SendEmail(string recipientEmail, string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var senderName = options.Value.SenderName;
        var senderEmail = options.Value.SenderEmail;
        var serverHost = options.Value.ServerHost;
        var serverPort = options.Value.ServerPort;
        var username = options.Value.Username;
        var password = options.Value.Password;

        Log.Information("Sending email to {recipientEmail}.", recipientEmail);

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(MailboxAddress.Parse(recipientEmail));
        message.Subject = subject;
        message.Body = new TextPart(TextFormat.Html) { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(serverHost, serverPort, false, cancellationToken);

        if (!string.IsNullOrEmpty(username))
        {
            await client.AuthenticateAsync(username, password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
