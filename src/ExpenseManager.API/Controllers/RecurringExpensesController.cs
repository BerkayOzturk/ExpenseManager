using ExpenseManager.Application.Features.RecurringExpenses;
using ExpenseManager.Application.Features.RecurringExpenses.Commands;
using ExpenseManager.Application.Features.RecurringExpenses.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManager.API.Controllers;

[ApiController]
[Route("api/recurring-expenses")]
[Authorize]
public sealed class RecurringExpensesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RecurringExpenseDto>>> List(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new ListRecurringExpensesQuery(), cancellationToken));

    [HttpGet("upcoming")]
    public async Task<ActionResult<IReadOnlyList<UpcomingExpenseDto>>> Upcoming([FromQuery] int months = 3, CancellationToken cancellationToken = default)
        => Ok(await mediator.Send(new GetUpcomingRecurringQuery(months), cancellationToken));

    [HttpPost]
    public async Task<ActionResult<RecurringExpenseDto>> Create(CreateRecurringExpenseRequest request, CancellationToken cancellationToken)
    {
        var created = await mediator.Send(
            new CreateRecurringExpenseCommand(
                request.Amount,
                request.Currency,
                request.FirstOccurrenceOn,
                request.Description,
                request.CategoryId,
                request.EndOn),
            cancellationToken);
        return CreatedAtAction(nameof(List), null, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<RecurringExpenseDto>> Update(Guid id, UpdateRecurringExpenseRequest request, CancellationToken cancellationToken)
        => Ok(await mediator.Send(
            new UpdateRecurringExpenseCommand(id, request.Amount, request.Currency, request.FirstOccurrenceOn, request.Description, request.CategoryId, request.EndOn),
            cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteRecurringExpenseCommand(id), cancellationToken);
        return NoContent();
    }
}

public sealed record CreateRecurringExpenseRequest(
    decimal Amount,
    string Currency,
    DateOnly FirstOccurrenceOn,
    string? Description,
    Guid? CategoryId,
    DateOnly? EndOn);

public sealed record UpdateRecurringExpenseRequest(
    decimal Amount,
    string Currency,
    DateOnly FirstOccurrenceOn,
    string? Description,
    Guid? CategoryId,
    DateOnly? EndOn);
