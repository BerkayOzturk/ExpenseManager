using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using MediatR;

namespace ExpenseManager.Application.Features.RecurringExpenses.Queries;

public sealed record ListRecurringExpensesQuery : IRequest<IReadOnlyList<RecurringExpenseDto>>;

public sealed class ListRecurringExpensesHandler(IRecurringExpenseRepository repo, ICurrentUserService currentUser)
    : IRequestHandler<ListRecurringExpensesQuery, IReadOnlyList<RecurringExpenseDto>>
{
    public async Task<IReadOnlyList<RecurringExpenseDto>> Handle(ListRecurringExpensesQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var list = await repo.ListAsync(userId, cancellationToken);
        return list
            .OrderBy(r => r.FirstOccurrenceOn)
            .Select(r => new RecurringExpenseDto(r.Id, r.Amount, r.Currency, r.FirstOccurrenceOn, r.Description, r.CategoryId, r.Category?.Name, r.EndOn))
            .ToList();
    }
}
