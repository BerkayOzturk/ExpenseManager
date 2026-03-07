using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using MediatR;

namespace ExpenseManager.Application.Features.Expenses.Queries;

public sealed record ListExpensesQuery(DateOnly? From, DateOnly? To, Guid? CategoryId) : IRequest<IReadOnlyList<ExpenseDto>>;

public sealed class ListExpensesHandler(IExpenseRepository expenses, ICurrentUserService currentUser)
    : IRequestHandler<ListExpensesQuery, IReadOnlyList<ExpenseDto>>
{
    public async Task<IReadOnlyList<ExpenseDto>> Handle(ListExpensesQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var list = await expenses.ListAsync(userId, request.From, request.To, request.CategoryId, cancellationToken);

        return list
            .OrderByDescending(e => e.OccurredOn)
            .ThenByDescending(e => e.CreatedAtUtc)
            .Select(e => new ExpenseDto(
                e.Id,
                e.Amount,
                e.Currency,
                e.OccurredOn,
                e.Description,
                e.CategoryId,
                e.Category?.Name))
            .ToList();
    }
}

