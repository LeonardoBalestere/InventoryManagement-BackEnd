using InventoryManagement.Application.Common.Interfaces;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Diagnostics.CodeAnalysis;

namespace InventoryManagement.Infrastructure.Email;

// Thin adapter over the SendGrid SDK; exercising it requires a live API key
[ExcludeFromCodeCoverage]
internal sealed class SendGridEmailSender : IEmailSender
{
    private readonly EmailOptions _options;

    public SendGridEmailSender(EmailOptions options)
    {
        _options = options;
    }

    public async Task SendAsync(string subject, string htmlBody, CancellationToken cancellationToken)
    {
        var client = new SendGridClient(_options.SendGridApiKey);

        var message = MailHelper.CreateSingleEmail(
            new EmailAddress(_options.From, _options.FromName),
            new EmailAddress(_options.To),
            subject,
            plainTextContent: null,
            htmlContent: htmlBody);

        var response = await client.SendEmailAsync(message, cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"SendGrid returned status code {(int)response.StatusCode}.");
    }
}
