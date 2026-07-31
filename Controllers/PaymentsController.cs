using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using techstore_api.DataBase;
using techstore_api.Dtos.Payments;
using techstore_api.Dtos.Transactions;
using techstore_api.Services;
using techstore_api.Services.Interfaces;
using TechStoreApi.Constants;
using TechStoreApi.Dtos.Common;
using Microsoft.EntityFrameworkCore;

namespace techstore_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaypalService _payPal;
        private readonly ITransactionService _transactionService;
        private readonly TiendaDbContext _context;

        public PaymentsController(
            IPaypalService payPal,
            ITransactionService transactionService,
            TiendaDbContext context)
        {
            _payPal = payPal;
            _transactionService = transactionService;
            _context = context;
        }

        /// <summary>
        /// Crea una orden de pago en PayPal a partir de una orden local (paso previo al pago).
        /// </summary>
        [HttpPost("create-order")]
        [Authorize(Roles = "CLIENTE")]
        public async Task<ActionResult<ResponseDto<object>>> CreateOrder([FromBody] CreatePayPalOrderDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var orden = await _context.Orders.FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (orden == null)
            {
                return NotFound(new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = CodigosDeEstadoHttp.NO_ENCONTRADO,
                    Message = "Orden no encontrada."
                });
            }

            if (orden.UserId != userId)
            {
                return StatusCode(CodigosDeEstadoHttp.PROHIBIDO, new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = CodigosDeEstadoHttp.PROHIBIDO,
                    Message = "No puedes pagar una orden que no es tuya."
                });
            }

            if (orden.Status.ToUpper() == "PAGADA")
            {
                return BadRequest(new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = CodigosDeEstadoHttp.SOLICITUD_INCORRECTA,
                    Message = "Esta orden ya fue pagada."
                });
            }

            var result = await _payPal.CreateOrderAsync(orden.TotalAmount, "USD", orden.Id.ToString());
            if (!result.Success)
            {
                return StatusCode(CodigosDeEstadoHttp.SOLICITUD_INCORRECTA, new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = CodigosDeEstadoHttp.SOLICITUD_INCORRECTA,
                    Message = "No se pudo crear la orden de pago en PayPal."
                });
            }

            return Ok(new ResponseDto<object>
            {
                Status = true,
                StatusCode = CodigosDeEstadoHttp.OK,
                Message = "Orden de PayPal creada.",
                Data = new { paypalOrderId = result.PaypalOrderId }
            });
        }

        /// <summary>
        /// Captura (confirma) el pago aprobado en PayPal, registra la transacción y actualiza el estado de la orden.
        /// </summary>
        [HttpPost("capture")]
        [Authorize(Roles = "CLIENTE")]
        public async Task<ActionResult<ResponseDto<object>>> Capture([FromBody] CapturePaymentDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var orden = await _context.Orders.FirstOrDefaultAsync(o => o.Id == dto.OrderId);

            if (orden == null)
            {
                return NotFound(new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = CodigosDeEstadoHttp.NO_ENCONTRADO,
                    Message = "Orden no encontrada."
                });
            }

            if (orden.UserId != userId)
            {
                return StatusCode(CodigosDeEstadoHttp.PROHIBIDO, new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = CodigosDeEstadoHttp.PROHIBIDO,
                    Message = "No puedes pagar una orden que no es tuya."
                });
            }

            var capture = await _payPal.CaptureOrderAsync(dto.PaypalOrderId);
            var nuevoEstado = capture.Success ? "PAGADA" : "PAGO_RECHAZADO";

            // Registrar la transacción (usa el servicio de la feature de transacciones)
            await _transactionService.CreateAsync(new TransactionCreateDto
            {
                OrderId = orden.Id,
                GatewayTransactionId = capture.CaptureId ?? dto.PaypalOrderId,
                Status = nuevoEstado,
                Amount = orden.TotalAmount,
                Currency = "USD",
                PaymentMethod = "PayPal"
            });

            // Actualizar el estado de la orden
            orden.Status = nuevoEstado;
            _context.Orders.Update(orden);
            await _context.SaveChangesAsync();

            if (!capture.Success)
            {
                return StatusCode(CodigosDeEstadoHttp.SOLICITUD_INCORRECTA, new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = CodigosDeEstadoHttp.SOLICITUD_INCORRECTA,
                    Message = "El pago fue rechazado por PayPal."
                });
            }

            return Ok(new ResponseDto<object>
            {
                Status = true,
                StatusCode = CodigosDeEstadoHttp.OK,
                Message = "Pago completado exitosamente.",
                Data = new { orderId = orden.Id, status = nuevoEstado, captureId = capture.CaptureId }
            });
        }
    }
}