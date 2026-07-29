using techstore_api.Dtos.Products;
using TechStoreApi.Dtos.Common;

namespace techstore_api.Services.Interfaces
{
    public interface IProductService
    {
        Task<ResponseDto<List<ProductDto>>> GetListAsync(string searchTerm = "", int page = 1, int pageSize = 0);
        Task<ResponseDto<ProductDto>> GetOneByIdAsync(int id);
        Task<ResponseDto<object>> CreateAsync(ProductCreateDto dto);
        Task<ResponseDto<object>> EditAsync(ProductEditDto dto, int id);
        Task<ResponseDto<object>> DeleteAsync(int id);
    }
}