using ExpenseManager.Domain.Common;

namespace ExpenseManager.Domain.Entities;

/// <summary>
/// One row per user. Stores default currency, date format, theme, and optional language.
/// </summary>
public sealed class UserSettings
{
    private UserSettings() { } // EF

    public UserSettings(string userId, string defaultCurrency, string dateFormat, string theme, string? language)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new DomainException("UserId is required.");
        UserId = userId;
        SetDefaults(defaultCurrency, dateFormat, theme, language);
    }

    public string UserId { get; private set; } = default!;
    public string DefaultCurrency { get; private set; } = "USD";
    public string DateFormat { get; private set; } = "yyyy-MM-dd";
    public string Theme { get; private set; } = "system"; // "light" | "dark" | "system"
    public string? Language { get; private set; }

    public void SetDefaults(string defaultCurrency, string dateFormat, string theme, string? language)
    {
        DefaultCurrency = string.IsNullOrWhiteSpace(defaultCurrency) ? "USD" : defaultCurrency.Trim().ToUpperInvariant();
        DateFormat = string.IsNullOrWhiteSpace(dateFormat) ? "yyyy-MM-dd" : dateFormat.Trim();
        var t = string.IsNullOrWhiteSpace(theme) ? "system" : theme.Trim().ToLowerInvariant();
        Theme = t is "light" or "dark" or "system" ? t : "system";
        Language = string.IsNullOrWhiteSpace(language) ? null : language.Trim();
    }
}
