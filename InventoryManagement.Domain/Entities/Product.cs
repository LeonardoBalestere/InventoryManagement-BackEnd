using System;
using System.Collections.Generic;
using System.Linq;
using InventoryManagement.Domain.Enums;
using InventoryManagement.Domain.Exceptions;

namespace InventoryManagement.Domain.Entities;

public sealed class Product
{
    public Guid Id { get; private set; }
    public string Sku { get; private set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal BasePrice { get; private set; }
    public int MinStockLevel { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<InventoryMovement> _movements = new();
    public IReadOnlyCollection<InventoryMovement> Movements => _movements.AsReadOnly();

    // Parameterless constructor for ORM
    private Product() { }

    public Product(string sku, string name, string description, decimal basePrice, int minStockLevel)
    {
        if (string.IsNullOrWhiteSpace(sku) || sku.Length > 20)
            throw new DomainException("SKU is required and cannot exceed 20 characters.");

        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
            throw new DomainException("Name is required and cannot exceed 100 characters.");

        if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
            throw new DomainException("Description cannot exceed 500 characters.");

        if (basePrice < 0)
            throw new DomainException("Base price cannot be negative.");

        if (minStockLevel < 0)
            throw new DomainException("Minimum stock level cannot be negative.");

        Id = Guid.NewGuid();
        Sku = sku.Trim();
        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        BasePrice = basePrice;
        MinStockLevel = minStockLevel;
        IsActive = true; 
    }

    public int GetCurrentStock()
    {
        return _movements.Sum(m => m.Type switch
        {
            MovementType.Inbound => m.Quantity,
            MovementType.Outbound => -m.Quantity,
            MovementType.Adjustment => m.Quantity, 
            _ => 0
        });
    }

    public void AddMovement(int quantity, MovementType type, string? justification = null)
    {
        if (!IsActive)
            throw new DomainException("Cannot process inventory movements for an inactive product.");

        if (type == MovementType.Outbound)
        {
            if (GetCurrentStock() - quantity < 0)
                throw new DomainException("Outbound movement cannot result in a negative stock balance.");
        }

        var movement = new InventoryMovement(Id, type, quantity, justification);
        _movements.Add(movement);
    }

    public void ChangePrice(decimal newBasePrice)
    {
        if (newBasePrice < 0)
            throw new DomainException("Base price cannot be negative.");

        BasePrice = newBasePrice;
    }

    public void UpdateInfo(string name, string description, int minStockLevel)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 100)
            throw new DomainException("Name is required and cannot exceed 100 characters.");

        if (!string.IsNullOrWhiteSpace(description) && description.Length > 500)
            throw new DomainException("Description cannot exceed 500 characters.");

        if (minStockLevel < 0)
            throw new DomainException("Minimum stock level cannot be negative.");

        Name = name.Trim();
        Description = description?.Trim() ?? string.Empty;
        MinStockLevel = minStockLevel;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }
}