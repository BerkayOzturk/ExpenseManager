using ExpenseManager.Application.Features.Budgets;
using ExpenseManager.Application.Features.Budgets.Commands;
using ExpenseManager.Application.Features.Budgets.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManager.API.Controllers;

[ApiController]
[Route("api/budgets")]
[Authorize]
public sealed class BudgetsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<BudgetDto>>> List(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new ListBudgetsQuery(), cancellationToken));

    [HttpGet("summary")]
    public async Task<ActionResult<IReadOnlyList<BudgetSummaryDto>>> Summary(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetBudgetSummaryQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BudgetDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetBudgetByIdQuery(id), cancellationToken));

    public sealed record CreateBudgetRequest(Guid? CategoryId, decimal Amount, string Currency, int Year, int? Month);

    [HttpPost]
    public async Task<ActionResult<BudgetDto>> Create(CreateBudgetRequest request, CancellationToken cancellationToken)
    {
        var created = await mediator.Send(
            new CreateBudgetCommand(request.CategoryId, request.Amount, request.Currency, request.Year, request.Month),
            cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    public sealed record UpdateBudgetRequest(decimal Amount, string Currency);

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BudgetDto>> Update(Guid id, UpdateBudgetRequest request, CancellationToken cancellationToken)
        => Ok(await mediator.Send(new UpdateBudgetCommand(id, request.Amount, request.Currency), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteBudgetCommand(id), cancellationToken);
        return NoContent();
    }
}
