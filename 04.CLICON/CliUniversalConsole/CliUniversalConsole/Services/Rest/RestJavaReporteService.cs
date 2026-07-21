using CliUniversalConsole.Config;
using CliUniversalConsole.Models;
using System.Text.Json;

namespace CliUniversalConsole.Services.Rest
{
    public class RestJavaReporteService : IReporteService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public RestJavaReporteService()
        {
            _httpClient = new HttpClient();
            _baseUrl = ServiceConfig.RestJavaReporteUrl;
        }

        public async Task<List<MovimientoDetalle>> ObtenerMovimientosAsync(string codigoCuenta)
        {
            try
            {
                var url = $"{_baseUrl}/movimientos/{codigoCuenta}";
                var response = await _httpClient.GetAsync(url);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n❌ Error al obtener movimientos");
                    Console.ResetColor();
                    return new List<MovimientoDetalle>();
                }

                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                var movimientos = new List<MovimientoDetalle>();

                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in root.EnumerateArray())
                    {
                        var movimiento = new MovimientoDetalle
                        {
                            CodigoCuenta = item.TryGetProperty("CodigoCuenta", out var cc) ? cc.GetString() ?? "" : "",
                            NumeroMovimiento = item.TryGetProperty("NumeroMovimiento", out var num) ? num.GetInt32() : 0,
                            Fecha = item.TryGetProperty("Fecha", out var fecha) ? DateTime.Parse((fecha.GetString() ?? "").Replace("[UTC]", "")) : DateTime.Now,
                            TipoMovimiento = item.TryGetProperty("TipoMovimiento", out var tipo) ? tipo.GetString() ?? "" : "",
                            EmpleadoNombre = item.TryGetProperty("EmpleadoNombre", out var emp) ? emp.GetString() ?? "" : "",
                            CuentaReferencia = item.TryGetProperty("CuentaReferencia", out var ref1) ? ref1.GetString() : null,
                            Importe = item.TryGetProperty("Importe", out var imp) ? imp.GetDecimal() : 0
                        };
                        movimientos.Add(movimiento);
                    }
                }

                return movimientos;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Console.ResetColor();
                return new List<MovimientoDetalle>();
            }
        }
    }
}
