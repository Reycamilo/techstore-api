using techstore_api.Dtos.Orders;
using TechStoreApi.Dtos.Common;

namespace techstore_api.Services.Interfaces
{
    public interface IOrderService
    {
        Task<ResponseDto<List<OrderDto>>> GetListAsync(string searchTerm = "", int page = 1, int pageSize = 0);
        Task<ResponseDto<OrderDto>> GetOneByIdAsync(int id);
        Task<ResponseDto<object>> CreateAsync(OrderCreateDto dto);
        Task<ResponseDto<object>> EditAsync(OrderEditDto dto, int id);
        Task<ResponseDto<object>> UpdateStatusAsync(OrderStatusUpdateDto dto, int id);
        Task<ResponseDto<object>> DeleteAsync(int id);
    }
}