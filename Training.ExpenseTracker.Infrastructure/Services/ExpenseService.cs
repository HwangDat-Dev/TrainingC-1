using System.Text;
using Microsoft.EntityFrameworkCore;
using Training.ExpenseTracker.Application.DTO;
using Training.ExpenseTracker.Application.DTO.Expenses;
using Training.ExpenseTracker.Application.Interfaces;
using Training.ExpenseTracker.Domain.Entities;

namespace Training.ExpenseTracker.Infrastructure.Security;

public class ExpenseService : IExpenseService
{
    private readonly IAppDbContext _db;
    private readonly IReceiptStorage _receiptStorage;


    public ExpenseService(IAppDbContext db, IReceiptStorage receiptStorage)
    {
        _db = db;
        _receiptStorage = receiptStorage;
    }

    public async Task<ResponseExpenses> CreateAsync(Guid userId, CreateExpenses request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Category))
            throw new ArgumentException("Doanh mục bắt buộc phải có");

        if (request.Amount <= 0)
            throw new ArgumentException("Số tiền phải lớn hơn 0");

        var userExists = await _db.Users.AnyAsync(u => u.Id == userId, ct);
        if (!userExists)
            throw new UnauthorizedAccessException("Không tìm thấy mã người dùng");

        var entity = new Expense()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Amount = request.Amount,
            Category = request.Category.Trim(),
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            SpendDate = request.SpendDate == default ? DateTime.UtcNow : request.SpendDate.ToUniversalTime(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = null
        };

        _db.Expenses.Add(entity);
        await _db.SaveChangesAsync(ct);

        return new ResponseExpenses()
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Amount = entity.Amount,
            Category = entity.Category,
            Note = entity.Note,
            SpendDate = entity.SpendDate,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public async Task<PagedResult<ResponseExpenses>> GetListAsync(Guid userId, GetExpensesRequest request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : request.PageSize;
        if (pageSize > 100) pageSize = 100;

        var query = _db.Expenses
            .AsNoTracking()
            .Where(x => x.UserId == userId);

        if (request.From.HasValue)
        {
            var from = request.From.Value.ToUniversalTime();
            query = query.Where(x => x.SpendDate >= from);
        }

        if (request.To.HasValue)
        {
            var to = request.To.Value.ToUniversalTime();
            query = query.Where(x => x.SpendDate <= to);
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var cat = request.Category.Trim();
            query = query.Where(x => x.Category == cat);
        }

        if (request.MinAmount.HasValue)
            query = query.Where(x => x.Amount >= request.MinAmount.Value);

        if (request.MaxAmount.HasValue)
            query = query.Where(x => x.Amount <= request.MaxAmount.Value);

        query = request.Sort switch
        {
            ExpenseSort.SpendDateAsc => query.OrderBy(x => x.SpendDate).ThenByDescending(x => x.CreatedAt),
            ExpenseSort.SpendDateDesc => query.OrderByDescending(x => x.SpendDate).ThenByDescending(x => x.CreatedAt),
            ExpenseSort.AmountAsc => query.OrderBy(x => x.Amount).ThenByDescending(x => x.CreatedAt),
            ExpenseSort.AmountDesc => query.OrderByDescending(x => x.Amount).ThenByDescending(x => x.CreatedAt),
            ExpenseSort.CreatedAtDesc => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.SpendDate).ThenByDescending(x => x.CreatedAt)
        };

        var totalItems = await query.CountAsync(ct);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new ResponseExpenses()
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

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return new PagedResult<ResponseExpenses>
        {
            Page = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Items = items
        };
    }

    
    public async Task<ResponseExpenses?> GetByIdAsync(Guid userId, Guid expenseId, CancellationToken ct)
    {
        return await _db.Expenses
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.Id == expenseId)
            .Select(x => new ResponseExpenses()
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
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ResponseExpenses?> UpdateAsync(
        Guid userId,
        Guid expenseId,
        UpdateExpenses request,
        CancellationToken ct)
    {
        var entity = await _db.Expenses
            .FirstOrDefaultAsync(x => x.Id == expenseId && x.UserId == userId, ct);

        if (entity is null)
            return null;

        if (request.Amount.HasValue)
            entity.Amount = request.Amount.Value;

        if (request.Category is not null)
            entity.Category = request.Category.Trim();

        if (request.Note is not null)
            entity.Note = string.IsNullOrWhiteSpace(request.Note)
                ? null
                : request.Note.Trim();

        if (request.SpendDate.HasValue)
            entity.SpendDate = request.SpendDate.Value.ToUniversalTime();

        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new ResponseExpenses
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Amount = entity.Amount,
            Category = entity.Category,
            Note = entity.Note,
            SpendDate = entity.SpendDate,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    public async Task<bool> DeleteAsync(Guid userId, Guid expenseId, CancellationToken ct)
    {
        var entity = await _db.Expenses
            .FirstOrDefaultAsync(x => x.Id == expenseId && x.UserId == userId, ct);

        if (entity is null)
            return false;

        _db.Expenses.Remove(entity);
        await _db.SaveChangesAsync(ct);

        return true;
    }

    public async Task<ExpenseSummaryResponse> GetSummaryAsync(
        Guid userId,
        DateTime? from,
        DateTime? to,
        CancellationToken ct)
    {
        var toUtc = (to ?? DateTime.UtcNow).ToUniversalTime();
        var fromUtc = (from ?? toUtc.AddDays(-30)).ToUniversalTime();

        if (fromUtc > toUtc)
            throw new ArgumentException("'from' phải nhỏ hơn hoặc bằng 'to'.");

        var baseQuery = _db.Expenses
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.SpendDate >= fromUtc && x.SpendDate <= toUtc);

        var totalAmount = await baseQuery.SumAsync(x => x.Amount, ct);
        var totalCount = await baseQuery.CountAsync(ct);

        var byCategory = await baseQuery
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
            From = fromUtc,
            To = toUtc,
            TotalAmount = totalAmount,
            TotalCount = totalCount,
            ByCategory = byCategory
        };
    }

    public async Task<(byte[] Content, string FileName)> ExportCSV(Guid userId, GetExpensesRequest request, CancellationToken ct)
    {
        var query = _db.Expenses
        .AsNoTracking()
        .Where(x => x.UserId == userId);

        if (request.From.HasValue)
        {
            var from = request.From.Value.ToUniversalTime();
            query = query.Where(x => x.SpendDate >= from);
        }

        if (request.To.HasValue)
        {
            var to = request.To.Value.ToUniversalTime();
            query = query.Where(x => x.SpendDate <= to);
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var cat = request.Category.Trim();
            query = query.Where(x => x.Category == cat);
        }

        if (request.MinAmount.HasValue)
            query = query.Where(x => x.Amount >= request.MinAmount.Value);

        if (request.MaxAmount.HasValue)
            query = query.Where(x => x.Amount <= request.MaxAmount.Value);

        query = request.Sort switch
        {
            ExpenseSort.SpendDateAsc => query.OrderBy(x => x.SpendDate).ThenByDescending(x => x.CreatedAt),
            ExpenseSort.SpendDateDesc => query.OrderByDescending(x => x.SpendDate).ThenByDescending(x => x.CreatedAt),
            ExpenseSort.AmountAsc => query.OrderBy(x => x.Amount).ThenByDescending(x => x.CreatedAt),
            ExpenseSort.AmountDesc => query.OrderByDescending(x => x.Amount).ThenByDescending(x => x.CreatedAt),
            ExpenseSort.CreatedAtDesc => query.OrderByDescending(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.SpendDate).ThenByDescending(x => x.CreatedAt)
        };

        var rows = await query
            .Select(x => new
            {
                x.Id,
                x.UserId,
                x.Amount,
                x.Category,
                x.Note,
                x.SpendDate,
                x.CreatedAt,
                x.UpdatedAt
            })
            .ToListAsync(ct);

        static string CsvEscape(string? value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            var v = value.Replace("\"", "\"\"");
            if (v.Contains(',') || v.Contains('\n') || v.Contains('\r') || v.Contains('"'))
                return $"\"{v}\"";
            return v;
        }

        var sb = new StringBuilder();

        sb.AppendLine("Id,UserId,Amount,Category,Note,SpendDate,CreatedAt,UpdatedAt");

        foreach (var r in rows)
        {
            sb.Append(CsvEscape(r.Id.ToString())); sb.Append(',');
            sb.Append(CsvEscape(r.UserId.ToString())); sb.Append(',');
            sb.Append(CsvEscape(r.Amount.ToString())); sb.Append(',');
            sb.Append(CsvEscape(r.Category)); sb.Append(',');
            sb.Append(CsvEscape(r.Note)); sb.Append(',');
            sb.Append(CsvEscape(r.SpendDate.ToString("O"))); sb.Append(',');
            sb.Append(CsvEscape(r.CreatedAt.ToString("O"))); sb.Append(',');
            sb.Append(CsvEscape(r.UpdatedAt?.ToString("O")));
            sb.AppendLine();
        }

        var utf8Bom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        var bytes = utf8Bom.GetBytes(sb.ToString());

        var fileName = $"expenses_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        return (bytes, fileName);
    }

    public async Task<ResponseExpenses?> UploadImageAsync(
        Guid userId,
        Guid expenseId,
        Stream fileStream,
        string fileName,
        string contentType,
        long length,
        CancellationToken ct)
    {
        var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowed.Contains(contentType))
            throw new ArgumentException("Chỉ nhận jpg/png/webp.");

        if (length <= 0)
            throw new ArgumentException("File rỗng.");

        if (length > 5 * 1024 * 1024)
            throw new ArgumentException("File tối đa 5MB.");

        var entity = await _db.Expenses
            .FirstOrDefaultAsync(x => x.Id == expenseId && x.UserId == userId, ct);

        if (entity is null)
            return null;

        var (url, publicId) = await _receiptStorage.UploadAsync(fileStream, fileName, ct);

        if (!string.IsNullOrWhiteSpace(entity.ImageUrlId))
            await _receiptStorage.DeleteAsync(entity.ImageUrlId, ct);

        entity.ImageUrl = url;
        entity.ImageUrlId = publicId;
        entity.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        return new ResponseExpenses
        {
            Id = entity.Id,
            UserId = entity.UserId,
            Amount = entity.Amount,
            Category = entity.Category,
            Note = entity.Note,
            SpendDate = entity.SpendDate,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            ImageUrl = entity.ImageUrl,
            ImageUrlId = entity.ImageUrlId
        };
    }
}