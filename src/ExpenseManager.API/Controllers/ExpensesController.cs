using ExpenseManager.Application.Features.Expenses;
using ExpenseManager.Application.Features.Expenses.Commands;
using ExpenseManager.Application.Features.Expenses.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManager.API.Controllers;

[ApiController]
[Route("api/expenses")]
[Authorize]
public sealed class ExpensesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ListExpensesResponse>> List(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        [FromQuery] Guid? categoryId,
        [FromQuery] string? search,
        CancellationToken cancellationToken)
        => Ok(await mediator.Send(new ListExpensesQuery(from, to, categoryId, search), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpenseDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetExpenseByIdQuery(id), cancellationToken));

    public sealed record CreateExpenseRequest(
        decimal Amount,
        string Currency,
        DateOnly OccurredOn,
        string? Description,
        Guid? CategoryId);

    [HttpPost]
    public async Task<ActionResult<ExpenseDto>> Create(CreateExpenseRequest request, CancellationToken cancellationToken)
    {
        var created = await mediator.Send(
            new CreateExpenseCommand(
                request.Amount,
                request.Currency,
                request.OccurredOn,
                request.Description,
                request.CategoryId),
            cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    public sealed record UpdateExpenseRequest(
        decimal Amount,
        string Currency,
        DateOnly OccurredOn,
        string? Description,
        Guid? CategoryId);

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExpenseDto>> Update(Guid id, UpdateExpenseRequest request, CancellationToken cancellationToken)
        => Ok(await mediator.Send(
            new UpdateExpenseCommand(
                id,
                request.Amount,
                request.Currency,
                request.OccurredOn,
                request.Description,
                request.CategoryId),
            cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteExpenseCommand(id), cancellationToken);
        return NoContent();
    }
}

