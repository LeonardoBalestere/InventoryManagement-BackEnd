namespace InventoryManagement.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendAsync(string subject, string htmlBody, CancellationToken cancellationToken);
}
