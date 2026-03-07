namespace ExpenseManager.Application.Features.Settings;

public sealed record UserSettingsDto(
    string DefaultCurrency,
    string DateFormat,
    string Theme,
    string? Language);
