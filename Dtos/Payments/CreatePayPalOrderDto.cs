

using System.ComponentModel.DataAnnotations;

namespace techstore_api.Dtos.Payments
{
    /// <summary>
    /// Solicitud del frontend para crear una orden de pago en PayPal a partir de una orden local.
    /// </summary>
    public class CreatePayPalOrderDto
    {
        [Required]
        public int OrderId { get; set; }
    }
}