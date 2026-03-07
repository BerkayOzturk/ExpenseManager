using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using MediatR;

namespace ExpenseManager.Application.Features.Categories.Queries;

public sealed record ListCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;

public sealed class ListCategoriesHandler(ICategoryRepository categories, ICurrentUserService currentUser)
    : IRequestHandler<ListCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public async Task<IReadOnlyList<CategoryDto>> Handle(ListCategoriesQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var list = await categories.ListAsync(userId, cancellationToken);
        return list
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(c.Id, c.Name))
            .ToList();
    }
}

