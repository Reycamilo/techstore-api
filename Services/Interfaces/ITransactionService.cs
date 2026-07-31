using techstore_api.Dtos.Transactions;
using TechStoreApi.Dtos.Common;

namespace techstore_api.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<ResponseDto<List<TransactionDto>>> GetMyTransactionsAsync(string userId);
        Task<ResponseDto<List<TransactionDto>>> GetListAsync();
        Task<ResponseDto<TransactionDto>> CreateAsync(TransactionCreateDto dto);
    }
}