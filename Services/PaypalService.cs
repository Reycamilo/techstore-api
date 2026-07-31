using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using techstore_api.Dtos.Payments;
using techstore_api.Services.Interfaces;


namespace techstore_api.Services
{
    /// <summary>
    /// Integración con la API REST de PayPal (Orders v2). Usa credenciales de Sandbox.
    /// </summary>
    public class PaypalService(HttpClient http, IConfiguration config) : IPaypalService
    {
        private readonly HttpClient _http = http;
        private readonly IConfiguration _config = config;

        private string BaseUrl => _config["PayPal:BaseUrl"] ?? "https://api-m.sandbox.paypal.com";
        private string ClientId => _config["PayPal:ClientId"] ?? string.Empty;
        private string Secret => _config["PayPal:Secret"] ?? string.Empty;

        private async Task<string?> GetAccessTokenAsync()
        {
            var basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ClientId}:{Secret}"));
            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v1/oauth2/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);
            request.Content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode) return null;

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString();
        }

        public async Task<PayPalCreateResults> CreateOrderAsync(decimal amount, string currency, string referenceId)
        {
            var token = await GetAccessTokenAsync();
            if (token == null)
            {
                return new PayPalCreateResults { Success = false, RawResponse = "No se pudo obtener el token de PayPal (¿credenciales?)." };
            }

            var body = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        reference_id = referenceId,
                        amount = new
                        {
                            currency_code = currency,
                            value = amount.ToString("F2", CultureInfo.InvariantCulture)
                        }
                    }
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v2/checkout/orders");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return new PayPalCreateResults { Success = false, RawResponse = json };
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            return new PayPalCreateResults
            {
                Success = true,
                PaypalOrderId = root.GetProperty("id").GetString(),
                Status = root.TryGetProperty("status", out var st) ? st.GetString() : null,
                RawResponse = json
            };
        }

        public async Task<PayPalCaptureResult> CaptureOrderAsync(string paypalOrderId)
        {
            var token = await GetAccessTokenAsync();
            if (token == null)
            {
                return new PayPalCaptureResult { Success = false, RawResponse = "No se pudo obtener el token de PayPal (¿credenciales?)." };
            }

            var request = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/v2/checkout/orders/{paypalOrderId}/capture");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

            var response = await _http.SendAsync(request);
            var json = await response.Content.ReadAsStringAsync();
            var result = new PayPalCaptureResult { RawResponse = json };

            if (!response.IsSuccessStatusCode)
            {
                result.Success = false;
                return result;
            }

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            result.Status = root.TryGetProperty("status", out var st) ? st.GetString() : null;
            result.Success = result.Status == "COMPLETED";

            // Extraer el id de la captura (dentro de purchase_units[0].payments.captures[0].id)
            try
            {
                result.CaptureId = root
                    .GetProperty("purchase_units")[0]
                    .GetProperty("payments")
                    .GetProperty("captures")[0]
                    .GetProperty("id")
                    .GetString();
            }
            catch
            {
                // si no viene, se deja null
            }

            return result;
        }
    }
}