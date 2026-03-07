using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using ExpenseManager.Domain.Entities;
using FluentValidation;
using MediatR;

namespace ExpenseManager.Application.Features.Settings.Commands;

public sealed record UpdateUserSettingsCommand(
    string DefaultCurrency,
    string DateFormat,
    string Theme,
    string? Language) : IRequest<UserSettingsDto>;

public sealed class UpdateUserSettingsValidator : AbstractValidator<UpdateUserSettingsCommand>
{
    public UpdateUserSettingsValidator()
    {
        RuleFor(x => x.DefaultCurrency).NotEmpty().MaximumLength(5);
        RuleFor(x => x.DateFormat).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Theme).Must(t => string.Equals(t, "light", StringComparison.OrdinalIgnoreCase) || string.Equals(t, "dark", StringComparison.OrdinalIgnoreCase) || string.Equals(t, "system", StringComparison.OrdinalIgnoreCase)).WithMessage("Theme must be light, dark, or system.");
    }
}

public sealed class UpdateUserSettingsHandler(IUserSettingsRepository settingsRepo, IUnitOfWork uow, ICurrentUserService currentUser)
    : IRequestHandler<UpdateUserSettingsCommand, UserSettingsDto>
{
    public async Task<UserSettingsDto> Handle(UpdateUserSettingsCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var settings = await settingsRepo.GetByUserIdAsync(userId, cancellationToken);
        if (settings is null)
        {
            settings = new UserSettings(userId, request.DefaultCurrency, request.DateFormat, request.Theme, request.Language);
            await settingsRepo.AddAsync(settings, cancellationToken);
        }
        else
        {
            settings.SetDefaults(request.DefaultCurrency, request.DateFormat, request.Theme, request.Language);
        }

        await uow.SaveChangesAsync(cancellationToken);

        return new UserSettingsDto(
            settings.DefaultCurrency,
            settings.DateFormat,
            settings.Theme,
            settings.Language);
    }
}
