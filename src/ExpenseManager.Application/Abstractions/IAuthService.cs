namespace ExpenseManager.Application.Abstractions;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<AuthResult?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<AuthResult?> LoginWithGoogleAsync(string idToken, CancellationToken cancellationToken = default);
}

public sealed record AuthResult(string UserId, string Email, string Token);
