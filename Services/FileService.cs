using techstore_api.Services.Interfaces;

namespace techstore_api.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string?> SaveProductImageAsync(IFormFile file, int productId)
        {
            if (file == null || file.Length == 0)
                return null;

            if (!IsValidImageFile(file))
                throw new ArgumentException("El archivo no es una imagen válida");

            // Crear directorio si no existe
            var uploadPath = Path.Combine(_environment.WebRootPath, "images", "products");
            if (!Directory.Exists(uploadPath))
                Directory.CreateDirectory(uploadPath);

            // Generar nombre único para el archivo
            var fileName = $"{productId}-{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(uploadPath, fileName);

            // Guardar archivo
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Retornar la URL relativa
            return $"/images/products/{fileName}";
        }

        public async Task<bool> DeleteProductImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return false;

            try
            {
                var fileName = Path.GetFileName(imageUrl);
                var filePath = Path.Combine(_environment.WebRootPath, "images", "products", fileName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return true;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public bool IsValidImageFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return false;

            // Verificar extensión
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(fileExtension))
                return false;

            // Verificar tipo MIME
            var allowedMimeTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/bmp" };
            if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
                return false;

            // Verificar tamaño (máximo 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return false;

            return true;
        }
    }
}