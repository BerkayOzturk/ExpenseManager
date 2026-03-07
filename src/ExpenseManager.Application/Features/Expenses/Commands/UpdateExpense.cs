using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using ExpenseManager.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace ExpenseManager.Application.Features.Expenses.Commands;

public sealed record UpdateExpenseCommand(
    Guid Id,
    decimal Amount,
    string Currency,
    DateOnly OccurredOn,
    string? Description,
    Guid? CategoryId) : IRequest<ExpenseDto>;

public sealed class UpdateExpenseValidator : AbstractValidator<UpdateExpenseCommand>
{
    public UpdateExpenseValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().MinimumLength(3).MaximumLength(5);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class UpdateExpenseHandler(IExpenseRepository expenses, ICategoryRepository categories, IUnitOfWork uow, ICurrentUserService currentUser)
    : IRequestHandler<UpdateExpenseCommand, ExpenseDto>
{
    public async Task<ExpenseDto> Handle(UpdateExpenseCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var expense = await expenses.GetByIdAsync(userId, request.Id, cancellationToken);
        if (expense is null) throw new NotFoundException("Expense not found.");

        if (request.CategoryId is Guid categoryId)
        {
            var exists = await categories.GetByIdAsync(userId, categoryId, cancellationToken);
            if (exists is null)
                throw new ValidationException("CategoryId is invalid.");
        }

        expense.Update(
            money: Money.Create(request.Amount, request.Currency),
            occurredOn: request.OccurredOn,
            description: request.Description,
            categoryId: request.CategoryId);

        await uow.SaveChangesAsync(cancellationToken);

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

