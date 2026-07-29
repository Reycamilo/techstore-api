using AutoMapper;
using Microsoft.EntityFrameworkCore;
using techstore_api.DataBase;
using techstore_api.DataBase.Entities;
using techstore_api.Dtos.Products;
using techstore_api.Services.Interfaces;
using TechStoreApi.Dtos.Common;
using HttpStatusCode = TechStoreApi.Constants.CodigosDeEstadoHttp;


namespace techstore_api.Services
{
    public class ProductService : IProductService
    {
        private readonly TiendaDbContext _contexto;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;

        public ProductService(TiendaDbContext context, IMapper mapper, IFileService fileService)
        {
            _contexto = context;
            _mapper = mapper;
            _fileService = fileService;
        }

        public async Task<ResponseDto<List<ProductDto>>> GetListAsync(string searchTerm = "", int page = 1, int pageSize = 0)
        {
            var consulta = _contexto.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Select(p => new ProductEntity
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    Category = p.Category,
                    SellerId = p.SellerId,
                    Seller = p.Seller,
                    ImageUrl = p.ImageUrl,
                    FechaCreacion = p.FechaCreacion,
                    FechaActualizacion = p.FechaActualizacion,
                    CreadoPor = p.CreadoPor,
                    ActualizadoPor = p.ActualizadoPor
                })
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                consulta = consulta.Where(p => p.Name.Contains(searchTerm) || p.Description!.Contains(searchTerm) || p.Category.Name.Contains(searchTerm));
            }

            var productos = await consulta.ToListAsync();
            var productosDto = _mapper.Map<List<ProductDto>>(productos);

            return new ResponseDto<List<ProductDto>>
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Data = productosDto
            };
        }

        public async Task<ResponseDto<ProductDto>> GetOneByIdAsync(int id)
        {
            var producto = await _contexto.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .Select(p => new ProductEntity
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    Stock = p.Stock,
                    CategoryId = p.CategoryId,
                    Category = p.Category,
                    SellerId = p.SellerId,
                    Seller = p.Seller,
                    ImageUrl = p.ImageUrl,
                    FechaCreacion = p.FechaCreacion,
                    FechaActualizacion = p.FechaActualizacion,
                    CreadoPor = p.CreadoPor,
                    ActualizadoPor = p.ActualizadoPor
                })
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
            {
                return new ResponseDto<ProductDto>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.NO_ENCONTRADO,
                    Message = "Producto no encontrado."
                };
            }

            var productoDto = _mapper.Map<ProductDto>(producto);

            return new ResponseDto<ProductDto>
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Data = productoDto
            };
        }

        public async Task<ResponseDto<object>> CreateAsync(ProductCreateDto dto)
        {
            var categoriaExiste = await _contexto.Categories.AnyAsync(c => c.Id == dto.CategoryId && c.Type == "Product");
            if (!categoriaExiste)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                    Message = "Categoría de producto no encontrada."
                };
            }

            var vendedorExiste = await _contexto.Users.AnyAsync(u => u.Id == dto.SellerId);
            if (!vendedorExiste)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                    Message = "Vendedor no encontrado."
                };
            }

            // Validar que el usuario autenticado sea el mismo que el SellerId si no es admin
            // (esto se puede reforzar en el controlador si se requiere)

            var producto = _mapper.Map<ProductEntity>(dto);
            
            // Asegurar que ImageUrl se asigne correctamente
            producto.ImageUrl = dto.ImageUrl;
            
            await _contexto.Products.AddAsync(producto);
            await _contexto.SaveChangesAsync();

            return new ResponseDto<object>
            {
                Status = true,
                StatusCode = HttpStatusCode.CREADO,
                Message = "Producto creado exitosamente."
            };
        }

        public async Task<ResponseDto<object>> EditAsync(ProductEditDto dto, int id)
        {
            var producto = await _contexto.Products.FindAsync(id);
            if (producto == null)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.NO_ENCONTRADO,
                    Message = "Producto no encontrado."
                };
            }

            var categoriaExiste = await _contexto.Categories.AnyAsync(c => c.Id == dto.CategoryId);
            if (!categoriaExiste)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                    Message = "Categoría no encontrada."
                };
            }

            _mapper.Map(dto, producto);
            
            // Asegurar que ImageUrl se asigne correctamente
            producto.ImageUrl = dto.ImageUrl;
            
            _contexto.Products.Update(producto);
            await _contexto.SaveChangesAsync();

            return new ResponseDto<object>
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Producto actualizado exitosamente."
            };
        }

        public async Task<ResponseDto<object>> DeleteAsync(int id)
        {
            var producto = await _contexto.Products.FindAsync(id);
            if (producto == null)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.NO_ENCONTRADO,
                    Message = "Producto no encontrado."
                };
            }

            // Eliminar imagen si existe
            if (!string.IsNullOrEmpty(producto.ImageUrl))
            {
                await _fileService.DeleteProductImageAsync(producto.ImageUrl);
            }

            _contexto.Products.Remove(producto);
            await _contexto.SaveChangesAsync();

            return new ResponseDto<object>
            {
                Status = true,
                StatusCode = HttpStatusCode.SIN_CONTENIDO,
                Message = "Producto eliminado exitosamente."
            };
        }
    }
}