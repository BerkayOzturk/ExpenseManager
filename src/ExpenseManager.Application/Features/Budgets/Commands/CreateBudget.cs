using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using ExpenseManager.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ExpenseManager.Application.Features.Budgets.Commands;

public sealed record CreateBudgetCommand(
    Guid? CategoryId,
    decimal Amount,
    string Currency,
    int Year,
    int? Month) : IRequest<BudgetDto>;

public sealed class CreateBudgetValidator : AbstractValidator<CreateBudgetCommand>
{
    public CreateBudgetValidator()
    {
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().MinimumLength(3).MaximumLength(5);
        RuleFor(x => x.Year).InclusiveBetween(2000, 2100);
        RuleFor(x => x.Month).InclusiveBetween(1, 12).When(x => x.Month.HasValue);
    }
}

public sealed class CreateBudgetHandler(IBudgetRepository budgets, ICategoryRepository categories, IUnitOfWork uow, ICurrentUserService currentUser)
    : IRequestHandler<CreateBudgetCommand, BudgetDto>
{
    public async Task<BudgetDto> Handle(CreateBudgetCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        if (request.CategoryId is Guid categoryId)
        {
            var cat = await categories.GetByIdAsync(userId, categoryId, cancellationToken);
            if (cat is null) throw new NotFoundException("Category not found.");
        }

        var budget = new Budget(userId, request.CategoryId, request.Amount, request.Currency, request.Year, request.Month);
        await budgets.AddAsync(budget, cancellationToken);
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
