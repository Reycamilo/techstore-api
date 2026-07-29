using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using techstore_api.DataBase;
using techstore_api.Dtos.Products;
using techstore_api.Services.Interfaces;
using TechStoreApi.Dtos.Common;

namespace techstore_api.Controllers 
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController: ControllerBase
    {
        private readonly IProductService _productService;
        private readonly TiendaDbContext _context;

        public ProductsController(IProductService productService, TiendaDbContext context)
        {
            _productService = productService;
            _context = context;
        }

        /// <summary>
        /// Obtiene el catálogo de productos (público - para clientes)
        /// </summary>
        [HttpGet("catalog")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<List<ProductDto>>>> GetProductCatalog()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Where(p => p.Category.Type == "Product")
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    SellerId = p.SellerId,
                    SellerName = $"{p.Seller.FirstName} {p.Seller.LastName}",
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();

            return Ok(new ResponseDto<List<ProductDto>>
            {
                Status = true,
                StatusCode = 200,
                Message = "Catálogo de productos obtenido exitosamente",
                Data = products
            });
        }

        /// <summary>
        /// Obtiene mis productos (para vendedores)
        /// </summary>
        [HttpGet("my-products")]
        [Authorize(Roles = "VENDEDOR")]
        public async Task<ActionResult<ResponseDto<List<ProductDto>>>> GetMyProducts()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ResponseDto<List<ProductDto>>
                {
                    Status = false,
                    StatusCode = 401,
                    Message = "Usuario no autenticado"
                });
            }

            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Where(p => p.SellerId == userId && p.Category.Type == "Product")
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    SellerId = p.SellerId,
                    SellerName = $"{p.Seller.FirstName} {p.Seller.LastName}",
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();

            return Ok(new ResponseDto<List<ProductDto>>
            {
                Status = true,
                StatusCode = 200,
                Message = "Mis productos obtenidos exitosamente",
                Data = products
            });
        }

        /// <summary>
        /// Obtiene todos los productos (para administradores, vendedores y clientes autenticados)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "ADMINISTRADOR,VENDEDOR,CLIENTE")]
        public async Task<ActionResult<ResponseDto<List<ProductDto>>>> GetAllProducts()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Where(p => p.Category.Type == "Product")
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    SellerId = p.SellerId,
                    SellerName = $"{p.Seller.FirstName} {p.Seller.LastName}",
                    ImageUrl = p.ImageUrl
                })
                .ToListAsync();

            return Ok(new ResponseDto<List<ProductDto>>
            {
                Status = true,
                StatusCode = 200,
                Message = "Productos obtenidos exitosamente",
                Data = products
            });
        }

        /// <summary>
        /// Obtiene un producto por ID (público)
        /// </summary>
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<ProductDto>>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Where(p => p.Id == id && p.Category.Type == "Product")
                .Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,
                    SellerId = p.SellerId,
                    SellerName = $"{p.Seller.FirstName} {p.Seller.LastName}",
                    ImageUrl = p.ImageUrl
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(new ResponseDto<ProductDto>
                {
                    Status = false,
                    StatusCode = 404,
                    Message = "Producto no encontrado"
                });
            }

            return Ok(new ResponseDto<ProductDto>
            {
                Status = true,
                StatusCode = 200,
                Message = "Producto obtenido exitosamente",
                Data = product
            });
        }

        /// <summary>
        /// Crea un nuevo producto
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "VENDEDOR,ADMINISTRADOR")]
        public async Task<ActionResult<ResponseDto<ProductDto>>> CreateProduct([FromBody] ProductCreateDto productDto)
        {
            var response = await _productService.CreateAsync(productDto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Crea un nuevo producto con imagen
        /// </summary>
        [HttpPost("with-image")]
        [Authorize(Roles = "VENDEDOR,ADMINISTRADOR")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<ResponseDto<ProductDto>>> CreateProductWithImage(
            [FromForm] string name,
            [FromForm] string description,
            [FromForm] decimal price,
            [FromForm] int stock,
            [FromForm] int categoryId,
            [FromForm] string sellerId,
            [FromForm] IFormFile? imageFile)
        {
            // Crear el DTO manualmente
            var productDto = new ProductCreateDto
            {
                Name = name,
                Description = description,
                Price = price,
                Stock = stock,
                CategoryId = categoryId,
                SellerId = sellerId
            };

            // Si se proporciona una imagen, guardarla primero
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    // Validar el archivo
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new ResponseDto<ProductDto>
                        {
                            Status = false,
                            StatusCode = 400,
                            Message = "Formato de archivo no válido. Use JPG, PNG, GIF o BMP."
                        });
                    }

                    if (imageFile.Length > 5 * 1024 * 1024) // 5MB
                    {
                        return BadRequest(new ResponseDto<ProductDto>
                        {
                            Status = false,
                            StatusCode = 400,
                            Message = "El archivo es demasiado grande. Máximo 5MB."
                        });
                    }

                    // Generar nombre único para el archivo usando timestamp
                    var fileName = $"{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid()}{fileExtension}";
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                    
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Actualizar la URL de la imagen en el DTO
                    productDto.ImageUrl = $"/images/products/{fileName}";
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new ResponseDto<ProductDto>
                    {
                        Status = false,
                        StatusCode = 500,
                        Message = $"Error al procesar la imagen: {ex.Message}"
                    });
                }
            }

            // Crear el producto con la imagen (si se proporcionó)
            var response = await _productService.CreateAsync(productDto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "VENDEDOR,ADMINISTRADOR")]
        public async Task<ActionResult<ResponseDto<ProductDto>>> UpdateProduct(int id, [FromBody] ProductEditDto productDto)
        {
            var response = await _productService.EditAsync(productDto, id);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Actualiza un producto existente con imagen
        /// </summary>
        [HttpPut("{id}/with-image")]
        [Authorize(Roles = "VENDEDOR,ADMINISTRADOR")]
        [Consumes("multipart/form-data")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<ActionResult<ResponseDto<ProductDto>>> UpdateProductWithImage(
            int id,
            [FromForm] string name,
            [FromForm] string description,
            [FromForm] decimal price,
            [FromForm] int stock,
            [FromForm] int categoryId,
            [FromForm] string sellerId,
            [FromForm] IFormFile? imageFile,
            [FromForm] string? currentImageUrl = null)
        {
            // Crear el DTO manualmente
            var productDto = new ProductEditDto
            {
                Name = name,
                Description = description,
                Price = price,
                Stock = stock,
                CategoryId = categoryId,
                SellerId = sellerId
            };

            // Si se proporciona una imagen, guardarla primero
            if (imageFile != null && imageFile.Length > 0)
            {
                try
                {
                    // Validar el archivo
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
                    var fileExtension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                    
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        return BadRequest(new ResponseDto<ProductDto>
                        {
                            Status = false,
                            StatusCode = 400,
                            Message = "Formato de archivo no válido. Use JPG, PNG, GIF o BMP."
                        });
                    }

                    if (imageFile.Length > 5 * 1024 * 1024) // 5MB
                    {
                        return BadRequest(new ResponseDto<ProductDto>
                        {
                            Status = false,
                            StatusCode = 400,
                            Message = "El archivo es demasiado grande. Máximo 5MB."
                        });
                    }

                    // Guardar la imagen
                    var fileName = $"{id}-{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                    
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Actualizar la URL de la imagen en el DTO
                    productDto.ImageUrl = $"/images/products/{fileName}";
                }
                catch (Exception ex)
                {
                    return StatusCode(500, new ResponseDto<ProductDto>
                    {
                        Status = false,
                        StatusCode = 500,
                        Message = $"Error al procesar la imagen: {ex.Message}"
                    });
                }
            }
            else
            {
                // Si no se proporciona una nueva imagen, mantener la imagen actual
                productDto.ImageUrl = currentImageUrl;
            }

            // Actualizar el producto
            var response = await _productService.EditAsync(productDto, id);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Elimina un producto
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "VENDEDOR,ADMINISTRADOR")]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            var response = await _productService.DeleteAsync(id);
            if (response.StatusCode == 204)
            {
                return NoContent();
            }
            return StatusCode(response.StatusCode, response);
        }
    }
}