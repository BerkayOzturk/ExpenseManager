using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using MediatR;

namespace ExpenseManager.Application.Features.Expenses.Queries;

public sealed record ListExpensesQuery(DateOnly? From, DateOnly? To, Guid? CategoryId, string? Search) : IRequest<ListExpensesResponse>;

public sealed class ListExpensesHandler(IExpenseRepository expenses, ICurrentUserService currentUser)
    : IRequestHandler<ListExpensesQuery, ListExpensesResponse>
{
    public async Task<ListExpensesResponse> Handle(ListExpensesQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var (items, totalCount) = await expenses.ListWithCountAsync(
            userId, request.From, request.To, request.CategoryId, request.Search, cancellationToken);

        var dtos = items
            .Select(e => new ExpenseDto(
                e.Id,
                e.Amount,
                e.Currency,
                e.OccurredOn,
                e.Description,
                e.CategoryId,
                e.Category?.Name))
            .ToList();

        return new ListExpensesResponse(dtos, totalCount);
    }
}

