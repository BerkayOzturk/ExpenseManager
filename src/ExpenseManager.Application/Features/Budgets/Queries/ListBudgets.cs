using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using MediatR;

namespace ExpenseManager.Application.Features.Budgets.Queries;

public sealed record ListBudgetsQuery : IRequest<IReadOnlyList<BudgetDto>>;

public sealed class ListBudgetsHandler(IBudgetRepository budgets, ICurrentUserService currentUser)
    : IRequestHandler<ListBudgetsQuery, IReadOnlyList<BudgetDto>>
{
    public async Task<IReadOnlyList<BudgetDto>> Handle(ListBudgetsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var list = await budgets.ListAsync(userId, cancellationToken);
        return list
            .OrderBy(b => b.Year).ThenBy(b => b.Month ?? 0).ThenBy(b => b.Category?.Name ?? "")
            .Select(b => new BudgetDto(b.Id, b.CategoryId, b.Category?.Name, b.Amount, b.Currency, b.Year, b.Month))
            .ToList();
    }
}
