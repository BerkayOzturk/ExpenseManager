using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using MediatR;


namespace ExpenseManager.Application.Features.Settings.Queries;

public sealed record GetUserSettingsQuery : IRequest<UserSettingsDto>;

public sealed class GetUserSettingsHandler(IUserSettingsRepository settingsRepo, ICurrentUserService currentUser)
    : IRequestHandler<GetUserSettingsQuery, UserSettingsDto>
{
    public async Task<UserSettingsDto> Handle(GetUserSettingsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var settings = await settingsRepo.GetByUserIdAsync(userId, cancellationToken);
        if (settings is null)
            return new UserSettingsDto("USD", "yyyy-MM-dd", "system", null);

        return new UserSettingsDto(
            settings.DefaultCurrency,
            settings.DateFormat,
            settings.Theme,
            settings.Language);
    }
}
