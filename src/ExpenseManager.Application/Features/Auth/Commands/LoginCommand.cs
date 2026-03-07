using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Common.Exceptions;
using FluentValidation;
using MediatR;

namespace ExpenseManager.Application.Features.Auth.Commands;

public sealed record LoginCommand(string Email, string Password) : IRequest<AuthResult>;

public sealed class LoginValidator : AbstractValidator<LoginCommand>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginHandler(IAuthService authService) : IRequestHandler<LoginCommand, AuthResult>
{
    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request.Email, request.Password, cancellationToken);
        if (result is null)
            throw new UnauthorizedException("Invalid email or password.");
        return result;
    }
}
