using InventoryManagement.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace InventoryManagement.Infrastructure.Email;

/// <summary>
/// Fallback used when no SendGrid API key is configured (local development),
/// so alerts remain observable in the logs instead of being silently dropped.
/// </summary>
internal sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string subject, string htmlBody, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Email dispatch skipped (no SendGrid API key). Subject: {Subject}. Body: {Body}", subject, htmlBody);
        return Task.CompletedTask;
    }
}
