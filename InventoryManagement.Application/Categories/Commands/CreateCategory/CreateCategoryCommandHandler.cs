using Ganss.Xss;
using InventoryManagement.Application.Common.Interfaces;
using InventoryManagement.Domain.Entities;
using MediatR;

namespace InventoryManagement.Application.Categories.Commands.CreateCategory;

internal sealed class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(ICategoryRepository categoryRepository, IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var existingCategory = await _categoryRepository.GetByNameAsync(request.Name, cancellationToken);
        if (existingCategory != null)
        {
            throw new InventoryManagement.Application.Common.Exceptions.ConflictException("A category with this name already exists.");
        }

        var sanitizer = new HtmlSanitizer();
        var sanitizedName = sanitizer.Sanitize(request.Name);
        var sanitizedDescription = string.IsNullOrWhiteSpace(request.Description) ? request.Description : sanitizer.Sanitize(request.Description);

        var category = new Category(
            sanitizedName,
            sanitizedDescription);

        await _categoryRepository.AddAsync(category, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
