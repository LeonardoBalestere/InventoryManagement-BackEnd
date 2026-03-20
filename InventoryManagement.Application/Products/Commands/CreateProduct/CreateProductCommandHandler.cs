using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Domain.Entities;
using MediatR;
using Ganss.Xss;

namespace InventoryManagement.Application.Products.Commands.CreateProduct;

internal sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var existingProduct = await _productRepository.GetBySkuAsync(request.Sku, cancellationToken);
        if (existingProduct != null)
        {
            throw new InventoryManagement.Application.Common.Exceptions.ConflictException("A product with this SKU already exists.");
        }

        var sanitizer = new HtmlSanitizer();
        var sanitizedName = sanitizer.Sanitize(request.Name);
        var sanitizedDescription = string.IsNullOrWhiteSpace(request.Description) ? request.Description : sanitizer.Sanitize(request.Description);

        var product = new Product(
            request.CategoryId,
            request.Sku,
            sanitizedName,
            sanitizedDescription,
            request.BasePrice,
            request.MinStockLevel);

        await _productRepository.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}