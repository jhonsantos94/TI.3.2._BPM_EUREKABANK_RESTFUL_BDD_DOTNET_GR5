using CliUniversalConsole.Config;
using CliUniversalConsole.Models;
using System.Text;
using System.Text.Json;

namespace CliUniversalConsole.Services.Rest
{
    public class RestDotNetTransaccionService : ITransaccionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public RestDotNetTransaccionService()
        {
            _httpClient = new HttpClient();
            _baseUrl = ServiceConfig.RestDotNetTransaccionUrl;
        }

        public async Task<TransaccionResult> RealizarDepositoAsync(TransaccionRequest request)
        {
            try
            {
                var jsonRequest = JsonSerializer.Serialize(new
                {
                    CodigoCuenta = request.CodigoCuenta,
                    ClaveCuenta = request.ClaveCuenta,
                    Importe = request.Importe,
                    CodigoEmpleado = request.CodigoEmpleado,
                    CodigoTipoMovimiento = "001"
                });

                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/deposito", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"DEBUG - Response Status: {response.StatusCode}");
                Console.WriteLine($"DEBUG - Response Body: {responseBody}");

                return ParseDepositoResponse(responseBody, request.CodigoCuenta);
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
                var jsonRequest = JsonSerializer.Serialize(new
                {
                    CodigoCuenta = request.CodigoCuenta,
                    ClaveCuenta = request.ClaveCuenta,
                    Importe = request.Importe,
                    CodigoEmpleado = request.CodigoEmpleado,
                    CodigoTipoMovimiento = "002"
                });

                var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync($"{_baseUrl}/retiro", content);
                var responseBody = await response.Content.ReadAsStringAsync();

                return ParseRetiroResponse(responseBody, request.CodigoCuenta);
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
                var jsonRequest = JsonSerializer.Serialize(new
                {
                    CuentaOrigen = request.CuentaOrigen,
                    ClaveCuentaOrigen = request.ClaveCuentaOrigen,
                    CuentaDestino = request.CuentaDestino,
                    Importe = request.Importe,
                    CodigoEmpleado = request.CodigoEmpleado,
                    CodigoTipoMovimiento = "009"
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

        private TransaccionResult ParseDepositoResponse(string jsonResponse, string codigoCuenta)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // .NET puede usar tanto PascalCase como camelCase dependiendo de la configuración
                bool exitoso = false;
                if (root.TryGetProperty("Exitoso", out var exitosoEl) || 
                    root.TryGetProperty("exitoso", out exitosoEl))
                {
                    exitoso = exitosoEl.GetBoolean();
                }

                string mensaje = "";
                if (root.TryGetProperty("Mensaje", out var mensajeEl) || 
                    root.TryGetProperty("mensaje", out mensajeEl))
                {
                    mensaje = mensajeEl.GetString() ?? "";
                }

                if (!exitoso)
                {
                    return new TransaccionResult { IsSuccess = false, Message = mensaje };
                }

                JsonElement datos;
                bool hasDatos = root.TryGetProperty("Datos", out datos) || 
                               root.TryGetProperty("datos", out datos);

                if (!hasDatos)
                {
                    return new TransaccionResult { IsSuccess = false, Message = "No se encontraron datos en la respuesta" };
                }

                var depositoResult = new DepositoResult
                {
                    CodigoCuenta = codigoCuenta,
                    ImporteDepositado = TryGetDecimal(datos, "Importe", "importe"),
                    SaldoAnterior = TryGetDecimal(datos, "SaldoAnterior", "saldoAnterior"),
                    SaldoNuevo = TryGetDecimal(datos, "SaldoNuevo", "saldoNuevo"),
                    NumeroMovimiento = TryGetInt32(datos, "NumeroMovimiento", "numeroMovimiento")
                };

                return new TransaccionResult { IsSuccess = true, Message = mensaje, Data = depositoResult };
            }
            catch (Exception ex)
            {
                return new TransaccionResult { IsSuccess = false, Message = $"Error al procesar respuesta: {ex.Message}" };
            }
        }

        private TransaccionResult ParseRetiroResponse(string jsonResponse, string codigoCuenta)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                bool exitoso = false;
                if (root.TryGetProperty("Exitoso", out var exitosoEl) || 
                    root.TryGetProperty("exitoso", out exitosoEl))
                {
                    exitoso = exitosoEl.GetBoolean();
                }

                string mensaje = "";
                if (root.TryGetProperty("Mensaje", out var mensajeEl) || 
                    root.TryGetProperty("mensaje", out mensajeEl))
                {
                    mensaje = mensajeEl.GetString() ?? "";
                }

                if (!exitoso)
                {
                    return new TransaccionResult { IsSuccess = false, Message = mensaje };
                }

                JsonElement datos;
                bool hasDatos = root.TryGetProperty("Datos", out datos) || 
                               root.TryGetProperty("datos", out datos);

                if (!hasDatos)
                {
                    return new TransaccionResult { IsSuccess = false, Message = "No se encontraron datos en la respuesta" };
                }

                var retiroResult = new RetiroResult
                {
                    CodigoCuenta = codigoCuenta,
                    ImporteRetirado = TryGetDecimal(datos, "ImporteRetiro", "importeRetiro"),
                    ImporteITF = TryGetDecimal(datos, "Itf", "itf"),
                    ImporteCargo = TryGetDecimal(datos, "CostoPorMovimiento", "costoPorMovimiento"),
                    TotalDescontado = TryGetDecimal(datos, "TotalDescontado", "totalDescontado"),
                    SaldoAnterior = TryGetDecimal(datos, "SaldoAnterior", "saldoAnterior"),
                    SaldoNuevo = TryGetDecimal(datos, "SaldoNuevo", "saldoNuevo"),
                    NumeroMovimientoRetiro = TryGetInt32(datos, "NumeroMovimientoRetiro", "numeroMovimientoRetiro"),
                    NumeroMovimientoITF = TryGetNullableInt32(datos, "NumeroMovimientoITF", "numeroMovimientoITF"),
                    NumeroMovimientoCargo = TryGetNullableInt32(datos, "NumeroMovimientoCargo", "numeroMovimientoCargo")
                };

                return new TransaccionResult { IsSuccess = true, Message = mensaje, Data = retiroResult };
            }
            catch (Exception ex)
            {
                return new TransaccionResult { IsSuccess = false, Message = $"Error al procesar respuesta: {ex.Message}" };
            }
        }

        private TransaccionResult ParseTransferenciaResponse(string jsonResponse)
        {
            try
            {
                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                bool exitoso = false;
                if (root.TryGetProperty("Exitoso", out var exitosoEl) || 
                    root.TryGetProperty("exitoso", out exitosoEl))
                {
                    exitoso = exitosoEl.GetBoolean();
                }

                string mensaje = "";
                if (root.TryGetProperty("Mensaje", out var mensajeEl) || 
                    root.TryGetProperty("mensaje", out mensajeEl))
                {
                    mensaje = mensajeEl.GetString() ?? "";
                }

                if (!exitoso)
                {
                    return new TransaccionResult { IsSuccess = false, Message = mensaje };
                }

                JsonElement datos;
                bool hasDatos = root.TryGetProperty("Datos", out datos) || 
                               root.TryGetProperty("datos", out datos);

                if (!hasDatos)
                {
                    return new TransaccionResult { IsSuccess = false, Message = "No se encontraron datos en la respuesta" };
                }

                // Para transferencia, CuentaOrigen y CuentaDestino son objetos
                JsonElement cuentaOrigen, cuentaDestino;
                
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
                    CuentaOrigen = TryGetString(cuentaOrigen, "Codigo", "codigo") ?? "",
                    CuentaDestino = TryGetString(cuentaDestino, "Codigo", "codigo") ?? "",
                    ImporteTransferido = TryGetDecimal(datos, "ImporteTransferido", "importeTransferido"),
                    SaldoAnteriorOrigen = TryGetDecimal(cuentaOrigen, "SaldoAnterior", "saldoAnterior"),
                    SaldoNuevoOrigen = TryGetDecimal(cuentaOrigen, "SaldoNuevo", "saldoNuevo"),
                    SaldoAnteriorDestino = TryGetDecimal(cuentaDestino, "SaldoAnterior", "saldoAnterior"),
                    SaldoNuevoDestino = TryGetDecimal(cuentaDestino, "SaldoNuevo", "saldoNuevo"),
                    NumeroMovimientoOrigen = TryGetInt32(datos, "NumeroMovimientoOrigen", "numeroMovimientoOrigen"),
                    NumeroMovimientoDestino = TryGetInt32(datos, "NumeroMovimientoDestino", "numeroMovimientoDestino")
                };

                return new TransaccionResult { IsSuccess = true, Message = mensaje, Data = transferenciaResult };
            }
            catch (Exception ex)
            {
                return new TransaccionResult { IsSuccess = false, Message = $"Error al procesar respuesta: {ex.Message}" };
            }
        }

        // Métodos helper para manejar ambas convenciones de nombres
        private string? TryGetString(JsonElement element, string pascalCase, string camelCase)
        {
            if (element.TryGetProperty(pascalCase, out var value) ||
                element.TryGetProperty(camelCase, out value))
            {
                return value.GetString();
            }
            return null;
        }

        private decimal TryGetDecimal(JsonElement element, string pascalCase, string camelCase)
        {
            if (element.TryGetProperty(pascalCase, out var value) ||
                element.TryGetProperty(camelCase, out value))
            {
                return value.GetDecimal();
            }
            return 0;
        }

        private int TryGetInt32(JsonElement element, string pascalCase, string camelCase)
        {
            if (element.TryGetProperty(pascalCase, out var value) ||
                element.TryGetProperty(camelCase, out value))
            {
                return value.GetInt32();
            }
            return 0;
        }

        private int? TryGetNullableInt32(JsonElement element, string pascalCase, string camelCase)
        {
            if (element.TryGetProperty(pascalCase, out var value) ||
                element.TryGetProperty(camelCase, out value))
            {
                return value.GetInt32();
            }
            return null;
        }
    }
}
