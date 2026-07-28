using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using techstore_api.DataBase;
using techstore_api.Dtos.Categories;
using techstore_api.Services.Interfaces;
using TechStoreApi.Dtos.Common;
using Microsoft.EntityFrameworkCore;

namespace techstore_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly TiendaDbContext _context;

        public CategoriesController(ICategoryService categoryService, TiendaDbContext context)
        {
            _categoryService = categoryService;
            _context = context;
        }

        /// <summary>
        /// Obtiene todas las categorías de productos (público)
        /// </summary>
        [HttpGet("products")]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseDto<List<CategoryDto>>>> GetProductCategories()
        {
            var categories = await _context.Categories
                .Where(c => c.Type == "Product")
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Type = c.Type
                })
                .ToListAsync();

            return Ok(new ResponseDto<List<CategoryDto>>
            {
                Status = true,
                StatusCode = 200,
                Message = "Categorías de productos obtenidas exitosamente",
                Data = categories
            });
        }

        /// <summary>
        /// Obtiene todas las categorías (solo productos)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "ADMINISTRADOR,VENDEDOR")]
        public async Task<ActionResult<ResponseDto<List<CategoryDto>>>> GetAllCategories()
        {
            var categories = await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Type = c.Type
                })
                .ToListAsync();

            return Ok(new ResponseDto<List<CategoryDto>>
            {
                Status = true,
                StatusCode = 200,
                Message = "Categorías obtenidas exitosamente",
                Data = categories
            });
        }

        /// <summary>
        /// Obtiene una categoría por ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize(Roles = "ADMINISTRADOR,VENDEDOR")]
        public async Task<ActionResult<ResponseDto<CategoryDto>>> GetCategory(int id)
        {
            var category = await _context.Categories
                .Where(c => c.Id == id)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Type = c.Type
                })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return NotFound(new ResponseDto<CategoryDto>
                {
                    Status = false,
                    StatusCode = 404,
                    Message = "Categoría no encontrada"
                });
            }

            return Ok(new ResponseDto<CategoryDto>
            {
                Status = true,
                StatusCode = 200,
                Message = "Categoría obtenida exitosamente",
                Data = category
            });
        }

        /// <summary>
        /// Crea una nueva categoría
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<ActionResult<ResponseDto<CategoryDto>>> CreateCategory([FromBody] CategoryCreateDto categoryDto)
        {
            var response = await _categoryService.CreateAsync(categoryDto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Actualiza una categoría existente
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<ActionResult<ResponseDto<CategoryDto>>> UpdateCategory(int id, [FromBody] CategoryEditDto categoryDto)
        {
            var response = await _categoryService.EditAsync(categoryDto, id);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Elimina una categoría
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            var response = await _categoryService.DeleteAsync(id);
            if (response.StatusCode == 204)
            {
                return NoContent();
            }
            return StatusCode(response.StatusCode, response);
        }
    }
}