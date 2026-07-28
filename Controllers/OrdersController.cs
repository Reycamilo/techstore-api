using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using techstore_api.DataBase;
using techstore_api.Dtos.Orders;
using techstore_api.Services.Interfaces;
using TechStoreApi.Constants;
using TechStoreApi.Dtos.Common;

namespace techstore_api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly TiendaDbContext _context;

        public OrdersController(IOrderService orderService, TiendaDbContext context)
        {
            _orderService = orderService;
            _context = context;
        }

        /// <summary>
        /// Obtiene mis órdenes (para clientes, vendedores y técnicos)
        /// </summary>
        [HttpGet("my-orders")]
        [Authorize(Roles = "CLIENTE,VENDEDOR")]
        public async Task<ActionResult<ResponseDto<List<OrderDto>>>> GetMyOrders()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new ResponseDto<List<OrderDto>>
                {
                    Status = false,
                    StatusCode = 401,
                    Message = "Usuario no autenticado"
                });
            }

            var query = _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .AsQueryable();

            // Filtrar órdenes según el rol del usuario
            if (userRoles.Contains(NombresDeRoles.CLIENTE))
            {
                // Clientes ven solo sus propias órdenes
                query = query.Where(o => o.UserId == userId);
            }
            else if (userRoles.Contains(NombresDeRoles.VENDEDOR))
            {
                // Vendedores ven órdenes que contengan sus productos
                query = query.Where(o => o.OrderDetails.Any(od =>
                    od.ProductId.HasValue &&
                    od.Product != null &&
                    od.Product.SellerId == userId));
            }

            var orders = await query
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    OrderDetails = o.OrderDetails.Select(od => new OrderDetailDto
                    {
                        Id = od.Id,
                        OrderId = od.OrderId,
                        ProductId = od.ProductId,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        ProductName = od.Product != null ? od.Product.Name : null,
                        ProductSellerId = od.Product != null ? od.Product.SellerId : null
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new ResponseDto<List<OrderDto>>
            {
                Status = true,
                StatusCode = 200,
                Message = "Mis órdenes obtenidas exitosamente",
                Data = orders
            });
        }

        /// <summary>
        /// Obtiene todas las órdenes (solo para administradores)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<ActionResult<ResponseDto<List<OrderDto>>>> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    OrderDetails = o.OrderDetails.Select(od => new OrderDetailDto
                    {
                        Id = od.Id,
                        OrderId = od.OrderId,
                        ProductId = od.ProductId,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        ProductName = od.Product != null ? od.Product.Name : null,
                        ProductSellerId = od.Product != null ? od.Product.SellerId : null
                    }).ToList()
                })
                .ToListAsync();

            return Ok(new ResponseDto<List<OrderDto>>
            {
                Status = true,
                StatusCode = 200,
                Message = "Todas las órdenes obtenidas exitosamente",
                Data = orders
            });
        }

        /// <summary>
        /// Obtiene una orden por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ResponseDto<OrderDto>>> GetOrder(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .Where(o => o.Id == id)
                .Select(o => new OrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    TotalAmount = o.TotalAmount,
                    Status = o.Status,
                    OrderDetails = o.OrderDetails.Select(od => new OrderDetailDto
                    {
                        Id = od.Id,
                        OrderId = od.OrderId,
                        ProductId = od.ProductId,
                        Quantity = od.Quantity,
                        UnitPrice = od.UnitPrice,
                        ProductName = od.Product != null ? od.Product.Name : null,
                        ProductSellerId = od.Product != null ? od.Product.SellerId : null
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (order == null)
            {
                return NotFound(new ResponseDto<OrderDto>
                {
                    Status = false,
                    StatusCode = 404,
                    Message = "Orden no encontrada"
                });
            }

            // Verificar permisos: solo el propietario o administrador puede ver la orden
            if (!userRoles.Contains("ADMINISTRADOR") && order.UserId != userId)
            {
                return Forbid();
            }

            return Ok(new ResponseDto<OrderDto>
            {
                Status = true,
                StatusCode = 200,
                Message = "Orden obtenida exitosamente",
                Data = order
            });
        }

        /// <summary>
        /// Crea una nueva orden
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "CLIENTE")]
        public async Task<ActionResult<ResponseDto<OrderDto>>> CreateOrder([FromBody] OrderCreateDto orderDto)
        {
            var response = await _orderService.CreateAsync(orderDto);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Actualiza una orden existente (solo administradores)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<ActionResult<ResponseDto<OrderDto>>> UpdateOrder(int id, [FromBody] OrderEditDto orderDto)
        {
            var response = await _orderService.EditAsync(orderDto, id);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Actualiza solo el estado de una orden (para vendedores y técnicos)
        /// </summary>
        [HttpPut("{id}/status")]
        [Authorize(Roles = "VENDEDOR")]
        public async Task<ActionResult<ResponseDto<object>>> UpdateOrderStatus(int id, [FromBody] OrderStatusUpdateDto statusDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRoles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

            // Verificar que la orden pertenece al vendedor/técnico
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = 404,
                    Message = "Orden no encontrada"
                });
            }

            // Verificar que el vendedor/técnico tiene permisos sobre esta orden
            bool hasPermission = false;
            if (userRoles.Contains("VENDEDOR"))
            {
                // Vendedor solo puede editar órdenes que contengan sus productos
                hasPermission = order.OrderDetails.Any(od =>
                    od.ProductId.HasValue &&
                    od.Product != null &&
                    od.Product.SellerId == userId);
            }

            if (!hasPermission)
            {
                return Forbid();
            }

            var response = await _orderService.UpdateStatusAsync(statusDto, id);
            return StatusCode(response.StatusCode, response);
        }

        /// <summary>
        /// Elimina una orden
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMINISTRADOR")]
        public async Task<ActionResult> DeleteOrder(int id)
        {
            var response = await _orderService.DeleteAsync(id);
            if (response.StatusCode == 204)
            {
                return NoContent();
            }
            return StatusCode(response.StatusCode, response);
        }
    }
}