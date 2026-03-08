using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using FluentValidation;
using MediatR;

namespace ExpenseManager.Application.Features.Categories.Commands;

public sealed record UpdateCategoryCommand(Guid Id, string Name, int? SortOrder) : IRequest<CategoryDto>;

public sealed class UpdateCategoryValidator : AbstractValidator<UpdateCategoryCommand>
{
    public UpdateCategoryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public sealed class UpdateCategoryHandler(ICategoryRepository categories, IUnitOfWork uow, ICurrentUserService currentUser)
    : IRequestHandler<UpdateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var category = await categories.GetByIdAsync(userId, request.Id, cancellationToken);
        if (category is null) throw new NotFoundException("Category not found.");

        var name = request.Name.Trim();
        if (await categories.ExistsWithNameAsync(userId, name, excludingId: request.Id, cancellationToken))
            throw new ValidationException($"Category with name '{name}' already exists.");

        category.Rename(name);
        if (request.SortOrder.HasValue)
            category.SetSortOrder(request.SortOrder.Value);
        await uow.SaveChangesAsync(cancellationToken);

        return new CategoryDto(category.Id, category.Name, category.SortOrder);
    }
}

