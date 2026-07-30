
namespace techstore_api.Dtos.Payments
{
    // <summary>Resultado de crear una orden en PayPal.</summary>
    public class PayPalResults
    {
        public bool Success { get; set; }
        public string? PaypalOrderId { get; set; }
        public string? Status { get; set; }
        public string? RawResponse { get; set; }
    }

    /// <summary>Resultado de capturar (cobrar) una orden en PayPal.</summary>
    public class PayPalCaptureResult
    {
        public bool Success { get; set; }
        public string? Status { get; set; }
        public string? CaptureId { get; set; }
        public string? RawResponse { get; set; }
    }
}