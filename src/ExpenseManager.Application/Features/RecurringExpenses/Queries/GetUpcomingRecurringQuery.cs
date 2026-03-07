using ExpenseManager.Application.Abstractions;
using ExpenseManager.Application.Abstractions.Persistence;
using ExpenseManager.Application.Common.Exceptions;
using MediatR;

namespace ExpenseManager.Application.Features.RecurringExpenses.Queries;

public sealed record GetUpcomingRecurringQuery(int Months = 3) : IRequest<IReadOnlyList<UpcomingExpenseDto>>;

public sealed class GetUpcomingRecurringHandler(IRecurringExpenseRepository repo, ICurrentUserService currentUser)
    : IRequestHandler<GetUpcomingRecurringQuery, IReadOnlyList<UpcomingExpenseDto>>
{
    public async Task<IReadOnlyList<UpcomingExpenseDto>> Handle(GetUpcomingRecurringQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUser.UserId ?? throw new UnauthorizedException();
        var list = await repo.ListAsync(userId, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var monthsAhead = Math.Max(1, request.Months);
        var end = today.AddMonths(monthsAhead);
        var result = new List<UpcomingExpenseDto>();

        foreach (var r in list)
        {
            var dayOfMonth = r.FirstOccurrenceOn.Day;
            var firstYear = r.FirstOccurrenceOn.Year;
            var firstMonth = r.FirstOccurrenceOn.Month;

            for (var i = 0; i < monthsAhead; i++)
            {
                var monthStart = today.AddMonths(i);
                var y = monthStart.Year;
                var m = monthStart.Month;
                if (y < firstYear || (y == firstYear && m < firstMonth)) continue;

                var occurrenceDate = GetOccurrenceDate(y, m, dayOfMonth);
                if (occurrenceDate < today) continue;
                if (occurrenceDate >= end) continue;
                if (r.EndOn.HasValue && occurrenceDate > r.EndOn.Value) continue;

                result.Add(new UpcomingExpenseDto(
                    occurrenceDate,
                    r.Amount,
                    r.Currency,
                    r.Description,
                    r.Category?.Name,
                    r.Id));
            }
        }

        return result.OrderBy(x => x.OccurredOn).ThenBy(x => x.RecurringExpenseId).ToList();
    }

    private static DateOnly GetOccurrenceDate(int year, int month, int dayOfMonth)
    {
        var daysInMonth = DateTime.DaysInMonth(year, month);
        var day = Math.Min(dayOfMonth, daysInMonth);
        return new DateOnly(year, month, day);
    }
}
