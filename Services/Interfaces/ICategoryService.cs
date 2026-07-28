using techstore_api.Dtos.Categories;
using TechStoreApi.Dtos.Common;

namespace techstore_api.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<ResponseDto<List<CategoryDto>>> GetListAsync(string searchTerm = "", int page = 1, int pageSize = 0);
        Task<ResponseDto<CategoryDto>> GetOneByIdAsync(int id);
        Task<ResponseDto<object>> CreateAsync(CategoryCreateDto dto);
        Task<ResponseDto<object>> EditAsync(CategoryEditDto dto, int id);
        Task<ResponseDto<object>> DeleteAsync(int id);
    }
}