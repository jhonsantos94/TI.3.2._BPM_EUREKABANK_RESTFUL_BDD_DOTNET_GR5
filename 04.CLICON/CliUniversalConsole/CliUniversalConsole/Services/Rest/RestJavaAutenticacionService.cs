using CliUniversalConsole.Models;
using CliUniversalConsole.Config;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace CliUniversalConsole.Services.Rest
{
    public class RestJavaAutenticacionService : IAutenticacionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = ServiceConfig.RestJavaBaseUrl;

        public RestJavaAutenticacionService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
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


                // Configurar opciones de serialización para que use PascalCase (Java espera Usuario y Clave con mayúscula)
                var serializerOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = null // null usa PascalCase por defecto en C#
                };

                var json = JsonSerializer.Serialize(request, serializerOptions);
                

                var httpContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/login", httpContent);
                var content = await response.Content.ReadAsStringAsync();


                // Verificar si la respuesta es exitosa
                if (!response.IsSuccessStatusCode)
                {
                    return new LoginResult
                    {
                        IsSuccess = false,
                        Message = $"Error HTTP {response.StatusCode}: {response.ReasonPhrase}"
                    };
                }

                // Verificar si la respuesta es XML en lugar de JSON
                if (content.TrimStart().StartsWith("<"))
                {
                    return new LoginResult
                    {
                        IsSuccess = false,
                        Message = "El servicio REST Java está devolviendo XML en lugar de JSON. Verifica la configuración del servidor."
                    };
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var jsonResponse = JsonSerializer.Deserialize<JsonElement>(content, options);

                var result = new LoginResult();

                if (jsonResponse.TryGetProperty("Exitoso", out var exitoso) ||
                    jsonResponse.TryGetProperty("exitoso", out exitoso))
                {
                    result.IsSuccess = exitoso.GetBoolean();
                }

                if (jsonResponse.TryGetProperty("Mensaje", out var mensaje) ||
                    jsonResponse.TryGetProperty("mensaje", out mensaje))
                {
                    result.Message = mensaje.GetString();
                }

                if (result.IsSuccess && jsonResponse.TryGetProperty("Datos", out var datos))
                {
                    result.EmpleadoInfo = new Empleado
                    {
                        Codigo = datos.TryGetProperty("Codigo", out var codigo) ? codigo.GetString() : null,
                        Paterno = datos.TryGetProperty("Paterno", out var paterno) ? paterno.GetString() : null,
                        Materno = datos.TryGetProperty("Materno", out var materno) ? materno.GetString() : null,
                        Nombre = datos.TryGetProperty("Nombre", out var nombre) ? nombre.GetString() : null,
                        NombreCompleto = datos.TryGetProperty("NombreCompleto", out var nombreCompleto) ? nombreCompleto.GetString() : null,
                        Ciudad = datos.TryGetProperty("Ciudad", out var ciudad) ? ciudad.GetString() : null,
                        Direccion = datos.TryGetProperty("Direccion", out var direccion) ? direccion.GetString() : null,
                        Usuario = datos.TryGetProperty("Usuario", out var usuarioData) ? usuarioData.GetString() : null
                    };
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
    }
}
