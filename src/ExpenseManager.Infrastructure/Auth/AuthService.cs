using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using ExpenseManager.Application.Abstractions;
using ExpenseManager.Infrastructure.Identity;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseManager.Infrastructure.Auth;

public sealed class AuthService(
    UserManager<AppUser> userManager,
    IConfiguration configuration,
    IPasswordResetCodeStore codeStore,
    IEmailSender emailSender) : IAuthService
{
    private const int CodeLength = 6;
    private static readonly TimeSpan CodeExpiry = TimeSpan.FromMinutes(15);
    private const string ResetEmailSubject = "Your password reset code - Coin Canvas";
    public async Task<AuthResult> RegisterAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = new AppUser { UserName = email, Email = email };
        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

        var authResult = await LoginAsync(email, password, cancellationToken);
        return authResult ?? throw new InvalidOperationException("Login failed after registration.");
    }

    public async Task<AuthResult?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.CheckPasswordAsync(user, password))
            return null;

        var token = GenerateJwt(user.Id, user.Email!);
        return new AuthResult(user.Id, user.Email!, token);
    }

    public async Task<AuthResult?> LoginWithGoogleAsync(string idToken, CancellationToken cancellationToken = default)
    {
        var clientId = configuration["Google:ClientId"];
        if (string.IsNullOrEmpty(clientId))
            return null;

        try
        {
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { clientId },
            };
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
            var email = payload.Email;
            if (string.IsNullOrEmpty(email) || !payload.EmailVerified)
                return null;

            var user = await FindOrCreateExternalUserAsync(email, cancellationToken);
            if (user is null) return null;

            var token = GenerateJwt(user.Id, user.Email!);
            return new AuthResult(user.Id, user.Email!, token);
        }
        catch (InvalidJwtException)
        {
            return null;
        }
    }

    public async Task RequestPasswordResetAsync(string email, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return;

        var code = GenerateNumericCode(CodeLength);
        var expiresAt = DateTimeOffset.UtcNow.Add(CodeExpiry);
        await codeStore.StoreAsync(email, code, expiresAt, cancellationToken);

        var body = $"Your password reset code is: {code}\n\nIt expires in {CodeExpiry.TotalMinutes} minutes.\n\nIf you didn't request this, you can ignore this email.";
        await emailSender.SendAsync(email, ResetEmailSubject, body, cancellationToken);
    }

    public async Task<bool> ResetPasswordWithCodeAsync(string email, string code, string newPassword, CancellationToken cancellationToken = default)
    {
        if (!await codeStore.TryConsumeAsync(email, code.Trim(), cancellationToken))
            return false;

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
            return false;

        await userManager.RemovePasswordAsync(user);
        var result = await userManager.AddPasswordAsync(user, newPassword);
        return result.Succeeded;
    }

    private static string GenerateNumericCode(int length)
    {
        var bytes = RandomNumberGenerator.GetBytes(length * 2);
        var sb = new System.Text.StringBuilder(length);
        for (var i = 0; i < length; i++)
            sb.Append((bytes[i] % 10).ToString());
        return sb.ToString();
    }

    private async Task<AppUser?> FindOrCreateExternalUserAsync(string email, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user is not null)
            return user;

        user = new AppUser { UserName = email, Email = email, EmailConfirmed = true };
        var password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var result = await userManager.CreateAsync(user, password);
        return result.Succeeded ? user : null;
    }

    private string GenerateJwt(string userId, string email)
    {
        var key = configuration["Jwt:Key"] ?? "ExpenseManagerSecretKeyThatIsAtLeast32CharactersLong!";
        var issuer = configuration["Jwt:Issuer"] ?? "ExpenseManager";
        var audience = configuration["Jwt:Audience"] ?? "ExpenseManager";

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
        };

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
