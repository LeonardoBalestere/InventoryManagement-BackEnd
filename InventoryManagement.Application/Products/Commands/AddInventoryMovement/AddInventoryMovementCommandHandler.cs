using System;
using System.Threading;
using System.Threading.Tasks;
using InventoryManagement.Application.Common.Exceptions;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Domain.Enums;
using MediatR;

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
        // Idempotency check should IDEALLY happen before touching Domain logic,
        // typically using an Idempotency Service or Decorator. For now, we trust the pipeline or API layer handles the idempotency tracking.

        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken);

        if (product == null)
            throw new NotFoundException(nameof(Domain.Entities.Product), request.ProductId);

        var movementType = Enum.Parse<MovementType>(request.MovementType, ignoreCase: true);

        product.AddMovement(request.Quantity, movementType, request.Justification);

        await _productRepository.UpdateAsync(product, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // We could technically return the MovementId, but returning the ProductId is fine for REST
        return product.Id;
    }
}