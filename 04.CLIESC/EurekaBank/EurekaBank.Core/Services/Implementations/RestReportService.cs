// En: EurekaBank.Core/Services/Implementations/RestReportService.cs
using EurekaBank.Core.Managers;
using EurekaBank.Core.Models.Responses;
using EurekaBank.Core.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EurekaBank.Core.Services.Implementations
{
    public class RestReportService : IReportService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private ApiPlatform _currentTarget = ApiPlatform.Java;

        public RestReportService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public void SetTarget(ApiPlatform target)
        {
            _currentTarget = target;
        }

        public async Task<IEnumerable<MovementDto>> ObtenerMovimientosAsync(string codigoCuenta)
        {
            var httpClient = _httpClientFactory.CreateClient();
            try
            {
                string hostKey = _currentTarget == ApiPlatform.Java ? "Hosts:Rest:Java" : "Hosts:Rest:DotNet";
                string? baseUrl = _configuration[hostKey];
                string? baseIp = _configuration["ServerConfig:BaseIp"];

                if (!string.IsNullOrWhiteSpace(baseUrl) && !string.IsNullOrWhiteSpace(baseIp))
                {
                    baseUrl = baseUrl.Replace("{IP}", baseIp);
                }

                if (string.IsNullOrEmpty(baseUrl))
                {
                    System.Diagnostics.Debug.WriteLine($"RestReportService: Base URL not configured for {hostKey}");
                    return Enumerable.Empty<MovementDto>();
                }

                var fullUrl = $"{baseUrl}/api/Reporte/movimientos/{codigoCuenta}";

                // Logging para debugging
                System.Diagnostics.Debug.WriteLine($"RestReportService: Calling {fullUrl}");

                if (_currentTarget == ApiPlatform.Java)
                {
                    // === MANEJO ESPECIAL PARA JAVA ===
                    var jsonResponse = await httpClient.GetStringAsync(fullUrl);
                    
                    System.Diagnostics.Debug.WriteLine($"RestReportService Java response length: {jsonResponse?.Length ?? 0} chars");
                    
                    // Preprocesar el JSON para corregir el formato de fechas
                    if (!string.IsNullOrEmpty(jsonResponse))
                    {
                        // Reemplazar el formato de fecha problemático 2022-01-09T05:00:00Z[UTC] por 2022-01-09T05:00:00Z
                        jsonResponse = System.Text.RegularExpressions.Regex.Replace(
                            jsonResponse, 
                            @"(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z)\[UTC\]", 
                            "$1");
                            
                        System.Diagnostics.Debug.WriteLine("RestReportService: Fixed date format in JSON");
                    }

                    // Configuración especial de Newtonsoft.Json para manejar fechas de Java
                    var settings = new JsonSerializerSettings
                    {
                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                        DateParseHandling = DateParseHandling.DateTime,
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Include,
                        Converters = { new IsoDateTimeConverter { DateTimeFormat = "yyyy-MM-ddTHH:mm:ssZ" } },
                        // Manejo de errores para campos problemáticos
                        Error = (sender, args) =>
                        {
                            System.Diagnostics.Debug.WriteLine($"JSON Error: {args.ErrorContext.Error.Message}");
                            System.Diagnostics.Debug.WriteLine($"JSON Path: {args.ErrorContext.Path}");
                            
                            // Simplemente marcar el error como manejado
                            args.ErrorContext.Handled = true;
                        }
                    };

                    var movements = JsonConvert.DeserializeObject<IEnumerable<MovementDto>>(jsonResponse ?? "", settings);
                    var result = movements ?? Enumerable.Empty<MovementDto>();
                    
                    System.Diagnostics.Debug.WriteLine($"RestReportService: Deserialized {result.Count()} movements");
                    
                    // Mostrar una muestra de los primeros movimientos para debugging
                    foreach (var mov in result.Take(3))
                    {
                        System.Diagnostics.Debug.WriteLine($"  Movement sample: {mov.NumeroMovimiento} - {mov.TipoMovimiento} - {mov.Accion} - {mov.Importe}");
                    }
                    
                    return result;
                }
                else
                {
                    // === MANEJO NORMAL PARA .NET ===
                    System.Diagnostics.Debug.WriteLine("RestReportService: Using .NET endpoint");
                    var movements = await httpClient.GetFromJsonAsync<IEnumerable<MovementDto>>(fullUrl);
                    var result = movements ?? Enumerable.Empty<MovementDto>();
                    
                    System.Diagnostics.Debug.WriteLine($"RestReportService .NET: Retrieved {result.Count()} movements");
                    return result;
                }
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"RestReportService HTTP Error: {httpEx.Message}");
                return Enumerable.Empty<MovementDto>();
            }
            catch (JsonException jsonEx)
            {
                System.Diagnostics.Debug.WriteLine($"RestReportService JSON Error: {jsonEx.Message}");
                return Enumerable.Empty<MovementDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RestReportService Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return Enumerable.Empty<MovementDto>();
            }
        }
    }
}