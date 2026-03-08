namespace ExpenseManager.Application.Features.Expenses.Queries;

public sealed record ListExpensesResponse(IReadOnlyList<ExpenseDto> Items, int TotalCount);
