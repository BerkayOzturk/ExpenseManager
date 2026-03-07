using ExpenseManager.Application.Features.Categories;
using ExpenseManager.Application.Features.Categories.Commands;
using ExpenseManager.Application.Features.Categories.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManager.API.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public sealed class CategoriesController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> List(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new ListCategoriesQuery(), cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetCategoryByIdQuery(id), cancellationToken));

    public sealed record CreateCategoryRequest(string Name);

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var created = await mediator.Send(new CreateCategoryCommand(request.Name), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    public sealed record UpdateCategoryRequest(string Name);

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<CategoryDto>> Update(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
        => Ok(await mediator.Send(new UpdateCategoryCommand(id, request.Name), cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await mediator.Send(new DeleteCategoryCommand(id), cancellationToken);
        return NoContent();
    }
}

