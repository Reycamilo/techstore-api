using AutoMapper;
using Microsoft.EntityFrameworkCore;
using techstore_api.DataBase;
using techstore_api.DataBase.Entities;
using techstore_api.Dtos.Transactions;
using techstore_api.Services.Interfaces;
using TechStoreApi.Dtos.Common;
using HttpStatusCode = TechStoreApi.Constants.CodigosDeEstadoHttp;


namespace techstore_api.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly TiendaDbContext _contexto;
        private readonly IMapper _mapper;

        public TransactionService(TiendaDbContext context, IMapper mapper)
        {
            _contexto = context;
            _mapper = mapper;
        }

        public async Task<ResponseDto<List<TransactionDto>>> GetMyTransactionsAsync(string userId)
        {
            var transacciones = await _contexto.Transactions
                .Include(t => t.Order)
                .Where(t => t.Order.UserId == userId)
                .OrderByDescending(t => t.FechaCreacion)
                .ToListAsync();

            return new ResponseDto<List<TransactionDto>>
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Transacciones obtenidas exitosamente.",
                Data = _mapper.Map<List<TransactionDto>>(transacciones)
            };
        }

        public async Task<ResponseDto<List<TransactionDto>>> GetListAsync()
        {
            var transacciones = await _contexto.Transactions
                .Include(t => t.Order)
                .OrderByDescending(t => t.FechaCreacion)
                .ToListAsync();

            return new ResponseDto<List<TransactionDto>>
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Transacciones obtenidas exitosamente.",
                Data = _mapper.Map<List<TransactionDto>>(transacciones)
            };
        }

        public async Task<ResponseDto<TransactionDto>> CreateAsync(TransactionCreateDto dto)
        {
            var orden = await _contexto.Orders.FindAsync(dto.OrderId);
            if (orden == null)
            {
                return new ResponseDto<TransactionDto>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                    Message = $"Orden con ID {dto.OrderId} no encontrada."
                };
            }

            var transaccion = _mapper.Map<PaymentTransactionEntity>(dto);
            await _contexto.Transactions.AddAsync(transaccion);
            await _contexto.SaveChangesAsync();

            // Recargar la orden asociada para armar el DTO del historial
            await _contexto.Entry(transaccion).Reference(t => t.Order).LoadAsync();

            return new ResponseDto<TransactionDto>
            {
                Status = true,
                StatusCode = HttpStatusCode.CREADO,
                Message = "Transacción registrada exitosamente.",
                Data = _mapper.Map<TransactionDto>(transaccion)
            };
        }
    }
}