using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Features.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseManager.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IMediator mediator, IAuthService authService, IConfiguration configuration) : ControllerBase
{
    [HttpGet("config")]
    [AllowAnonymous]
    public ActionResult<AuthConfigResponse> GetConfig()
    {
        var googleClientId = configuration["Google:ClientId"] ?? "";
        return Ok(new AuthConfigResponse(googleClientId));
    }

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

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        await authService.RequestPasswordResetAsync(request.Email, cancellationToken);
        return Ok(new { message = "If an account exists for this email, a reset code has been sent." });
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<ActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var success = await authService.ResetPasswordWithCodeAsync(request.Email, request.Code, request.NewPassword, cancellationToken);
        if (!success)
            return BadRequest(new { detail = "Invalid or expired code. Please request a new one." });
        return Ok(new { message = "Password has been reset. You can now sign in." });
    }
}

public sealed record RegisterRequest(string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record GoogleLoginRequest(string IdToken);
public sealed record ForgotPasswordRequest(string Email);
public sealed record ResetPasswordRequest(string Email, string Code, string NewPassword);
public sealed record AuthResponse(string UserId, string Email, string Token);
public sealed record AuthConfigResponse(string GoogleClientId);
