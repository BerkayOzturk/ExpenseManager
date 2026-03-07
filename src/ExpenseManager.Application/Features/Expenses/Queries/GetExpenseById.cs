using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using MediatR;

namespace ExpenseManager.Application.Features.Expenses.Queries;

public sealed record GetExpenseByIdQuery(Guid Id) : IRequest<ExpenseDto>;

public sealed class GetExpenseByIdHandler(IExpenseRepository expenses, ICurrentUserService currentUser)
    : IRequestHandler<GetExpenseByIdQuery, ExpenseDto>
{
    public async Task<ExpenseDto> Handle(GetExpenseByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var expense = await expenses.GetByIdAsync(userId, request.Id, cancellationToken);
        if (expense is null) throw new NotFoundException("Expense not found.");

        return new ExpenseDto(
            expense.Id,
            expense.Amount,
            expense.Currency,
            expense.OccurredOn,
            expense.Description,
            expense.CategoryId,
            expense.Category?.Name);
    }
}

