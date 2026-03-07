using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using ExpenseManager.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ExpenseManager.Application.Features.RecurringExpenses.Commands;

public sealed record CreateRecurringExpenseCommand(
    decimal Amount,
    string Currency,
    DateOnly FirstOccurrenceOn,
    string? Description,
    Guid? CategoryId,
    DateOnly? EndOn) : IRequest<RecurringExpenseDto>;

public sealed class CreateRecurringExpenseValidator : AbstractValidator<CreateRecurringExpenseCommand>
{
    public CreateRecurringExpenseValidator()
    {
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().MinimumLength(3).MaximumLength(5);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class CreateRecurringExpenseHandler(IRecurringExpenseRepository repo, ICategoryRepository categories, IUnitOfWork uow, ICurrentUserService currentUser)
    : IRequestHandler<CreateRecurringExpenseCommand, RecurringExpenseDto>
{
    public async Task<RecurringExpenseDto> Handle(CreateRecurringExpenseCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        if (request.CategoryId is Guid categoryId)
        {
            var cat = await categories.GetByIdAsync(userId, categoryId, cancellationToken);
            if (cat is null) throw new NotFoundException("Category not found.");
        }

        var recurring = new RecurringExpense(userId, request.Amount, request.Currency, request.FirstOccurrenceOn, request.Description, request.CategoryId, request.EndOn);
        await repo.AddAsync(recurring, cancellationToken);
        await uow.SaveChangesAsync(cancellationToken);

        return new RecurringExpenseDto(
            recurring.Id,
            recurring.Amount,
            recurring.Currency,
            recurring.FirstOccurrenceOn,
            recurring.Description,
            recurring.CategoryId,
            recurring.Category?.Name,
            recurring.EndOn);
    }
}
