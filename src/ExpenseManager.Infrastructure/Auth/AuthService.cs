using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ExpenseManager.Application.Abstractions;
using ExpenseManager.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ExpenseManager.Infrastructure.Auth;

public sealed class AuthService(
    UserManager<AppUser> userManager,
    IConfiguration configuration) : IAuthService
{
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
