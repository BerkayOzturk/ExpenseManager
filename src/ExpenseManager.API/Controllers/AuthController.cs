using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Features.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManager.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IMediator mediator, IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RegisterCommand(request.Email, request.Password), cancellationToken);
        return Ok(new AuthResponse(result.UserId, result.Email, result.Token));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new LoginCommand(request.Email, request.Password), cancellationToken);
        return Ok(new AuthResponse(result.UserId, result.Email, result.Token));
    }

    [HttpPost("google")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> LoginWithGoogle(GoogleLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginWithGoogleAsync(request.IdToken, cancellationToken);
        if (result is null)
            return Unauthorized(new { detail = "Invalid or expired Google token." });
        return Ok(new AuthResponse(result.UserId, result.Email, result.Token));
    }

    }

public sealed record RegisterRequest(string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record GoogleLoginRequest(string IdToken);
public sealed record AuthResponse(string UserId, string Email, string Token);
