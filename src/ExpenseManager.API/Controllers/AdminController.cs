using ExpenseManager.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseManager.API.Controllers;

/// <summary>
/// Secret admin endpoints. Access requires X-Admin-Key header matching Admin__SecretKey (env).
/// Not linked from the app; only for owners who know the URL and secret.
/// </summary>
[ApiController]
[Route("api/admin")]
public sealed class AdminController(ExpenseManagerDbContext db) : ControllerBase
{
    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<AdminUserSummaryDto>>> GetUsers(CancellationToken cancellationToken)
    {
        var users = await db.Users
            .Select(u => new { u.Id, u.Email })
            .ToListAsync(cancellationToken);

        var expenseCounts = await db.Expenses
            .Select(e => e.UserId)
            .ToListAsync(cancellationToken);
        var expenseByUser = expenseCounts
            .GroupBy(x => x)
            .ToDictionary(g => g.Key, g => g.Count());

        var categoryCounts = await db.Categories
            .Select(c => c.UserId)
            .ToListAsync(cancellationToken);
        var categoryByUser = categoryCounts
            .GroupBy(x => x)
            .ToDictionary(g => g.Key, g => g.Count());

        var list = users
            .Select(u => new AdminUserSummaryDto(
                u.Id,
                u.Email ?? "",
                expenseByUser.GetValueOrDefault(u.Id, 0),
                categoryByUser.GetValueOrDefault(u.Id, 0)))
            .ToList();

        return Ok(list);
    }

    public sealed record AdminUserSummaryDto(string UserId, string Email, int ExpenseCount, int CategoryCount);
}
