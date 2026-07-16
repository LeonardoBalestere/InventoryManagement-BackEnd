namespace InventoryManagement.Infrastructure.Email;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string SendGridApiKey { get; init; } = string.Empty;
    public string From { get; init; } = string.Empty;
    public string FromName { get; init; } = "Inventory Management";
    public string To { get; init; } = string.Empty;
}
