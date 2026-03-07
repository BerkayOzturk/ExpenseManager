using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using FluentValidation;
using MediatR;

namespace ExpenseManager.Application.Features.RecurringExpenses.Commands;

public sealed record UpdateRecurringExpenseCommand(
    Guid Id,
    decimal Amount,
    string Currency,
    DateOnly FirstOccurrenceOn,
    string? Description,
    Guid? CategoryId,
    DateOnly? EndOn) : IRequest<RecurringExpenseDto>;

public sealed class UpdateRecurringExpenseValidator : AbstractValidator<UpdateRecurringExpenseCommand>
{
    public UpdateRecurringExpenseValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Amount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Currency).NotEmpty().MinimumLength(3).MaximumLength(5);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}

public sealed class UpdateRecurringExpenseHandler(IRecurringExpenseRepository repo, IUnitOfWork uow, ICurrentUserService currentUser)
    : IRequestHandler<UpdateRecurringExpenseCommand, RecurringExpenseDto>
{
    public async Task<RecurringExpenseDto> Handle(UpdateRecurringExpenseCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var recurring = await repo.GetByIdAsync(userId, request.Id, cancellationToken);
        if (recurring is null) throw new NotFoundException("Recurring expense not found.");

        recurring.Update(request.Amount, request.Currency, request.FirstOccurrenceOn, request.Description, request.CategoryId, request.EndOn);
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
