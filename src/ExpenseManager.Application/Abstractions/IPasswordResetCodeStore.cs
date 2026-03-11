namespace ExpenseManager.Application.Abstractions;

public interface IPasswordResetCodeStore
{
    Task StoreAsync(string email, string code, DateTimeOffset expiresAt, CancellationToken cancellationToken = default);
    Task<bool> TryConsumeAsync(string email, string code, CancellationToken cancellationToken = default);
}
