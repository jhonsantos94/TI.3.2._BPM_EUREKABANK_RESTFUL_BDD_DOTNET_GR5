using CliUniversalConsole.Config;
using CliUniversalConsole.Models;
using System.Text;
using System.Text.Json;

namespace CliUniversalConsole.Services.Rest
{
    public class RestJavaTransaccionService : ITransaccionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public RestJavaTransaccionService()
        {
            _httpClient = new HttpClient();
            _baseUrl = ServiceConfig.RestJavaTransaccionUrl;
        }

        public async Task<TransaccionResult> RealizarDepositoAsync(TransaccionRequest request)
        {
            try
            {
                // Java REST espera PascalCase
                var jsonRequest = JsonSerializer.Serialize(new
                {
                    CodigoCuenta = request.CodigoCuenta,
                    ClaveCuenta = request.ClaveCuenta,
                    Importe = request.Importe,
                    CodigoEmpleado = request.CodigoEmpleado
                });

                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/deposito", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                // Log para debug (opcional - puedes comentar después)
                // Console.WriteLine($"\nRespuesta del servidor: {responseBody}");

                return ParseDepositoResponse(responseBody);
            }
            catch (Exception ex)
            {
                return new TransaccionResult
                {
                    IsSuccess = false,
                    Message = $"Error en depósito: {ex.Message}"
                };
            }
        }

        public async Task<TransaccionResult> RealizarRetiroAsync(TransaccionRequest request)
        {
            try
            {
                // Java REST espera PascalCase
                var jsonRequest = JsonSerializer.Serialize(new
                {
                    CodigoCuenta = request.CodigoCuenta,
                    ClaveCuenta = request.ClaveCuenta,
                    Importe = request.Importe,
                    CodigoEmpleado = request.CodigoEmpleado
                });

                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/retiro", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                return ParseRetiroResponse(responseBody);
            }
            catch (Exception ex)
            {
                return new TransaccionResult
                {
                    IsSuccess = false,
                    Message = $"Error en retiro: {ex.Message}"
                };
            }
        }

        public async Task<TransaccionResult> RealizarTransferenciaAsync(TransferenciaRequest request)
        {
            try
            {
                // Java REST espera PascalCase
                var jsonRequest = JsonSerializer.Serialize(new
                {
                    CuentaOrigen = request.CuentaOrigen,
                    ClaveCuentaOrigen = request.ClaveCuentaOrigen,
                    CuentaDestino = request.CuentaDestino,
                    Importe = request.Importe,
                    CodigoEmpleado = request.CodigoEmpleado
                });

                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/transferencia", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                return ParseTransferenciaResponse(responseBody);
            }
            catch (Exception ex)
            {
                return new TransaccionResult
                {
                    IsSuccess = false,
                    Message = $"Error en transferencia: {ex.Message}"
                };
            }
        }

        private TransaccionResult ParseDepositoResponse(string jsonResponse)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                // Obtener 'exitoso'
                bool exitoso = TryGetBooleanProperty(root, "exitoso", "Exitoso");

                // Obtener 'mensaje'
                string mensaje = TryGetStringProperty(root, "mensaje", "Mensaje") ?? "";

                // Si no es exitoso, retornar el error
                if (!exitoso)
                {
                    // Verificar si hay codigoError
                    string codigoError = TryGetStringProperty(root, "codigoError", "CodigoError") ?? "";
                    if (!string.IsNullOrEmpty(codigoError))
                    {
                        mensaje = $"{mensaje} (Código: {codigoError})";
                    }
                    return new TransaccionResult { IsSuccess = false, Message = mensaje };
                }

                // Intentar obtener 'datos' - puede ser null si hay error
                JsonElement datos;
                if (!root.TryGetProperty("datos", out datos) && !root.TryGetProperty("Datos", out datos))
                {
                    return new TransaccionResult { IsSuccess = false, Message = "No se encontraron datos en la respuesta" };
                }

                // Verificar si datos es null
                if (datos.ValueKind == JsonValueKind.Null)
                {
                    return new TransaccionResult { IsSuccess = false, Message = mensaje };
                }

                var depositoResult = new DepositoResult
                {
                    CodigoCuenta = TryGetStringProperty(datos, "codigoCuenta", "CodigoCuenta") ?? "",
                    ImporteDepositado = TryGetDecimalProperty(datos, "importe", "Importe"),
                    SaldoAnterior = TryGetDecimalProperty(datos, "saldoAnterior", "SaldoAnterior"),
                    SaldoNuevo = TryGetDecimalProperty(datos, "saldoNuevo", "SaldoNuevo"),
                    NumeroMovimiento = TryGetInt32Property(datos, "numeroMovimiento", "NumeroMovimiento")
                };

                return new TransaccionResult { IsSuccess = true, Message = mensaje, Data = depositoResult };
            }
            catch (Exception ex)
            {
                return new TransaccionResult { IsSuccess = false, Message = $"Error al procesar respuesta: {ex.Message}\nRespuesta: {jsonResponse}" };
            }
        }

        private TransaccionResult ParseRetiroResponse(string jsonResponse)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                // Obtener 'exitoso'
                bool exitoso = TryGetBooleanProperty(root, "exitoso", "Exitoso");

                // Obtener 'mensaje'
                string mensaje = TryGetStringProperty(root, "mensaje", "Mensaje") ?? "";

                // Si no es exitoso, retornar el error
                if (!exitoso)
                {
                    string codigoError = TryGetStringProperty(root, "codigoError", "CodigoError") ?? "";
                    if (!string.IsNullOrEmpty(codigoError))
                    {
                        mensaje = $"{mensaje} (Código: {codigoError})";
                    }
                    return new TransaccionResult { IsSuccess = false, Message = mensaje };
                }

                // Intentar obtener 'datos'
                JsonElement datos;
                if (!root.TryGetProperty("datos", out datos) && !root.TryGetProperty("Datos", out datos))
                {
                    return new TransaccionResult { IsSuccess = false, Message = "No se encontraron datos en la respuesta" };
                }

                // Verificar si datos es null
                if (datos.ValueKind == JsonValueKind.Null)
                {
                    return new TransaccionResult { IsSuccess = false, Message = mensaje };
                }

                var retiroResult = new RetiroResult
                {
                    CodigoCuenta = TryGetStringProperty(datos, "codigoCuenta", "CodigoCuenta") ?? "",
                    ImporteRetirado = TryGetDecimalProperty(datos, "importeRetirado", "ImporteRetirado"),
                    ImporteITF = TryGetDecimalProperty(datos, "importeITF", "ImporteITF"),
                    ImporteCargo = TryGetDecimalProperty(datos, "importeCargo", "ImporteCargo"),
                    TotalDescontado = TryGetDecimalProperty(datos, "totalDescontado", "TotalDescontado"),
                    SaldoAnterior = TryGetDecimalProperty(datos, "saldoAnterior", "SaldoAnterior"),
                    SaldoNuevo = TryGetDecimalProperty(datos, "saldoNuevo", "SaldoNuevo"),
                    NumeroMovimientoRetiro = TryGetInt32Property(datos, "numeroMovimientoRetiro", "NumeroMovimientoRetiro"),
                    NumeroMovimientoITF = TryGetNullableInt32Property(datos, "numeroMovimientoITF", "NumeroMovimientoITF"),
                    NumeroMovimientoCargo = TryGetNullableInt32Property(datos, "numeroMovimientoCargo", "NumeroMovimientoCargo")
                };

                return new TransaccionResult { IsSuccess = true, Message = mensaje, Data = retiroResult };
            }
            catch (Exception ex)
            {
                return new TransaccionResult { IsSuccess = false, Message = $"Error al procesar respuesta: {ex.Message}\nRespuesta: {jsonResponse}" };
            }
        }

        private TransaccionResult ParseTransferenciaResponse(string jsonResponse)
        {
            try
            {
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                // Obtener 'exitoso'
                bool exitoso = TryGetBooleanProperty(root, "exitoso", "Exitoso");

                // Obtener 'mensaje'
                string mensaje = TryGetStringProperty(root, "mensaje", "Mensaje") ?? "";

                // Si no es exitoso, retornar el error
                if (!exitoso)
                {
                    string codigoError = TryGetStringProperty(root, "codigoError", "CodigoError") ?? "";
                    if (!string.IsNullOrEmpty(codigoError))
                    {
                        mensaje = $"{mensaje} (Código: {codigoError})";
                    }
                    return new TransaccionResult { IsSuccess = false, Message = mensaje };
                }

                // Intentar obtener 'datos'
                JsonElement datos;
                if (!root.TryGetProperty("datos", out datos) && !root.TryGetProperty("Datos", out datos))
                {
                    return new TransaccionResult { IsSuccess = false, Message = "No se encontraron datos en la respuesta" };
                }

                // Verificar si datos es null
                if (datos.ValueKind == JsonValueKind.Null)
                {
                    return new TransaccionResult { IsSuccess = false, Message = mensaje };
                }

                // Para transferencia, CuentaOrigen y CuentaDestino son objetos con Codigo, SaldoAnterior, SaldoNuevo
                JsonElement cuentaOrigen, cuentaDestino;
                
                // Obtener objetos de cuentas
                if (!datos.TryGetProperty("CuentaOrigen", out cuentaOrigen) && 
                    !datos.TryGetProperty("cuentaOrigen", out cuentaOrigen))
                {
                    return new TransaccionResult { IsSuccess = false, Message = "No se encontró CuentaOrigen en la respuesta" };
                }

                if (!datos.TryGetProperty("CuentaDestino", out cuentaDestino) && 
                    !datos.TryGetProperty("cuentaDestino", out cuentaDestino))
                {
                    return new TransaccionResult { IsSuccess = false, Message = "No se encontró CuentaDestino en la respuesta" };
                }

                var transferenciaResult = new TransferenciaResult
                {
                    CuentaOrigen = TryGetStringProperty(cuentaOrigen, "codigo", "Codigo") ?? "",
                    CuentaDestino = TryGetStringProperty(cuentaDestino, "codigo", "Codigo") ?? "",
                    ImporteTransferido = TryGetDecimalProperty(datos, "importeTransferido", "ImporteTransferido"),
                    SaldoAnteriorOrigen = TryGetDecimalProperty(cuentaOrigen, "saldoAnterior", "SaldoAnterior"),
                    SaldoNuevoOrigen = TryGetDecimalProperty(cuentaOrigen, "saldoNuevo", "SaldoNuevo"),
                    SaldoAnteriorDestino = TryGetDecimalProperty(cuentaDestino, "saldoAnterior", "SaldoAnterior"),
                    SaldoNuevoDestino = TryGetDecimalProperty(cuentaDestino, "saldoNuevo", "SaldoNuevo"),
                    NumeroMovimientoOrigen = TryGetInt32Property(datos, "numeroMovimientoOrigen", "NumeroMovimientoOrigen"),
                    NumeroMovimientoDestino = TryGetInt32Property(datos, "numeroMovimientoDestino", "NumeroMovimientoDestino")
                };

                return new TransaccionResult { IsSuccess = true, Message = mensaje, Data = transferenciaResult };
            }
            catch (Exception ex)
            {
                return new TransaccionResult { IsSuccess = false, Message = $"Error al procesar respuesta: {ex.Message}\nRespuesta: {jsonResponse}" };
            }
        }

        // Métodos helper para manejar ambas convenciones de nombres
        private bool TryGetBooleanProperty(JsonElement element, string camelCase, string pascalCase)
        {
            if (element.TryGetProperty(camelCase, out var value))
            {
                return value.GetBoolean();
            }
            else if (element.TryGetProperty(pascalCase, out value))
            {
                return value.GetBoolean();
            }
            return false;
        }

        private string? TryGetStringProperty(JsonElement element, string camelCase, string pascalCase)
        {
            if (element.TryGetProperty(camelCase, out var value))
            {
                return value.ValueKind == JsonValueKind.Null ? null : value.GetString();
            }
            else if (element.TryGetProperty(pascalCase, out value))
            {
                return value.ValueKind == JsonValueKind.Null ? null : value.GetString();
            }
            return null;
        }

        private decimal TryGetDecimalProperty(JsonElement element, string camelCase, string pascalCase)
        {
            if (element.TryGetProperty(camelCase, out var value))
            {
                if (value.ValueKind == JsonValueKind.Number)
                    return value.GetDecimal();
            }
            else if (element.TryGetProperty(pascalCase, out value))
            {
                if (value.ValueKind == JsonValueKind.Number)
                    return value.GetDecimal();
            }
            return 0;
        }

        private int TryGetInt32Property(JsonElement element, string camelCase, string pascalCase)
        {
            if (element.TryGetProperty(camelCase, out var value))
            {
                if (value.ValueKind == JsonValueKind.Number)
                    return value.GetInt32();
            }
            else if (element.TryGetProperty(pascalCase, out value))
            {
                if (value.ValueKind == JsonValueKind.Number)
                    return value.GetInt32();
            }
            return 0;
        }

        private int? TryGetNullableInt32Property(JsonElement element, string camelCase, string pascalCase)
        {
            if (element.TryGetProperty(camelCase, out var value))
            {
                if (value.ValueKind == JsonValueKind.Number)
                    return value.GetInt32();
            }
            else if (element.TryGetProperty(pascalCase, out value))
            {
                if (value.ValueKind == JsonValueKind.Number)
                    return value.GetInt32();
            }
            return null;
        }
    }
}
