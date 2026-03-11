namespace ExpenseManager.Infrastructure.Persistence;

public sealed class PasswordResetCode
{
    public string Email { get; set; } = "";
    public string Code { get; set; } = "";
    public string ResetToken { get; set; } = "";
    public DateTimeOffset ExpiresAtUtc { get; set; }
}
