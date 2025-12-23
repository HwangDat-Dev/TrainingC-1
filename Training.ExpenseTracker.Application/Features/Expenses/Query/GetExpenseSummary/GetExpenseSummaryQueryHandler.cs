using Microsoft.EntityFrameworkCore;
using Training.ExpenseTracker.Application.Abstractions;
using Training.ExpenseTracker.Application.DTO.Expenses;
using Training.ExpenseTracker.Application.Interfaces;

namespace Training.ExpenseTracker.Application.Features.Expenses.Query.GetExpenseSummary;

public sealed class GetExpenseSummaryQueryHandler : IQueryHandler<GetExpenseSummaryQuery, ExpenseSummaryResponse> {
    private readonly IReadDbContext _db;

    public GetExpenseSummaryQueryHandler(IReadDbContext db)
    {
        _db = db;
    }

    public async Task<ExpenseSummaryResponse> Handle(GetExpenseSummaryQuery query, CancellationToken ct)
    {
        var nowUtc = DateTime.UtcNow;

        var monthStartUtc = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthEndUtc = monthStartUtc.AddMonths(1).AddTicks(-1);

        var from = query.From.HasValue
            ? DateTime.SpecifyKind(query.From.Value, DateTimeKind.Utc)
            : monthStartUtc;

        var to = query.To.HasValue
            ? DateTime.SpecifyKind(query.To.Value, DateTimeKind.Utc)
            : monthEndUtc;

        var q = _db.Expenses.AsNoTracking()
            .Where(x => x.UserId == query.UserId)
            .Where(x => x.SpendDate >= from && x.SpendDate <= to);

        var totalAmount = await q.SumAsync(x => x.Amount, ct);
        var totalCount = await q.CountAsync(ct);

        var byCategory = await q
            .GroupBy(x => x.Category)
            .Select(g => new ExpenseSummaryByCategory
            {
                Category = g.Key,
                TotalAmount = g.Sum(x => x.Amount),
                Count = g.Count()
            })
            .OrderByDescending(x => x.TotalAmount)
            .ToListAsync(ct);

        return new ExpenseSummaryResponse
        {
            From = from,
            To = to,
            TotalAmount = totalAmount,
            TotalCount = totalCount,
            ByCategory = byCategory
        };
    }
}