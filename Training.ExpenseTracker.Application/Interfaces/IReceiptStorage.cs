namespace Training.ExpenseTracker.Application.Interfaces;

public interface IReceiptStorage
{
    Task DeleteAsync(string imageUrlId, CancellationToken ct);
    Task<(string ImageUrl, string ImageUrlId)> UploadAsync(Stream stream, string fileName, CancellationToken ct);
}