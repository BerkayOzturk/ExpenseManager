using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using MediatR;

namespace ExpenseManager.Application.Features.Expenses.Commands;

public sealed record DeleteExpenseCommand(Guid Id) : IRequest;

public sealed class DeleteExpenseHandler(IExpenseRepository expenses, IUnitOfWork uow, ICurrentUserService currentUser)
    : IRequestHandler<DeleteExpenseCommand>
{
    public async Task Handle(DeleteExpenseCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var expense = await expenses.GetByIdAsync(userId, request.Id, cancellationToken);
        if (expense is null) throw new NotFoundException("Expense not found.");

        expenses.Remove(expense);
        await uow.SaveChangesAsync(cancellationToken);
    }
}

