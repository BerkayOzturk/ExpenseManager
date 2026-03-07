using ExpenseManager.Application.Abstractions;
using FluentValidation;
using MediatR;

namespace ExpenseManager.Application.Features.Auth.Commands;

public sealed record RegisterCommand(string Email, string Password) : IRequest<AuthResult>;

public sealed class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6).MaximumLength(100);
    }
}

public sealed class RegisterHandler(IAuthService authService) : IRequestHandler<RegisterCommand, AuthResult>
{
    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var result = await authService.RegisterAsync(request.Email, request.Password, cancellationToken);
        return result;
    }
}
