using Core.BackgroundJobs.Abstractions;
using Core.Events.Events;
using Core.Integration.Abstractions;
using Core.Logging.Abstractions;

namespace Core.BackgroundJobs.Handlers;

public class SendEmailJobHandler : IBackgroundJobHandler<SendEmailEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public SendEmailJobHandler(IEmailService emailService, ILogger logger)
    {
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(SendEmailEvent @event, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.Information("Sending email to {To}", @event.To);

            await _emailService.SendEmailAsync(
                to: @event.To,
                subject: @event.Subject,
                body: @event.Body,
                cancellationToken: cancellationToken
            );

            _logger.Information("Email sent successfully to {To}", @event.To);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to send email to {To}", @event.To);
            throw; // allows Polly to retry
        }
    }
}
