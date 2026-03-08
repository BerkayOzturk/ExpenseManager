using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using ExpenseManager.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ExpenseManager.Application.Features.Categories.Commands;

public sealed record CreateCategoryCommand(string Name, int SortOrder = 0) : IRequest<CategoryDto>;

public sealed class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}

public sealed class CreateCategoryHandler(ICategoryRepository categories, IUnitOfWork uow, ICurrentUserService currentUser)
    : IRequestHandler<CreateCategoryCommand, CategoryDto>
{
    public async Task<CategoryDto> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var name = request.Name.Trim();

        if (await categories.ExistsWithNameAsync(userId, name, excludingId: null, cancellationToken))
            throw new ValidationException($"Category with name '{name}' already exists.");

        var category = new Category(name, userId, request.SortOrder);
        await categories.AddAsync(category, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return new CategoryDto(category.Id, category.Name, category.SortOrder);
    }
}

