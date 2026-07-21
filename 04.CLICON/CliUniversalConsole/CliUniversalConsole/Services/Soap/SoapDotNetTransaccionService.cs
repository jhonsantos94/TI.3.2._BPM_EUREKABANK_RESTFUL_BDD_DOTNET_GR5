using CliUniversalConsole.Config;
using CliUniversalConsole.Models;
using System.Text;
using System.Xml.Linq;

namespace CliUniversalConsole.Services.Soap
{
    public class SoapDotNetTransaccionService : ITransaccionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public SoapDotNetTransaccionService()
        {
            _httpClient = new HttpClient();
            _baseUrl = ServiceConfig.SoapDotNetTransaccionUrl;
        }

        public async Task<TransaccionResult> RealizarDepositoAsync(TransaccionRequest request)
        {
            try
            {
                var soapEnvelope = $@"
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"" xmlns:eur=""http://schemas.datacontract.org/2004/07/EurekaBank_Soap_DotNet_GR01.Models.DTOs"">
   <soap:Header/>
   <soap:Body>
      <tem:RealizarDeposito>
         <tem:datos>
            <eur:ClaveCuenta>{request.ClaveCuenta}</eur:ClaveCuenta>
            <eur:CodigoCuenta>{request.CodigoCuenta}</eur:CodigoCuenta>
            <eur:CodigoEmpleado>{request.CodigoEmpleado}</eur:CodigoEmpleado>
            <eur:CodigoTipoMovimiento>001</eur:CodigoTipoMovimiento>
            <eur:Importe>{request.Importe}</eur:Importe>
         </tem:datos>
      </tem:RealizarDeposito>
   </soap:Body>
</soap:Envelope>";

                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "http://tempuri.org/IServicioTransaccion/RealizarDeposito");

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
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"" xmlns:eur=""http://schemas.datacontract.org/2004/07/EurekaBank_Soap_DotNet_GR01.Models.DTOs"">
   <soap:Header/>
   <soap:Body>
      <tem:RealizarRetiro>
         <tem:datos>
            <eur:ClaveCuenta>{request.ClaveCuenta}</eur:ClaveCuenta>
            <eur:CodigoCuenta>{request.CodigoCuenta}</eur:CodigoCuenta>
            <eur:CodigoEmpleado>{request.CodigoEmpleado}</eur:CodigoEmpleado>
            <eur:CodigoTipoMovimiento>002</eur:CodigoTipoMovimiento>
            <eur:Importe>{request.Importe}</eur:Importe>
         </tem:datos>
      </tem:RealizarRetiro>
   </soap:Body>
</soap:Envelope>";

                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "http://tempuri.org/IServicioTransaccion/RealizarRetiro");

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
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"" xmlns:eur=""http://schemas.datacontract.org/2004/07/EurekaBank_Soap_DotNet_GR01.Models.DTOs"">
   <soap:Header/>
   <soap:Body>
      <tem:RealizarTransferencia>
         <tem:datos>
            <eur:ClaveCuentaOrigen>{request.ClaveCuentaOrigen}</eur:ClaveCuentaOrigen>
            <eur:CodigoEmpleado>{request.CodigoEmpleado}</eur:CodigoEmpleado>
            <eur:CodigoTipoMovimiento>009</eur:CodigoTipoMovimiento>
            <eur:CuentaDestino>{request.CuentaDestino}</eur:CuentaDestino>
            <eur:CuentaOrigen>{request.CuentaOrigen}</eur:CuentaOrigen>
            <eur:Importe>{request.Importe}</eur:Importe>
         </tem:datos>
      </tem:RealizarTransferencia>
   </soap:Body>
</soap:Envelope>";

                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "http://tempuri.org/IServicioTransaccion/RealizarTransferencia");

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
                XNamespace tem = "http://tempuri.org/";
                XNamespace a = "http://schemas.datacontract.org/2004/07/EurekaBank_Soap_DotNet_GR01.Models.DTOs";

                var resultElement = xdoc.Descendants(tem + "RealizarDepositoResult").FirstOrDefault();
                if (resultElement == null)
                {
                    return new TransaccionResult { IsSuccess = false, Message = "Respuesta inválida del servidor" };
                }

                var exitoso = bool.Parse(resultElement.Element(a + "Exitoso")?.Value ?? "false");
                var mensaje = resultElement.Element(a + "Mensaje")?.Value ?? "";

                if (!exitoso)
                {
                    return new TransaccionResult { IsSuccess = false, Message = mensaje };
                }

                var datosElement = resultElement.Element(a + "Datos");
                var depositoResult = new DepositoResult
                {
                    CodigoCuenta = datosElement?.Element(a + "CodigoCuenta")?.Value ?? "",
                    ImporteDepositado = decimal.Parse(datosElement?.Element(a + "Importe")?.Value ?? "0"),
                    SaldoAnterior = decimal.Parse(datosElement?.Element(a + "SaldoAnterior")?.Value ?? "0"),
                    SaldoNuevo = decimal.Parse(datosElement?.Element(a + "SaldoNuevo")?.Value ?? "0"),
                    NumeroMovimiento = int.Parse(datosElement?.Element(a + "NumeroMovimiento")?.Value ?? "0")
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
                XNamespace tem = "http://tempuri.org/";
                XNamespace a = "http://schemas.datacontract.org/2004/07/EurekaBank_Soap_DotNet_GR01.Models.DTOs";

                var resultElement = xdoc.Descendants(tem + "RealizarRetiroResult").FirstOrDefault();
                if (resultElement == null)
                {
                    return new TransaccionResult { IsSuccess = false, Message = "Respuesta inválida del servidor" };
                }

                var exitoso = bool.Parse(resultElement.Element(a + "Exitoso")?.Value ?? "false");
                var mensaje = resultElement.Element(a + "Mensaje")?.Value ?? "";

                if (!exitoso)
                {
                    return new TransaccionResult { IsSuccess = false, Message = mensaje };
                }

                var datosElement = resultElement.Element(a + "Datos");
                var retiroResult = new RetiroResult
                {
                    CodigoCuenta = datosElement?.Element(a + "CodigoCuenta")?.Value ?? "",
                    ImporteRetirado = decimal.Parse(datosElement?.Element(a + "ImporteRetirado")?.Value ?? "0"),
                    ImporteITF = decimal.Parse(datosElement?.Element(a + "ITF")?.Value ?? "0"),
                    ImporteCargo = decimal.Parse(datosElement?.Element(a + "CostoPorMovimiento")?.Value ?? "0"),
                    TotalDescontado = decimal.Parse(datosElement?.Element(a + "TotalDescontado")?.Value ?? "0"),
                    SaldoAnterior = decimal.Parse(datosElement?.Element(a + "SaldoAnterior")?.Value ?? "0"),
                    SaldoNuevo = decimal.Parse(datosElement?.Element(a + "SaldoNuevo")?.Value ?? "0"),
                    NumeroMovimientoRetiro = int.Parse(datosElement?.Element(a + "NumeroMovimientoRetiro")?.Value ?? "0"),
                    NumeroMovimientoITF = int.TryParse(datosElement?.Element(a + "NumeroMovimientoITF")?.Value, out var itf) ? itf : null,
                    NumeroMovimientoCargo = int.TryParse(datosElement?.Element(a + "NumeroMovimientoCargo")?.Value, out var cargo) ? cargo : null
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
                XNamespace tem = "http://tempuri.org/";
                XNamespace a = "http://schemas.datacontract.org/2004/07/EurekaBank_Soap_DotNet_GR01.Models.DTOs";

                var resultElement = xdoc.Descendants(tem + "RealizarTransferenciaResult").FirstOrDefault();
                if (resultElement == null)
                {
                    return new TransaccionResult { IsSuccess = false, Message = "Respuesta inválida del servidor" };
                }

                var exitoso = bool.Parse(resultElement.Element(a + "Exitoso")?.Value ?? "false");
                var mensaje = resultElement.Element(a + "Mensaje")?.Value ?? "";

                if (!exitoso)
                {
                    return new TransaccionResult { IsSuccess = false, Message = mensaje };
                }

                var datosElement = resultElement.Element(a + "Datos");
                var cuentaOrigenElement = datosElement?.Element(a + "CuentaOrigen");
                var cuentaDestinoElement = datosElement?.Element(a + "CuentaDestino");
                
                var transferenciaResult = new TransferenciaResult
                {
                    CuentaOrigen = cuentaOrigenElement?.Element(a + "Codigo")?.Value ?? "",
                    CuentaDestino = cuentaDestinoElement?.Element(a + "Codigo")?.Value ?? "",
                    ImporteTransferido = decimal.Parse(datosElement?.Element(a + "ImporteTransferido")?.Value ?? "0"),
                    SaldoAnteriorOrigen = decimal.Parse(cuentaOrigenElement?.Element(a + "SaldoAnterior")?.Value ?? "0"),
                    SaldoNuevoOrigen = decimal.Parse(cuentaOrigenElement?.Element(a + "SaldoNuevo")?.Value ?? "0"),
                    SaldoAnteriorDestino = decimal.Parse(cuentaDestinoElement?.Element(a + "SaldoAnterior")?.Value ?? "0"),
                    SaldoNuevoDestino = decimal.Parse(cuentaDestinoElement?.Element(a + "SaldoNuevo")?.Value ?? "0"),
                    NumeroMovimientoOrigen = int.Parse(cuentaOrigenElement?.Element(a + "NumeroMovimiento")?.Value ?? "0"),
                    NumeroMovimientoDestino = int.Parse(cuentaDestinoElement?.Element(a + "NumeroMovimiento")?.Value ?? "0")
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
