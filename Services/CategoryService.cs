using AutoMapper;
using techstore_api.DataBase;
using techstore_api.Dtos.Categories;
using techstore_api.Services.Interfaces;
using TechStoreApi.Dtos.Common;
using Microsoft.EntityFrameworkCore;
using HttpStatusCode = TechStoreApi.Constants.CodigosDeEstadoHttp;
using techstore_api.DataBase.Entities;



namespace techstore_api.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly TiendaDbContext _contexto;
        private readonly IMapper _mapper;

        public CategoryService(TiendaDbContext context, IMapper mapper)
        {
            _contexto = context;
            _mapper = mapper;
        }

        public async Task<ResponseDto<List<CategoryDto>>> GetListAsync(string searchTerm = "", int page = 1, int pageSize = 0)
        {
            var consulta = _contexto.Categories.AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                consulta = consulta.Where(c => c.Name.Contains(searchTerm));
            }

            var categorias = await consulta.ToListAsync();
            var categoriasDto = _mapper.Map<List<CategoryDto>>(categorias);

            return new ResponseDto<List<CategoryDto>>
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Data = categoriasDto
            };
        }

        public async Task<ResponseDto<CategoryDto>> GetOneByIdAsync(int id)
        {
            var categoria = await _contexto.Categories.FindAsync(id);
            if (categoria == null)
            {
                return new ResponseDto<CategoryDto>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.NO_ENCONTRADO,
                    Message = "Categoría no encontrada."
                };
            }

            var categoriaDto = _mapper.Map<CategoryDto>(categoria);

            return new ResponseDto<CategoryDto>
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Data = categoriaDto
            };
        }

        public async Task<ResponseDto<object>> CreateAsync(CategoryCreateDto dto)
        {
            var categoriaExiste = await _contexto.Categories.AnyAsync(c => c.Name == dto.Name);
            if (categoriaExiste)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                    Message = "Ya existe una categoría con este nombre."
                };
            }

            var categoria = _mapper.Map<CategoryEntity>(dto);
            await _contexto.Categories.AddAsync(categoria);
            await _contexto.SaveChangesAsync();

            return new ResponseDto<object>
            {
                Status = true,
                StatusCode = HttpStatusCode.CREADO,
                Message = "Categoría creada exitosamente."
            };
        }

        public async Task<ResponseDto<object>> EditAsync(CategoryEditDto dto, int id)
        {
            var categoria = await _contexto.Categories.FindAsync(id);
            if (categoria == null)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.NO_ENCONTRADO,
                    Message = "Categoría no encontrada."
                };
            }

            var categoriaExiste = await _contexto.Categories.AnyAsync(c => c.Name == dto.Name && c.Id != id);
            if (categoriaExiste)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                    Message = "Ya existe otra categoría con este nombre."
                };
            }

            _mapper.Map(dto, categoria);
            _contexto.Categories.Update(categoria);
            await _contexto.SaveChangesAsync();

            return new ResponseDto<object>
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Categoría actualizada exitosamente."
            };
        }

        public async Task<ResponseDto<object>> DeleteAsync(int id)
        {
            var categoria = await _contexto.Categories.FindAsync(id);
            if (categoria == null)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.NO_ENCONTRADO,
                    Message = "Categoría no encontrada."
                };
            }

            _contexto.Categories.Remove(categoria);
            await _contexto.SaveChangesAsync();

            return new ResponseDto<object>
            {
                Status = true,
                StatusCode = HttpStatusCode.SIN_CONTENIDO,
                Message = "Categoría eliminada exitosamente."
            };
        }
    }
}