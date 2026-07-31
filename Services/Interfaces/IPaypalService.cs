using techstore_api.Dtos.Payments;

namespace techstore_api.Services.Interfaces
{
    public interface IPaypalService
    {
        /// <summary>Crea una orden de pago en PayPal por el monto indicado.</summary>
        Task<PayPalCreateResults> CreateOrderAsync(decimal amount, string currency, string referenceId);

        /// <summary>Captura (cobra) una orden de PayPal previamente aprobada por el comprador.</summary>
        Task<PayPalCaptureResult> CaptureOrderAsync(string paypalOrderId);
    }
}