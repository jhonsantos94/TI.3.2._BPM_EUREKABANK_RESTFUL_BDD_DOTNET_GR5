using CliUniversalConsole.Config;
using CliUniversalConsole.Models;
using System.Text;
using System.Xml.Linq;

namespace CliUniversalConsole.Services.Soap
{
    public class SoapJavaTransaccionService : ITransaccionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public SoapJavaTransaccionService()
        {
            _httpClient = new HttpClient();
            _baseUrl = ServiceConfig.SoapJavaTransaccionUrl;
        }

        public async Task<TransaccionResult> RealizarDepositoAsync(TransaccionRequest request)
        {
            try
            {
                var soapEnvelope = $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ws=""http://ws.monster.edu.ec/"">
   <soapenv:Header/>
   <soapenv:Body>
      <ws:realizarDeposito>
         <datos>
            <codigoCuenta>{request.CodigoCuenta}</codigoCuenta>
            <claveCuenta>{request.ClaveCuenta}</claveCuenta>
            <importe>{request.Importe}</importe>
            <codigoEmpleado>{request.CodigoEmpleado}</codigoEmpleado>
         </datos>
      </ws:realizarDeposito>
   </soapenv:Body>
</soapenv:Envelope>";

                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "");

                var response = await _httpClient.PostAsync(_baseUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseBody))
                {
                    return new TransaccionResult 
                    { 
                        IsSuccess = false, 
                        Message = $"El servidor no respondió correctamente. Status: {response.StatusCode}" 
                    };
                }

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
                var soapEnvelope = $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ws=""http://ws.monster.edu.ec/"">
   <soapenv:Header/>
   <soapenv:Body>
      <ws:realizarRetiro>
         <datos>
            <codigoCuenta>{request.CodigoCuenta}</codigoCuenta>
            <claveCuenta>{request.ClaveCuenta}</claveCuenta>
            <importe>{request.Importe}</importe>
            <codigoEmpleado>{request.CodigoEmpleado}</codigoEmpleado>
         </datos>
      </ws:realizarRetiro>
   </soapenv:Body>
</soapenv:Envelope>";

                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "");

                var response = await _httpClient.PostAsync(_baseUrl, content);
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
                var soapEnvelope = $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ws=""http://ws.monster.edu.ec/"">
   <soapenv:Header/>
   <soapenv:Body>
      <ws:realizarTransferencia>
         <datos>
            <cuentaOrigen>{request.CuentaOrigen}</cuentaOrigen>
            <claveCuentaOrigen>{request.ClaveCuentaOrigen}</claveCuentaOrigen>
            <cuentaDestino>{request.CuentaDestino}</cuentaDestino>
            <importe>{request.Importe}</importe>
            <codigoEmpleado>{request.CodigoEmpleado}</codigoEmpleado>
         </datos>
      </ws:realizarTransferencia>
   </soapenv:Body>
</soapenv:Envelope>";

                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "");

                var response = await _httpClient.PostAsync(_baseUrl, content);
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

        private TransaccionResult ParseDepositoResponse(string xmlResponse)
        {
            try
            {
                var xdoc = XDocument.Parse(xmlResponse);
                XNamespace ns = "http://ws.monster.edu.ec/";

                // Buscar el elemento 'return' - puede estar con o sin namespace
                var returnElement = xdoc.Descendants(ns + "return").FirstOrDefault() 
                                    ?? xdoc.Descendants("return").FirstOrDefault();
                
                if (returnElement == null)
                {
                    return new TransaccionResult { IsSuccess = false, Message = "Respuesta inválida del servidor" };
                }

                var exitoso = bool.Parse(returnElement.Element("exitoso")?.Value ?? "false");
                var mensaje = returnElement.Element("mensaje")?.Value ?? "";

                if (!exitoso)
                {
                    var codigoError = returnElement.Element("codigoError")?.Value ?? "";
                    var mensajeCompleto = !string.IsNullOrEmpty(codigoError) ? $"{mensaje} ({codigoError})" : mensaje;
                    return new TransaccionResult { IsSuccess = false, Message = mensajeCompleto };
                }

                var datosElement = returnElement.Element("datos");
                if (datosElement == null)
                {
                    return new TransaccionResult { IsSuccess = false, Message = "No se encontraron datos en la respuesta" };
                }

                var depositoResult = new DepositoResult
                {
                    CodigoCuenta = datosElement.Element("codigoCuenta")?.Value ?? "",
                    ImporteDepositado = decimal.Parse(datosElement.Element("importeDepositado")?.Value ?? "0"),
                    SaldoAnterior = decimal.Parse(datosElement.Element("saldoAnterior")?.Value ?? "0"),
                    SaldoNuevo = decimal.Parse(datosElement.Element("saldoNuevo")?.Value ?? "0"),
                    NumeroMovimiento = int.Parse(datosElement.Element("numeroMovimiento")?.Value ?? "0")
                };

                return new TransaccionResult { IsSuccess = true, Message = mensaje, Data = depositoResult };
            }
            catch (Exception ex)
            {
                return new TransaccionResult { IsSuccess = false, Message = $"Error al procesar respuesta: {ex.Message}" };
            }
        }

        private TransaccionResult ParseRetiroResponse(string xmlResponse)
        {
            try
            {
                var xdoc = XDocument.Parse(xmlResponse);
                XNamespace ns = "http://ws.monster.edu.ec/";

                // Buscar el elemento 'return' - puede estar con o sin namespace
                var returnElement = xdoc.Descendants(ns + "return").FirstOrDefault() 
                                    ?? xdoc.Descendants("return").FirstOrDefault();
                
                if (returnElement == null)
                {
                    return new TransaccionResult { IsSuccess = false, Message = "Respuesta inválida del servidor" };
                }

                var exitoso = bool.Parse(returnElement.Element("exitoso")?.Value ?? "false");
                var mensaje = returnElement.Element("mensaje")?.Value ?? "";

                if (!exitoso)
                {
                    var codigoError = returnElement.Element("codigoError")?.Value ?? "";
                    var mensajeCompleto = !string.IsNullOrEmpty(codigoError) ? $"{mensaje} ({codigoError})" : mensaje;
                    return new TransaccionResult { IsSuccess = false, Message = mensajeCompleto };
                }

                var datosElement = returnElement.Element("datos");
                if (datosElement == null)
                {
                    return new TransaccionResult { IsSuccess = false, Message = "No se encontraron datos en la respuesta" };
                }

                var retiroResult = new RetiroResult
                {
                    CodigoCuenta = datosElement.Element("codigoCuenta")?.Value ?? "",
                    ImporteRetirado = decimal.Parse(datosElement.Element("importeRetirado")?.Value ?? "0"),
                    ImporteITF = decimal.Parse(datosElement.Element("importeITF")?.Value ?? "0"),
                    ImporteCargo = decimal.Parse(datosElement.Element("importeCargo")?.Value ?? "0"),
                    TotalDescontado = decimal.Parse(datosElement.Element("totalDescontado")?.Value ?? "0"),
                    SaldoAnterior = decimal.Parse(datosElement.Element("saldoAnterior")?.Value ?? "0"),
                    SaldoNuevo = decimal.Parse(datosElement.Element("saldoNuevo")?.Value ?? "0"),
                    NumeroMovimientoRetiro = int.Parse(datosElement.Element("numeroMovimientoRetiro")?.Value ?? "0"),
                    NumeroMovimientoITF = int.TryParse(datosElement.Element("numeroMovimientoITF")?.Value, out var itf) ? itf : null,
                    NumeroMovimientoCargo = int.TryParse(datosElement.Element("numeroMovimientoCargo")?.Value, out var cargo) ? cargo : null
                };

                return new TransaccionResult { IsSuccess = true, Message = mensaje, Data = retiroResult };
            }
            catch (Exception ex)
            {
                return new TransaccionResult { IsSuccess = false, Message = $"Error al procesar respuesta: {ex.Message}" };
            }
        }

        private TransaccionResult ParseTransferenciaResponse(string xmlResponse)
        {
            try
            {
                var xdoc = XDocument.Parse(xmlResponse);
                XNamespace ns = "http://ws.monster.edu.ec/";

                // Buscar el elemento 'return' - puede estar con o sin namespace
                var returnElement = xdoc.Descendants(ns + "return").FirstOrDefault() 
                                    ?? xdoc.Descendants("return").FirstOrDefault();
                
                if (returnElement == null)
                {
                    return new TransaccionResult { IsSuccess = false, Message = "Respuesta inválida del servidor" };
                }

                var exitoso = bool.Parse(returnElement.Element("exitoso")?.Value ?? "false");
                var mensaje = returnElement.Element("mensaje")?.Value ?? "";

                if (!exitoso)
                {
                    var codigoError = returnElement.Element("codigoError")?.Value ?? "";
                    var mensajeCompleto = !string.IsNullOrEmpty(codigoError) ? $"{mensaje} ({codigoError})" : mensaje;
                    return new TransaccionResult { IsSuccess = false, Message = mensajeCompleto };
                }

                var datosElement = returnElement.Element("datos");
                if (datosElement == null)
                {
                    return new TransaccionResult { IsSuccess = false, Message = "No se encontraron datos en la respuesta" };
                }

                var transferenciaResult = new TransferenciaResult
                {
                    CuentaOrigen = datosElement.Element("cuentaOrigen")?.Value ?? "",
                    CuentaDestino = datosElement.Element("cuentaDestino")?.Value ?? "",
                    ImporteTransferido = decimal.Parse(datosElement.Element("importeTransferido")?.Value ?? "0"),
                    SaldoAnteriorOrigen = decimal.Parse(datosElement.Element("saldoAnteriorOrigen")?.Value ?? "0"),
                    SaldoNuevoOrigen = decimal.Parse(datosElement.Element("saldoNuevoOrigen")?.Value ?? "0"),
                    SaldoAnteriorDestino = decimal.Parse(datosElement.Element("saldoAnteriorDestino")?.Value ?? "0"),
                    SaldoNuevoDestino = decimal.Parse(datosElement.Element("saldoNuevoDestino")?.Value ?? "0"),
                    NumeroMovimientoOrigen = int.Parse(datosElement.Element("numeroMovimientoOrigen")?.Value ?? "0"),
                    NumeroMovimientoDestino = int.Parse(datosElement.Element("numeroMovimientoDestino")?.Value ?? "0")
                };

                return new TransaccionResult { IsSuccess = true, Message = mensaje, Data = transferenciaResult };
            }
            catch (Exception ex)
            {
                return new TransaccionResult { IsSuccess = false, Message = $"Error al procesar respuesta: {ex.Message}" };
            }
        }
    }
}
