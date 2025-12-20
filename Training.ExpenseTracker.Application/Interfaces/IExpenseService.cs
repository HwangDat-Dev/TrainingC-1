using Training.ExpenseTracker.Application.DTO;
using Training.ExpenseTracker.Application.DTO.Expenses;

namespace Training.ExpenseTracker.Application.Interfaces;

public interface IExpenseService
{
    Task<ResponseExpenses> CreateAsync(Guid userId, CreateExpenses request, CancellationToken ct);
    
    Task<PagedResult<ResponseExpenses>> GetListAsync(Guid userId, GetExpensesRequest request, CancellationToken ct);
    
    
    Task<ResponseExpenses?> GetByIdAsync(Guid userId, Guid expenseId, CancellationToken ct);
    
    Task<ResponseExpenses?> UpdateAsync(Guid userId, Guid expenseId, UpdateExpenses request, CancellationToken ct);
    
    Task<bool> DeleteAsync(Guid userId, Guid expenseId, CancellationToken ct);
    
    Task<ExpenseSummaryResponse> GetSummaryAsync(Guid userId, DateTime? from, DateTime? to, CancellationToken ct); 
    
    Task<(byte[] Content, string FileName)> ExportCSV(Guid userId, GetExpensesRequest request, CancellationToken ct);
    
    Task<ResponseExpenses?> UploadImageAsync(Guid userId, Guid expenseId, Stream fileStream, string fileName, string contentType, long length, CancellationToken ct);
}