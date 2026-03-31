namespace InventoryManagement.Domain.Errors;

public static class DomainErrors
{
    public static class Product
    {
        public const string CategoryIdRequired = "CategoryId is required.";
        public const string SkuRequired = "SKU is required and cannot exceed 20 characters.";
        public const string NameRequired = "Name is required and cannot exceed 100 characters.";
        public const string DescriptionMax = "Description cannot exceed 500 characters.";
        public const string BasePriceNegative = "Base price cannot be negative.";
        public const string MinStockLevelNegative = "Minimum stock level cannot be negative.";
        public const string CannotModifyInactive = "Cannot process inventory movements for an inactive product.";
        public const string NegativeStockBalance = "Outbound movement cannot result in a negative stock balance.";
    }

    public static class Category
    {
        public const string NameRequired = "Category Name is required.";
    }

    public static class InventoryMovement
    {
        public const string ProductIdEmpty = "ProductId cannot be empty.";
        public const string QuantityGreaterThanZeroFormat = "{0} quantity must be greater than zero.";
        public const string InvalidAdjustment = "An adjustment movement requires a valid justification.";
        public const string InvalidAdjustmentZero = "Adjustment quantity cannot be zero.";
        public const string JustificationMax = "Justification cannot exceed 255 characters.";
    }
}