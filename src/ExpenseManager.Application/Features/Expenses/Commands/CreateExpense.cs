using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using ExpenseManager.Domain.Entities;
using ExpenseManager.Domain.ValueObjects;
using FluentValidation;
using MediatR;

namespace ExpenseManager.Application.Features.Expenses.Commands;

public sealed record CreateExpenseCommand(
    decimal Amount,
    string Currency,
    DateOnly OccurredOn,
    string? Description,
    Guid? CategoryId) : IRequest<ExpenseDto>;

public sealed class CreateExpenseValidator : AbstractValidator<CreateExpenseCommand>
{
    public CreateExpenseValidator()
    {
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().MinimumLength(3).MaximumLength(5);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class CreateExpenseHandler(IExpenseRepository expenses, ICategoryRepository categories, IUnitOfWork uow, ICurrentUserService currentUser)
    : IRequestHandler<CreateExpenseCommand, ExpenseDto>
{
    public async Task<ExpenseDto> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        if (request.CategoryId is Guid categoryId)
        {
            var exists = await categories.GetByIdAsync(userId, categoryId, cancellationToken);
            if (exists is null)
                throw new ValidationException("CategoryId is invalid.");
        }

        var expense = new Expense(
            userId,
            money: Money.Create(request.Amount, request.Currency),
            occurredOn: request.OccurredOn,
            description: request.Description,
            categoryId: request.CategoryId);

        await expenses.AddAsync(expense, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return new ExpenseDto(
            expense.Id,
            expense.Amount,
            expense.Currency,
            expense.OccurredOn,
            expense.Description,
            expense.CategoryId,
            CategoryName: null);
    }
}

