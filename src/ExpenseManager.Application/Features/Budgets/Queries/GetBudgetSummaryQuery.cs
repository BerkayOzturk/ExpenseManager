using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using MediatR;

namespace ExpenseManager.Application.Features.Budgets.Queries;

public sealed record GetBudgetSummaryQuery : IRequest<IReadOnlyList<BudgetSummaryDto>>;

public sealed class GetBudgetSummaryHandler(IBudgetRepository budgets, IExpenseRepository expenses, ICurrentUserService currentUser)
    : IRequestHandler<GetBudgetSummaryQuery, IReadOnlyList<BudgetSummaryDto>>
{
    public async Task<IReadOnlyList<BudgetSummaryDto>> Handle(GetBudgetSummaryQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var list = await budgets.ListAsync(userId, cancellationToken);
        var result = new List<BudgetSummaryDto>();

        foreach (var b in list)
        {
            var spent = await expenses.GetTotalSpentAsync(userId, b.Currency, b.Year, b.Month, b.CategoryId, cancellationToken);
            result.Add(new BudgetSummaryDto(
                b.Id,
                b.CategoryId,
                b.Category?.Name,
                b.Amount,
                b.Currency,
                b.Year,
                b.Month,
                spent,
                spent > b.Amount));
        }

        return result.OrderBy(x => x.Year).ThenBy(x => x.Month ?? 0).ThenBy(x => x.CategoryName ?? "").ToList();
    }
}
