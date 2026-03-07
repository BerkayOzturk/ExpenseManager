using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using FluentValidation;
using MediatR;

namespace ExpenseManager.Application.Features.Budgets.Commands;

public sealed record UpdateBudgetCommand(Guid Id, decimal Amount, string Currency) : IRequest<BudgetDto>;

public sealed class UpdateBudgetValidator : AbstractValidator<UpdateBudgetCommand>
{
    public UpdateBudgetValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().MinimumLength(3).MaximumLength(5);
    }
}

public sealed class UpdateBudgetHandler(IBudgetRepository budgets, IUnitOfWork uow, ICurrentUserService currentUser)
    : IRequestHandler<UpdateBudgetCommand, BudgetDto>
{
    public async Task<BudgetDto> Handle(UpdateBudgetCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var budget = await budgets.GetByIdAsync(userId, request.Id, cancellationToken);
        if (budget is null) throw new NotFoundException("Budget not found.");

        budget.Update(request.Amount, request.Currency);
        await uow.SaveChangesAsync(cancellationToken);

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
