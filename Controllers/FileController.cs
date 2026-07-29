using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using techstore_api.Services.Interfaces;

namespace techstore_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FileController : ControllerBase
    {
        private readonly IFileService _fileService;

        public FileController(IFileService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>
        /// Sube una imagen para un producto específico
        /// </summary>
        [HttpPost("upload-product-image/{productId}")]
        [Authorize(Roles = "ADMINISTRADOR,VENDEDOR")]
        public async Task<IActionResult> UploadProductImage(int productId, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No se ha proporcionado ningún archivo." });
            }

            if (!_fileService.IsValidImageFile(file))
            {
                return BadRequest(new { message = "El archivo no es una imagen válida. Formatos permitidos: JPG, PNG, GIF, BMP. Tamaño máximo: 5MB." });
            }

            try
            {
                var imageUrl = await _fileService.SaveProductImageAsync(file, productId);
                
                return Ok(new 
                { 
                    message = "Imagen subida exitosamente.",
                    imageUrl = imageUrl
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al subir la imagen: {ex.Message}" });
            }
        }

        /// <summary>
        /// Elimina la imagen de un producto
        /// </summary>
        [HttpDelete("delete-product-image")]
        [Authorize(Roles = "ADMINISTRADOR,VENDEDOR")]
        public async Task<IActionResult> DeleteProductImage([FromQuery] string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
            {
                return BadRequest(new { message = "URL de imagen no proporcionada." });
            }

            try
            {
                var success = await _fileService.DeleteProductImageAsync(imageUrl);
                
                if (success)
                {
                    return Ok(new { message = "Imagen eliminada exitosamente." });
                }
                else
                {
                    return NotFound(new { message = "Imagen no encontrada." });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error al eliminar la imagen: {ex.Message}" });
            }
        }
    }
}