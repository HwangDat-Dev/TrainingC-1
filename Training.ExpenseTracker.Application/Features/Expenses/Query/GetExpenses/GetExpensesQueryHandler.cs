using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Training.ExpenseTracker.Application.Abstractions;
using Training.ExpenseTracker.Application.DTO;
using Training.ExpenseTracker.Application.DTO.Expenses;
using Training.ExpenseTracker.Application.Interfaces;

namespace Training.ExpenseTracker.Application.Features.Expenses.Query.GetExpenses;

public sealed class GetExpensesQueryHandler
    : IQueryHandler<GetExpensesQuery, PagedResult<ResponseExpenses>>
{
    private readonly IReadDbContext _db;
    private readonly ILogger<GetExpensesQueryHandler> _logger;

    public GetExpensesQueryHandler(IReadDbContext db, ILogger<GetExpensesQueryHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<ResponseExpenses>> Handle(GetExpensesQuery query, CancellationToken ct)
    {
        _logger.LogInformation("[DB:READ] GetExpenses for user: {UserId}", query.UserId);
        
        var req = query.Request;

        var q = _db.Expenses
            .AsNoTracking()
            .Where(x => x.UserId == query.UserId);

        if (req.From.HasValue) q = q.Where(x => x.SpendDate >= req.From.Value);
        if (req.To.HasValue) q = q.Where(x => x.SpendDate <= req.To.Value);

        if (!string.IsNullOrWhiteSpace(req.Category))
            q = q.Where(x => x.Category == req.Category);

        if (req.MinAmount.HasValue) q = q.Where(x => x.Amount >= req.MinAmount.Value);
        if (req.MaxAmount.HasValue) q = q.Where(x => x.Amount <= req.MaxAmount.Value);

        q = req.Sort switch
        {
            ExpenseSort.SpendDateAsc => q.OrderBy(x => x.SpendDate),
            ExpenseSort.AmountAsc => q.OrderBy(x => x.Amount),
            ExpenseSort.AmountDesc => q.OrderByDescending(x => x.Amount),
            _ => q.OrderByDescending(x => x.SpendDate) 
        };

        var total = await q.CountAsync(ct);

        var items = await q
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(x => new ResponseExpenses
            {
                Id = x.Id,
                UserId = x.UserId,
                Amount = x.Amount,
                Category = x.Category,
                Note = x.Note,
                SpendDate = x.SpendDate,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                ImageUrl = x.ImageUrl,
                ImageUrlId = x.ImageUrlId
            })
            .ToListAsync(ct);

        return new PagedResult<ResponseExpenses>
        {
            Page = req.Page,
            PageSize = req.PageSize,
            TotalItems = total,
            TotalPages = (int)Math.Ceiling(total / (double)req.PageSize),
            Items = items
        };
    }
}