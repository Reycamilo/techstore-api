namespace techstore_api.Services.Interfaces
{
    public interface IFileService
    {
        Task<string?> SaveProductImageAsync(IFormFile file, int productId);
        Task<bool> DeleteProductImageAsync(string imageUrl);
        bool IsValidImageFile(IFormFile file);
    }
}