using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Training.ExpenseTracker.Application.Interfaces;

namespace Training.ExpenseTracker.Infrastructure.Storage;

public class CloudinaryReceiptStorage : IReceiptStorage
{
    private readonly Cloudinary _cloudinary;

    public CloudinaryReceiptStorage(Cloudinary cloudinary)
    {
        _cloudinary = cloudinary;
    }

    public async Task DeleteAsync(string imageUrlId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(imageUrlId))
            return;

        var delParams = new DeletionParams(imageUrlId)
        {
            ResourceType = ResourceType.Image
        };

        await _cloudinary.DestroyAsync(delParams);
    }

    public async Task<(string ImageUrl, string ImageUrlId)> UploadAsync(Stream stream, string fileName, CancellationToken ct)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, stream),
            Folder = "expense-receipts"
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.StatusCode != System.Net.HttpStatusCode.OK && result.StatusCode != System.Net.HttpStatusCode.Created)
            throw new Exception($"Upload failed: {result.Error?.Message}");

        return (result.SecureUrl.ToString(), result.PublicId);
    }
}