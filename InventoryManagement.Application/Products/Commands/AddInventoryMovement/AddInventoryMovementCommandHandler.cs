using InventoryManagement.Application.Common.Exceptions;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Domain.Enums;
using MediatR;
using Ganss.Xss;

namespace InventoryManagement.Application.Products.Commands.AddInventoryMovement;

internal sealed class AddInventoryMovementCommandHandler : IRequestHandler<AddInventoryMovementCommand, Guid>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AddInventoryMovementCommandHandler(IProductRepository productRepository, IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(AddInventoryMovementCommand request, CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product == null)
            throw new NotFoundException(nameof(Domain.Entities.Product), request.ProductId);

        var movementType = Enum.Parse<MovementType>(request.MovementType, ignoreCase: true);

        var sanitizer = new HtmlSanitizer();
        var sanitizedJustification = string.IsNullOrWhiteSpace(request.Justification) ? request.Justification : sanitizer.Sanitize(request.Justification);

        product.AddMovement(request.Quantity, movementType, sanitizedJustification);

        await _productRepository.UpdateAsync(product, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}