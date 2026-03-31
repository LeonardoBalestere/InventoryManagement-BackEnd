using InventoryManagement.Domain.Exceptions;
using InventoryManagement.Domain.Errors;

namespace InventoryManagement.Domain.Entities;

public sealed class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }

    // Parameterless constructor for ORM
    private Category() { }

    public Category(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(DomainErrors.Category.NameRequired);

        Id = Guid.NewGuid();
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
    }

    public void Update(string name, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(DomainErrors.Category.NameRequired);

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
    }
}