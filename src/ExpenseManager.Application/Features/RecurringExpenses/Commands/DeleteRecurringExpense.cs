using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using MediatR;

namespace ExpenseManager.Application.Features.RecurringExpenses.Commands;

public sealed record DeleteRecurringExpenseCommand(Guid Id) : IRequest;

public sealed class DeleteRecurringExpenseHandler(IRecurringExpenseRepository repo, IUnitOfWork uow, ICurrentUserService currentUser)
    : IRequestHandler<DeleteRecurringExpenseCommand>
{
    public async Task Handle(DeleteRecurringExpenseCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var recurring = await repo.GetByIdAsync(userId, request.Id, cancellationToken);
        if (recurring is null) throw new NotFoundException("Recurring expense not found.");

        repo.Remove(recurring);
        await uow.SaveChangesAsync(cancellationToken);
    }
}
