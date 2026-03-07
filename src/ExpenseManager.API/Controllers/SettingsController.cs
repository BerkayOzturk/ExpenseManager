using ExpenseManager.Application.Features.Settings;
using ExpenseManager.Application.Features.Settings.Commands;
using ExpenseManager.Application.Features.Settings.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManager.API.Controllers;

[ApiController]
[Route("api/settings")]
[Authorize]
public sealed class SettingsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<UserSettingsDto>> Get(CancellationToken cancellationToken)
        => Ok(await mediator.Send(new GetUserSettingsQuery(), cancellationToken));

    public sealed record UpdateSettingsRequest(string DefaultCurrency, string DateFormat, string Theme, string? Language);

    [HttpPut]
    public async Task<ActionResult<UserSettingsDto>> Update(UpdateSettingsRequest request, CancellationToken cancellationToken)
        => Ok(await mediator.Send(
            new UpdateUserSettingsCommand(request.DefaultCurrency, request.DateFormat, request.Theme, request.Language),
            cancellationToken));
}
