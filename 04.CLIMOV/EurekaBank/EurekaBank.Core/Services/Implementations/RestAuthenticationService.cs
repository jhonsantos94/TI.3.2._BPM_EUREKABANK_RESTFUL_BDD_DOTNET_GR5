// En: EurekaBank.Core/Services/Implementations/RestAuthenticationService.cs
using EurekaBank.Core.Managers;
using EurekaBank.Core.Models.Requests;
using EurekaBank.Core.Models.Responses;
using EurekaBank.Core.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;

namespace EurekaBank.Core.Services.Implementations
{
    public class RestAuthenticationService : IAuthenticationService
    {
        // Almacenamos la fábrica, no un cliente único
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private ApiPlatform _currentTarget = ApiPlatform.Java;
        public void SetTarget(ApiPlatform target) // Asegúrate de que sea public
        {
            _currentTarget = target;
        }
        // El constructor ahora pide un IHttpClientFactory además de la configuración
        public RestAuthenticationService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var httpClient = _httpClientFactory.CreateClient();
            HttpResponseMessage? response = null;
            string fullUrl = string.Empty; // Variable para depuración

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
                    return new LoginResponse
                    {
                        Exitoso = false,
                        Mensaje = $"La URL base para '{hostKey}' no está configurada en appsettings.json."
                    };
                }

                fullUrl = $"{baseUrl}/api/Autenticacion/login";
                // Para depurar, puedes imprimir la URL
                // System.Diagnostics.Debug.WriteLine($"Intentando conectar a: {fullUrl}");

                // ===== FIN DE LA DEPURACIÓN Y VALIDACIÓN =====

                var jsonRequest = JsonConvert.SerializeObject(request);
                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

                response = await httpClient.PostAsync(fullUrl, content);

                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(jsonResponse))
                {
                    return new LoginResponse
                    {
                        Exitoso = false,
                        Mensaje = $"El servidor respondió con un error pero sin detalles: {(int)response.StatusCode} ({response.ReasonPhrase})"
                    };
                }

                var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(jsonResponse);
                return loginResponse!;
            }
            catch (HttpRequestException httpEx) // Capturamos errores de red específicamente
            {
                return new LoginResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error de conexión al intentar acceder a '{fullUrl}'. Detalles: {httpEx.Message}"
                };
            }
            catch (JsonException jsonEx)
            {
                string responseContent = response != null ? await response.Content.ReadAsStringAsync() : "La respuesta fue nula.";
                return new LoginResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error al procesar el JSON de la respuesta: {jsonEx.Message}. Contenido: {responseContent}"
                };
            }
            catch (Exception ex)
            {
                // Este bloque ahora capturará otros errores inesperados
                return new LoginResponse
                {
                    Exitoso = false,
                    Mensaje = $"Error inesperado: {ex.GetType().Name} - {ex.Message}"
                };
            }
        }
    }
}