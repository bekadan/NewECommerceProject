using Core.Exceptions.Types;
using Core.Integration.Abstractions;
using Core.Logging.Abstractions;
using System.Net;
using System.Net.Mail;

namespace Core.Integration.Services;

public class SmtpEmailService : IEmailService
{
    private readonly ILogger _logger;
    private readonly SmtpClient _smtpClient;
    private readonly string _fromAddress;

    public SmtpEmailService(ILogger logger, string host, int port, string username, string password, string fromAddress)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _fromAddress = fromAddress ?? throw new ArgumentNullException(nameof(fromAddress));

        _smtpClient = new SmtpClient(host, port)
        {
            Credentials = new NetworkCredential(username, password),
            EnableSsl = true // assume SSL required; can make configurable
        };
    }

    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = true, CancellationToken cancellationToken = default)
    {
        try
        {
            using var message = new MailMessage(_fromAddress, to, subject, body)
            {
                IsBodyHtml = isHtml
            };

            _logger.Information("Sending email to {To} with subject {Subject}", to, subject);

            // SmtpClient.SendMailAsync supports cancellation token from .NET 6+
            await _smtpClient.SendMailAsync(message, cancellationToken);

            _logger.Information("Email sent successfully to {To}", to);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to send email to {To}", to);
            throw new EmailException($"Failed to send email to {to}", ex);
        }
    }
}
