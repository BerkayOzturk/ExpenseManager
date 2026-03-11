using ExpenseManager.Application.Abstractions;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace ExpenseManager.Infrastructure.Email;

public sealed class SmtpEmailSender(IConfiguration configuration, ILogger<SmtpEmailSender> logger) : IEmailSender
{
    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        var host = configuration["Smtp:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            logger.LogWarning("Smtp:Host not configured. Password reset code (if any) would be sent to {To}. Body: {Body}", to, body);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            configuration["Smtp:FromName"] ?? "Coin Canvas",
            configuration["Smtp:FromEmail"] ?? configuration["Smtp:User"] ?? "noreply@localhost"));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("plain") { Text = body };

        using var client = new SmtpClient();
        var portStr = configuration["Smtp:Port"];
        var port = int.TryParse(portStr, out var p) ? p : 587;
        var useSslStr = configuration["Smtp:UseSsl"];
        var useSsl = string.IsNullOrEmpty(useSslStr) || bool.TrueString.Equals(useSslStr, StringComparison.OrdinalIgnoreCase);
        await client.ConnectAsync(host, port, useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, cancellationToken);

        var user = configuration["Smtp:User"]?.Trim();
        var password = configuration["Smtp:Password"]?.Trim();
        if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
            await client.AuthenticateAsync(user, password, cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
        logger.LogInformation("Password reset email sent to {To}", to);
    }
}
