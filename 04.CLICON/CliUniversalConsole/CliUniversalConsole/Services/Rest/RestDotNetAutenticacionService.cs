using CliUniversalConsole.Models;
using CliUniversalConsole.Config;
using System.Net.Http.Json;
using System.Text.Json;

namespace CliUniversalConsole.Services.Rest
{
    public class RestDotNetAutenticacionService : IAutenticacionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = ServiceConfig.RestDotNetBaseUrl;

        public RestDotNetAutenticacionService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<LoginResult> LoginAsync(string usuario, string clave)
        {
            try
            {
                var request = new LoginRequest
                {
                    Usuario = usuario,
                    Clave = clave
                };

                var response = await _httpClient.PostAsJsonAsync($"{_baseUrl}/login", request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new LoginResult
                    {
                        IsSuccess = false,
                        Message = $"Error HTTP {response.StatusCode}: {response.ReasonPhrase}"
                    };
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(content, options);

                var result = new LoginResult();

                // Intentar con ambas convenciones de nombres (camelCase y PascalCase)
                if (jsonResponse.TryGetProperty("exitoso", out var exitoso) ||
                    jsonResponse.TryGetProperty("Exitoso", out exitoso))
                {
                    result.IsSuccess = exitoso.GetBoolean();
                }

                if (jsonResponse.TryGetProperty("mensaje", out var mensaje) ||
                    jsonResponse.TryGetProperty("Mensaje", out mensaje))
                {
                    result.Message = mensaje.GetString();
                }

                if (result.IsSuccess)
                {
                    // Buscar 'datos' o 'Datos'
                    JsonElement datos;
                    bool hasDatos = jsonResponse.TryGetProperty("datos", out datos) || 
                                   jsonResponse.TryGetProperty("Datos", out datos);
                    
                    if (hasDatos)
                    {
                        result.EmpleadoInfo = new Empleado
                        {
                            Codigo = TryGetStringProperty(datos, "codigo", "Codigo"),
                            Paterno = TryGetStringProperty(datos, "paterno", "Paterno"),
                            Materno = TryGetStringProperty(datos, "materno", "Materno"),
                            Nombre = TryGetStringProperty(datos, "nombre", "Nombre"),
                            NombreCompleto = TryGetStringProperty(datos, "nombreCompleto", "NombreCompleto"),
                            Ciudad = TryGetStringProperty(datos, "ciudad", "Ciudad"),
                            Direccion = TryGetStringProperty(datos, "direccion", "Direccion"),
                            Usuario = TryGetStringProperty(datos, "usuario", "Usuario")
                        };
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new LoginResult
                {
                    IsSuccess = false,
                    Message = $"Error al conectar con el servicio: {ex.Message}"
                };
            }
        }

        private string? TryGetStringProperty(JsonElement element, string camelCase, string pascalCase)
        {
            if (element.TryGetProperty(camelCase, out var value) ||
                element.TryGetProperty(pascalCase, out value))
            {
                return value.GetString();
            }
            return null;
        }
    }
}
