using AutoMapper;
using Microsoft.EntityFrameworkCore;
using techstore_api.DataBase;
using techstore_api.DataBase.Entities;
using techstore_api.Dtos.Orders;
using techstore_api.Services.Interfaces;
using TechStoreApi.Dtos.Common;
using HttpStatusCode = TechStoreApi.Constants.CodigosDeEstadoHttp;

namespace techstore_api.Services
{
    public class OrderService : IOrderService
    {
        private readonly TiendaDbContext _contexto;
        private readonly IMapper _mapper;

        public OrderService(TiendaDbContext context, IMapper mapper)
        {
            _contexto = context;
            _mapper = mapper;
        }

        public async Task<ResponseDto<List<OrderDto>>> GetListAsync(string searchTerm = "", int page = 1, int pageSize = 0)
        {
            var consulta = _contexto.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchTerm))
            {
                consulta = consulta.Where(o => o.UserId.Contains(searchTerm) || o.Status.Contains(searchTerm));
            }

            var ordenes = await consulta.ToListAsync();
            var ordenesDto = _mapper.Map<List<OrderDto>>(ordenes);

            return new ResponseDto<List<OrderDto>>
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Data = ordenesDto
            };
        }

        public async Task<ResponseDto<OrderDto>> GetOneByIdAsync(int id)
        {
            var orden = await _contexto.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orden == null)
            {
                return new ResponseDto<OrderDto>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.NO_ENCONTRADO,
                    Message = "Orden no encontrada."
                };
            }

            var ordenDto = _mapper.Map<OrderDto>(orden);

            return new ResponseDto<OrderDto>
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Data = ordenDto
            };
        }

        public async Task<ResponseDto<object>> CreateAsync(OrderCreateDto dto)
        {
            // Validar que la orden tenga al menos un producto (carrito multi-ítem)
            if (dto.OrderDetails == null || dto.OrderDetails.Count == 0)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                    Message = "La orden debe incluir al menos un producto."
                };
            }

            decimal total = 0;
            foreach (var detalleDto in dto.OrderDetails)
            {
                if (!detalleDto.ProductId.HasValue)
                {
                    return new ResponseDto<object>
                    {
                        Status = false,
                        StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                        Message = "Cada detalle debe especificar un producto."
                    };
                }

                if (detalleDto.Quantity < 1)
                {
                    return new ResponseDto<object>
                    {
                        Status = false,
                        StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                        Message = "La cantidad de cada producto debe ser al menos 1."
                    };
                }

                var producto = await _contexto.Products.FindAsync(detalleDto.ProductId.Value);
                if (producto == null)
                {
                    return new ResponseDto<object>
                    {
                        Status = false,
                        StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                        Message = $"Producto con ID {detalleDto.ProductId.Value} no encontrado."
                    };
                }

                if (producto.Stock < detalleDto.Quantity)
                {
                    return new ResponseDto<object>
                    {
                        Status = false,
                        StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                        Message = $"Stock insuficiente para el producto {producto.Name}."
                    };
                }

                producto.Stock -= detalleDto.Quantity;
                _contexto.Products.Update(producto);

                // Forzar el precio unitario desde el producto y acumular el total
                detalleDto.UnitPrice = producto.Price;
                total += producto.Price * detalleDto.Quantity;
            }

            dto.TotalAmount = total;
            dto.Status = "PENDIENTE";

            var orden = _mapper.Map<OrderEntity>(dto);
            await _contexto.Orders.AddAsync(orden);
            await _contexto.SaveChangesAsync();

            return new ResponseDto<object>
            {
                Status = true,
                StatusCode = HttpStatusCode.CREADO,
                Message = "Orden creada exitosamente."
            };
        }

        public async Task<ResponseDto<object>> EditAsync(OrderEditDto dto, int id)
        {
            var orden = await _contexto.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orden == null)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.NO_ENCONTRADO,
                    Message = "Orden no encontrada."
                };
            }

            // Los administradores pueden modificar órdenes en cualquier estado
            // (completadas, canceladas, etc.)

            // Eliminar detalles antiguos no presentes en el nuevo DTO
            foreach (var detalleExistente in orden.OrderDetails.ToList())
            {
                if (!dto.OrderDetails.Any(d => d.Id == detalleExistente.Id))
                {
                    _contexto.OrderDetails.Remove(detalleExistente);
                }
            }

            // Añadir o actualizar detalles nuevos/existentes
            foreach (var detalleDto in dto.OrderDetails)
            {
                var detalleExistente = orden.OrderDetails.FirstOrDefault(d => d.Id == detalleDto.Id);
                if (detalleExistente == null) // Nuevo detalle
                {
                    if (detalleDto.ProductId.HasValue)
                    {
                        var producto = await _contexto.Products.FindAsync(detalleDto.ProductId.Value);
                        if (producto == null)
                        {
                            return new ResponseDto<object>
                            {
                                Status = false,
                                StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                                Message = $"Producto con ID {detalleDto.ProductId.Value} no encontrado."
                            };
                        }
                        if (producto.Stock < detalleDto.Quantity)
                        {
                            return new ResponseDto<object>
                            {
                                Status = false,
                                StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                                Message = $"Stock insuficiente para el producto {producto.Name}."
                            };
                        }
                        producto.Stock -= detalleDto.Quantity;
                        _contexto.Products.Update(producto);
                    }

                    orden.OrderDetails.Add(_mapper.Map<OrderDetailEntity>(detalleDto));
                }
                else // Detalle existente
                {
                    // Revertir cambios de stock para la cantidad antigua antes de aplicar la nueva cantidad
                    if (detalleExistente.ProductId.HasValue) {
                        var producto = await _contexto.Products.FindAsync(detalleExistente.ProductId.Value);
                        if (producto != null) {
                            producto.Stock += detalleExistente.Quantity;
                            _contexto.Products.Update(producto);
                        }
                    }

                    _mapper.Map(detalleDto, detalleExistente);
                    
                    // Aplicar nuevos cambios de stock
                    if (detalleExistente.ProductId.HasValue)
                    {
                        var producto = await _contexto.Products.FindAsync(detalleExistente.ProductId.Value);
                        if (producto != null)
                        {
                             if (producto.Stock < detalleDto.Quantity)
                            {
                                return new ResponseDto<object>
                                {
                                    Status = false,
                                    StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                                    Message = $"Stock insuficiente para el producto {producto.Name}."
                                };
                            }
                            producto.Stock -= detalleDto.Quantity;
                            _contexto.Products.Update(producto);
                        }
                    }

                    _contexto.OrderDetails.Update(detalleExistente);
                }
            }

            _mapper.Map(dto, orden);
            _contexto.Orders.Update(orden);
            await _contexto.SaveChangesAsync();

            return new ResponseDto<object>
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Orden actualizada exitosamente."
            };
        }

        public async Task<ResponseDto<object>> UpdateStatusAsync(OrderStatusUpdateDto dto, int id)
        {
            var orden = await _contexto.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orden == null)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.NO_ENCONTRADO,
                    Message = "Orden no encontrada."
                };
            }

            // Verificar que la orden no esté completada
            if (orden.Status.ToUpper() == "COMPLETADA")
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                    Message = "No se puede modificar una orden que ya está completada."
                };
            }

            // Verificar que la orden no esté cancelada
            if (orden.Status.ToUpper() == "CANCELADA")
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.SOLICITUD_INCORRECTA,
                    Message = "No se puede modificar una orden que ya está cancelada."
                };
            }

            // Si el estado se cambia a "CANCELADA", restaurar el stock
            if (dto.Status.ToUpper() == "CANCELADA" && orden.Status.ToUpper() != "CANCELADA")
            {
                foreach (var detalle in orden.OrderDetails)
                {
                    if (detalle.ProductId.HasValue)
                    {
                        var producto = await _contexto.Products.FindAsync(detalle.ProductId.Value);
                        if (producto != null)
                        {
                            producto.Stock += detalle.Quantity;
                            _contexto.Products.Update(producto);
                        }
                    }
                }
            }

            orden.Status = dto.Status;
            _contexto.Orders.Update(orden);
            await _contexto.SaveChangesAsync();

            return new ResponseDto<object>
            {
                Status = true,
                StatusCode = HttpStatusCode.OK,
                Message = "Estado de la orden actualizado exitosamente."
            };
        }

        public async Task<ResponseDto<object>> DeleteAsync(int id)
        {
            var orden = await _contexto.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (orden == null)
            {
                return new ResponseDto<object>
                {
                    Status = false,
                    StatusCode = HttpStatusCode.NO_ENCONTRADO,
                    Message = "Orden no encontrada."
                };
            }

            // Restaurar stock para productos en los detalles de la orden antes de eliminar
            foreach (var detalle in orden.OrderDetails)
            {
                if (detalle.ProductId.HasValue)
                {
                    var producto = await _contexto.Products.FindAsync(detalle.ProductId.Value);
                    if (producto != null)
                    {
                        producto.Stock += detalle.Quantity;
                        _contexto.Products.Update(producto);
                    }
                }
            }

            _contexto.Orders.Remove(orden);
            await _contexto.SaveChangesAsync();

            return new ResponseDto<object>
            {
                Status = true,
                StatusCode = HttpStatusCode.SIN_CONTENIDO,
                Message = "Orden eliminada exitosamente."
            };
        }
    }
}