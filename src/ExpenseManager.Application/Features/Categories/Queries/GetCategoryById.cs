using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using MediatR;

namespace ExpenseManager.Application.Features.Categories.Queries;

public sealed record GetCategoryByIdQuery(Guid Id) : IRequest<CategoryDto>;

public sealed class GetCategoryByIdHandler(ICategoryRepository categories, ICurrentUserService currentUser)
    : IRequestHandler<GetCategoryByIdQuery, CategoryDto>
{
    public async Task<CategoryDto> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var category = await categories.GetByIdAsync(userId, request.Id, cancellationToken);
        if (category is null) throw new NotFoundException("Category not found.");

        return new CategoryDto(category.Id, category.Name);
    }
}

