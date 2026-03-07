using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using MediatR;

namespace ExpenseManager.Application.Features.Budgets.Commands;

public sealed record DeleteBudgetCommand(Guid Id) : IRequest;

public sealed class DeleteBudgetHandler(IBudgetRepository budgets, IUnitOfWork uow, ICurrentUserService currentUser)
    : IRequestHandler<DeleteBudgetCommand>
{
    public async Task Handle(DeleteBudgetCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var budget = await budgets.GetByIdAsync(userId, request.Id, cancellationToken);
        if (budget is null) throw new NotFoundException("Budget not found.");

        budgets.Remove(budget);
        await uow.SaveChangesAsync(cancellationToken);
    }
}
