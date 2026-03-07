using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using MediatR;

namespace ExpenseManager.Application.Features.Budgets.Queries;

public sealed record GetBudgetByIdQuery(Guid Id) : IRequest<BudgetDto>;

public sealed class GetBudgetByIdHandler(IBudgetRepository budgets, ICurrentUserService currentUser)
    : IRequestHandler<GetBudgetByIdQuery, BudgetDto>
{
    public async Task<BudgetDto> Handle(GetBudgetByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var budget = await budgets.GetByIdAsync(userId, request.Id, cancellationToken);
        if (budget is null) throw new NotFoundException("Budget not found.");

        return new BudgetDto(
            budget.Id,
            budget.CategoryId,
            budget.Category?.Name,
            budget.Amount,
            budget.Currency,
            budget.Year,
            budget.Month);
    }
}
