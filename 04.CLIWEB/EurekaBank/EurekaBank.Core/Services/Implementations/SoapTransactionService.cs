// En: EurekaBank.Core/Services/Implementations/SoapTransactionService.cs
using EurekaBank.Core.Managers;
using EurekaBank.Core.Models.Requests;
using EurekaBank.Core.Models.Responses;
using EurekaBank.Core.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using System.ServiceModel;
using Newtonsoft.Json;
using System.Xml;
using System.Text;
using System.Xml.Linq;

// Importamos los namespaces de los clientes generados
using DotNetSoapTransaction;
using JavaSoapTransaction;

namespace EurekaBank.Core.Services.Implementations
{
    public class SoapTransactionService : ITransactionService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private ApiPlatform _currentTarget = ApiPlatform.Java;
        private static readonly BasicHttpBinding Binding = new BasicHttpBinding();

        public SoapTransactionService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public void SetTarget(ApiPlatform target)
        {
            _currentTarget = target;
        }

        // --- MÉTODOS PÚBLICOS DE LA INTERFAZ ---

        public async Task<TransactionResponse<DepositResponseData>> RealizarDepositoAsync(DepositRequest request)
        {
            try
            {
                if (_currentTarget == ApiPlatform.DotNet)
                {
                    var client = GetDotNetClient();
                    var soapRequest = new DotNetSoapTransaction.TransaccionDTO // CORRECCIÓN: Nombre de tipo
                    {
                        CodigoCuenta = request.CodigoCuenta,
                        ClaveCuenta = request.ClaveCuenta,
                        Importe = request.Importe,
                        CodigoEmpleado = request.CodigoEmpleado,
                        CodigoTipoMovimiento = request.CodigoTipoMovimiento
                    };
                    var soapResponse = await client.RealizarDepositoAsync(soapRequest);
                    return MapDotNetDepositResponse(soapResponse);
                }
                else
                {
                    return await RealizarDepositoJavaManualAsync(request);
                }
            }
            catch (Exception ex)
            {
                return new TransactionResponse<DepositResponseData> { Exitoso = false, Mensaje = $"Error en la operación de depósito: {ex.Message}" };
            }
        }

        public async Task<TransactionResponse<WithdrawResponseData>> RealizarRetiroAsync(DepositRequest request)
        {
            try
            {
                if (_currentTarget == ApiPlatform.DotNet)
                {
                    var client = GetDotNetClient();
                    var soapRequest = new DotNetSoapTransaction.TransaccionDTO // CORRECCIÓN: Nombre de tipo
                    {
                        CodigoCuenta = request.CodigoCuenta,
                        ClaveCuenta = request.ClaveCuenta,
                        Importe = request.Importe,
                        CodigoEmpleado = request.CodigoEmpleado,
                        CodigoTipoMovimiento = request.CodigoTipoMovimiento
                    };
                    var soapResponse = await client.RealizarRetiroAsync(soapRequest);
                    return MapDotNetWithdrawResponse(soapResponse);
                }
                else
                {
                    return await RealizarRetiroJavaManualAsync(request);
                }
            }
            catch (Exception ex)
            {
                return new TransactionResponse<WithdrawResponseData> { Exitoso = false, Mensaje = $"Error en la operación de retiro: {ex.Message}" };
            }
        }

        public async Task<TransactionResponse<TransferResponseData>> RealizarTransferenciaAsync(TransferRequest request)
        {
            try
            {
                if (_currentTarget == ApiPlatform.DotNet)
                {
                    var client = GetDotNetClient();
                    var soapRequest = new DotNetSoapTransaction.TransferenciaDTO
                    {
                        CuentaOrigen = request.CuentaOrigen,
                        CuentaDestino = request.CuentaDestino,
                        ClaveCuentaOrigen = request.ClaveCuentaOrigen,
                        Importe = request.Importe,
                        CodigoEmpleado = request.CodigoEmpleado
                    };
                    var soapResponse = await client.RealizarTransferenciaAsync(soapRequest);
                    return MapDotNetTransferResponse(soapResponse);
                }
                else
                {
                    return await RealizarTransferenciaJavaManualAsync(request);
                }
            }
            catch (Exception ex)
            {
                return new TransactionResponse<TransferResponseData> { Exitoso = false, Mensaje = $"Error en la operación de transferencia: {ex.Message}" };
            }
        }

        // --- MÉTODOS MANUALES PARA JAVA SOAP ---

        private async Task<TransactionResponse<DepositResponseData>> RealizarDepositoJavaManualAsync(DepositRequest request)
        {
            var endpointUrl = _configuration["Hosts:Soap:Java:Transaction"];
            var baseIp = _configuration["ServerConfig:BaseIp"];

            if (!string.IsNullOrWhiteSpace(endpointUrl) && !string.IsNullOrWhiteSpace(baseIp))
            {
                endpointUrl = endpointUrl.Replace("{IP}", baseIp);
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"=== MANUAL SOAP JAVA DEPOSIT ===");
                System.Diagnostics.Debug.WriteLine($"Endpoint: {endpointUrl}");
                System.Diagnostics.Debug.WriteLine($"Importe: {request.Importe}");

                var soapEnvelope = CreateDepositSoapEnvelope(request);
                System.Diagnostics.Debug.WriteLine($"SOAP Request XML: {soapEnvelope}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpointUrl)
                {
                    Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml")
                };
                
                httpRequest.Headers.Add("SOAPAction", "http://ws.monster.edu.ec/ServicioTransaccion/realizarDepositoRequest");

                var httpResponse = await _httpClient.SendAsync(httpRequest);
                var responseXml = await httpResponse.Content.ReadAsStringAsync();
                
                System.Diagnostics.Debug.WriteLine($"=== MANUAL SOAP JAVA DEPOSIT RESPONSE ===");
                System.Diagnostics.Debug.WriteLine($"Status: {httpResponse.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response XML: {responseXml}");

                return ParseJavaDepositResponse(responseXml);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Manual SOAP deposit failed: {ex.Message}");
                return new TransactionResponse<DepositResponseData> { Exitoso = false, Mensaje = $"Error en llamada SOAP manual: {ex.Message}" };
            }
        }

        private async Task<TransactionResponse<WithdrawResponseData>> RealizarRetiroJavaManualAsync(DepositRequest request)
        {
            var endpointUrl = _configuration["Hosts:Soap:Java:Transaction"];
            var baseIp = _configuration["ServerConfig:BaseIp"];

            if (!string.IsNullOrWhiteSpace(endpointUrl) && !string.IsNullOrWhiteSpace(baseIp))
            {
                endpointUrl = endpointUrl.Replace("{IP}", baseIp);
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"=== MANUAL SOAP JAVA WITHDRAW ===");
                System.Diagnostics.Debug.WriteLine($"Importe: {request.Importe}");

                var soapEnvelope = CreateWithdrawSoapEnvelope(request);
                System.Diagnostics.Debug.WriteLine($"SOAP Request XML: {soapEnvelope}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpointUrl)
                {
                    Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml")
                };
                
                httpRequest.Headers.Add("SOAPAction", "http://ws.monster.edu.ec/ServicioTransaccion/realizarRetiroRequest");

                var httpResponse = await _httpClient.SendAsync(httpRequest);
                var responseXml = await httpResponse.Content.ReadAsStringAsync();
                
                System.Diagnostics.Debug.WriteLine($"Response XML: {responseXml}");

                return ParseJavaWithdrawResponse(responseXml);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Manual SOAP withdraw failed: {ex.Message}");
                return new TransactionResponse<WithdrawResponseData> { Exitoso = false, Mensaje = $"Error en llamada SOAP manual: {ex.Message}" };
            }
        }

        private async Task<TransactionResponse<TransferResponseData>> RealizarTransferenciaJavaManualAsync(TransferRequest request)
        {
            var endpointUrl = _configuration["Hosts:Soap:Java:Transaction"];
            var baseIp = _configuration["ServerConfig:BaseIp"];

            if (!string.IsNullOrWhiteSpace(endpointUrl) && !string.IsNullOrWhiteSpace(baseIp))
            {
                endpointUrl = endpointUrl.Replace("{IP}", baseIp);
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"=== MANUAL SOAP JAVA TRANSFER ===");
                System.Diagnostics.Debug.WriteLine($"Endpoint: {endpointUrl}");
                System.Diagnostics.Debug.WriteLine($"CuentaOrigen: {request.CuentaOrigen}");
                System.Diagnostics.Debug.WriteLine($"CuentaDestino: {request.CuentaDestino}");
                System.Diagnostics.Debug.WriteLine($"Importe: {request.Importe}");

                var soapEnvelope = CreateTransferSoapEnvelope(request);
                System.Diagnostics.Debug.WriteLine($"SOAP Request XML: {soapEnvelope}");

                var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpointUrl)
                {
                    Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml")
                };
                
                httpRequest.Headers.Add("SOAPAction", "http://ws.monster.edu.ec/ServicioTransaccion/realizarTransferenciaRequest");

                var httpResponse = await _httpClient.SendAsync(httpRequest);
                var responseXml = await httpResponse.Content.ReadAsStringAsync();
                
                System.Diagnostics.Debug.WriteLine($"=== MANUAL SOAP JAVA TRANSFER RESPONSE ===");
                System.Diagnostics.Debug.WriteLine($"Status: {httpResponse.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response XML: {responseXml}");

                return ParseJavaTransferResponse(responseXml);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Manual SOAP transfer failed: {ex.Message}");
                return new TransactionResponse<TransferResponseData> { Exitoso = false, Mensaje = $"Error en llamada SOAP manual: {ex.Message}" };
            }
        }

        // --- CREADORES DE XML SOAP PARA JAVA ---

        private string CreateDepositSoapEnvelope(DepositRequest request)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ws=""http://ws.monster.edu.ec/"">
    <soapenv:Header/>
    <soapenv:Body>
        <ws:realizarDeposito>
            <datos>
                <codigoCuenta>{request.CodigoCuenta}</codigoCuenta>
                <claveCuenta>{request.ClaveCuenta}</claveCuenta>
                <importe>{request.Importe}</importe>
                <codigoEmpleado>{request.CodigoEmpleado}</codigoEmpleado>
                <codigoTipoMovimiento>{request.CodigoTipoMovimiento}</codigoTipoMovimiento>
            </datos>
        </ws:realizarDeposito>
    </soapenv:Body>
</soapenv:Envelope>";
        }

        private string CreateWithdrawSoapEnvelope(DepositRequest request)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ws=""http://ws.monster.edu.ec/"">
    <soapenv:Header/>
    <soapenv:Body>
        <ws:realizarRetiro>
            <datos>
                <codigoCuenta>{request.CodigoCuenta}</codigoCuenta>
                <claveCuenta>{request.ClaveCuenta}</claveCuenta>
                <importe>{request.Importe}</importe>
                <codigoEmpleado>{request.CodigoEmpleado}</codigoEmpleado>
                <codigoTipoMovimiento>{request.CodigoTipoMovimiento}</codigoTipoMovimiento>
            </datos>
        </ws:realizarRetiro>
    </soapenv:Body>
</soapenv:Envelope>";
        }

        private string CreateTransferSoapEnvelope(TransferRequest request)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ws=""http://ws.monster.edu.ec/"">
    <soapenv:Header/>
    <soapenv:Body>
        <ws:realizarTransferencia>
            <datos>
                <cuentaOrigen>{request.CuentaOrigen}</cuentaOrigen>
                <cuentaDestino>{request.CuentaDestino}</cuentaDestino>
                <claveCuentaOrigen>{request.ClaveCuentaOrigen}</claveCuentaOrigen>
                <importe>{request.Importe}</importe>
                <codigoEmpleado>{request.CodigoEmpleado}</codigoEmpleado>
            </datos>
        </ws:realizarTransferencia>
    </soapenv:Body>
</soapenv:Envelope>";
        }

        // --- PARSERS DE RESPUESTA JAVA ---

        private TransactionResponse<DepositResponseData> ParseJavaDepositResponse(string xmlResponse)
        {
            try
            {
                var doc = XDocument.Parse(xmlResponse);
                var returnElement = doc.Descendants("return").FirstOrDefault();
                
                if (returnElement == null)
                {
                    return new TransactionResponse<DepositResponseData> { Exitoso = false, Mensaje = "No se encontró elemento return en la respuesta" };
                }

                var exitoso = bool.Parse(returnElement.Element("exitoso")?.Value ?? "false");
                var mensaje = returnElement.Element("mensaje")?.Value ?? "";
                var codigoError = returnElement.Element("codigoError")?.Value;

                var response = new TransactionResponse<DepositResponseData>
                {
                    Exitoso = exitoso,
                    Mensaje = mensaje,
                    CodigoError = codigoError
                };

                if (exitoso)
                {
                    var datosElement = returnElement.Element("datos");
                    if (datosElement != null)
                    {
                        response.Datos = new DepositResponseData
                        {
                            NumeroMovimiento = int.Parse(datosElement.Element("numeroMovimiento")?.Value ?? "0"),
                            SaldoAnterior = decimal.Parse(datosElement.Element("saldoAnterior")?.Value ?? "0"),
                            SaldoNuevo = decimal.Parse(datosElement.Element("saldoNuevo")?.Value ?? "0"),
                            Importe = decimal.Parse(datosElement.Element("importeDepositado")?.Value ?? "0")
                        };
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                return new TransactionResponse<DepositResponseData> { Exitoso = false, Mensaje = $"Error parsing deposit response: {ex.Message}" };
            }
        }

        private TransactionResponse<WithdrawResponseData> ParseJavaWithdrawResponse(string xmlResponse)
        {
            try
            {
                var doc = XDocument.Parse(xmlResponse);
                var returnElement = doc.Descendants("return").FirstOrDefault();
                
                if (returnElement == null)
                {
                    return new TransactionResponse<WithdrawResponseData> { Exitoso = false, Mensaje = "No se encontró elemento return en la respuesta" };
                }

                var exitoso = bool.Parse(returnElement.Element("exitoso")?.Value ?? "false");
                var mensaje = returnElement.Element("mensaje")?.Value ?? "";
                var codigoError = returnElement.Element("codigoError")?.Value;

                var response = new TransactionResponse<WithdrawResponseData>
                {
                    Exitoso = exitoso,
                    Mensaje = mensaje,
                    CodigoError = codigoError
                };

                if (exitoso)
                {
                    var datosElement = returnElement.Element("datos");
                    if (datosElement != null)
                    {
                        response.Datos = new WithdrawResponseData
                        {
                            SaldoAnterior = decimal.Parse(datosElement.Element("saldoAnterior")?.Value ?? "0"),
                            SaldoNuevo = decimal.Parse(datosElement.Element("saldoNuevo")?.Value ?? "0"),
                            ImporteRetiro = decimal.Parse(datosElement.Element("importeRetirado")?.Value ?? "0"),
                            Itf = decimal.Parse(datosElement.Element("importeITF")?.Value ?? "0"),
                            CostoPorMovimiento = decimal.Parse(datosElement.Element("importeCargo")?.Value ?? "0"),
                            TotalDescontado = decimal.Parse(datosElement.Element("totalDescontado")?.Value ?? "0")
                        };
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                return new TransactionResponse<WithdrawResponseData> { Exitoso = false, Mensaje = $"Error parsing withdraw response: {ex.Message}" };
            }
        }

        private TransactionResponse<TransferResponseData> ParseJavaTransferResponse(string xmlResponse)
        {
            try
            {
                var doc = XDocument.Parse(xmlResponse);
                var returnElement = doc.Descendants("return").FirstOrDefault();
                
                if (returnElement == null)
                {
                    return new TransactionResponse<TransferResponseData> { Exitoso = false, Mensaje = "No se encontró elemento return en la respuesta" };
                }

                var exitoso = bool.Parse(returnElement.Element("exitoso")?.Value ?? "false");
                var mensaje = returnElement.Element("mensaje")?.Value ?? "";
                var codigoError = returnElement.Element("codigoError")?.Value;

                System.Diagnostics.Debug.WriteLine($"Transfer parsed - Exitoso: {exitoso}, Mensaje: {mensaje}");

                var response = new TransactionResponse<TransferResponseData>
                {
                    Exitoso = exitoso,
                    Mensaje = mensaje,
                    CodigoError = codigoError
                };

                if (exitoso)
                {
                    var datosElement = returnElement.Element("datos");
                    if (datosElement != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found datos element for transfer: {datosElement}");
                        
                        response.Datos = new TransferResponseData
                        {
                            CuentaOrigen = new AccountMovement 
                            { 
                                Codigo = datosElement.Element("cuentaOrigen")?.Value ?? "",
                                SaldoAnterior = decimal.Parse(datosElement.Element("saldoAnteriorOrigen")?.Value ?? "0"),
                                SaldoNuevo = decimal.Parse(datosElement.Element("saldoNuevoOrigen")?.Value ?? "0")
                            },
                            CuentaDestino = new AccountMovement 
                            { 
                                Codigo = datosElement.Element("cuentaDestino")?.Value ?? "",
                                SaldoAnterior = decimal.Parse(datosElement.Element("saldoAnteriorDestino")?.Value ?? "0"),
                                SaldoNuevo = decimal.Parse(datosElement.Element("saldoNuevoDestino")?.Value ?? "0")
                            },
                            ImporteTransferido = decimal.Parse(datosElement.Element("importeTransferido")?.Value ?? "0"),
                            Itf = 0, // No viene en la respuesta Java
                            CostoPorMovimiento = 0, // No viene en la respuesta Java
                            TotalDescontado = decimal.Parse(datosElement.Element("importeTransferido")?.Value ?? "0")
                        };

                        System.Diagnostics.Debug.WriteLine($"Transfer data extracted - ImporteTransferido: {response.Datos.ImporteTransferido}, SaldoNuevoOrigen: {response.Datos.CuentaOrigen.SaldoNuevo}");
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing transfer response: {ex.Message}");
                return new TransactionResponse<TransferResponseData> { Exitoso = false, Mensaje = $"Error parsing transfer response: {ex.Message}" };
            }
        }

        // --- CLIENT FACTORIES ---

        private DotNetSoapTransaction.ServicioTransaccionClient GetDotNetClient()
        {
            var url = _configuration["Hosts:Soap:DotNet:Transaction"];
            var baseIp = _configuration["ServerConfig:BaseIp"];

            if (!string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(baseIp))
            {
                url = url.Replace("{IP}", baseIp);
            }
            return new DotNetSoapTransaction.ServicioTransaccionClient(Binding, new EndpointAddress(url));
        }

        private JavaSoapTransaction.ServicioTransaccionClient GetJavaClient()
        {
            var url = _configuration["Hosts:Soap:Java:Transaction"];
            return new JavaSoapTransaction.ServicioTransaccionClient(Binding, new EndpointAddress(url));
        }

        // --- MÉTODOS DE MAPEO PARA .NET ---

        private TransactionResponse<DepositResponseData> MapDotNetDepositResponse(DotNetSoapTransaction.RespuestaDTO r)
        {
            var response = new TransactionResponse<DepositResponseData> { Exitoso = r.Exitoso, Mensaje = r.Mensaje, CodigoError = r.CodigoError };
            if (r.Exitoso && r.Datos is DotNetSoapTransaction.DepositoResultDTO d)
            {
                response.Datos = new DepositResponseData { NumeroMovimiento = d.NumeroMovimiento, SaldoAnterior = d.SaldoAnterior, SaldoNuevo = d.SaldoNuevo, Importe = d.Importe };
            }
            return response;
        }

        private TransactionResponse<WithdrawResponseData> MapDotNetWithdrawResponse(DotNetSoapTransaction.RespuestaDTO r)
        {
            var response = new TransactionResponse<WithdrawResponseData> { Exitoso = r.Exitoso, Mensaje = r.Mensaje, CodigoError = r.CodigoError };
            if (r.Exitoso && r.Datos is DotNetSoapTransaction.RetiroResultDTO d)
            {
                response.Datos = new WithdrawResponseData { SaldoAnterior = d.SaldoAnterior, SaldoNuevo = d.SaldoNuevo, ImporteRetiro = d.ImporteRetiro, Itf = d.ITF, CostoPorMovimiento = d.CostoPorMovimiento, TotalDescontado = d.TotalDescontado };
            }
            return response;
        }

        private TransactionResponse<TransferResponseData> MapDotNetTransferResponse(DotNetSoapTransaction.RespuestaDTO r)
        {
            var response = new TransactionResponse<TransferResponseData> { Exitoso = r.Exitoso, Mensaje = r.Mensaje, CodigoError = r.CodigoError };
            if (r.Exitoso && r.Datos is DotNetSoapTransaction.TransferenciaResultDTO d)
            {
                response.Datos = new TransferResponseData
                {
                    CuentaOrigen = new AccountMovement { Codigo = d.CuentaOrigen.Codigo, SaldoAnterior = d.CuentaOrigen.SaldoAnterior, SaldoNuevo = d.CuentaOrigen.SaldoNuevo },
                    CuentaDestino = new AccountMovement { Codigo = d.CuentaDestino.Codigo, SaldoAnterior = d.CuentaDestino.SaldoAnterior, SaldoNuevo = d.CuentaDestino.SaldoNuevo },
                    ImporteTransferido = d.ImporteTransferido,
                    Itf = d.ITF,
                    CostoPorMovimiento = d.CostoPorMovimiento,
                    TotalDescontado = d.TotalDescontado
                };
            }
            return response;
        }

        // --- MÉTODOS DE MAPEO PARA JAVA (MANTENIDOS COMO FALLBACK) ---

        private TransactionResponse<DepositResponseData> MapJavaDepositResponse(JavaSoapTransaction.respuestaDTO r)
        {
            var response = new TransactionResponse<DepositResponseData> 
            { 
                Exitoso = r.exitoso, 
                Mensaje = r.mensaje, 
                CodigoError = r.codigoError 
            };

            // LOGGING PARA DEBUGGING
            System.Diagnostics.Debug.WriteLine($"=== JAVA SOAP RESPONSE DEBUG ===");
            System.Diagnostics.Debug.WriteLine($"Exitoso: {r.exitoso}");
            System.Diagnostics.Debug.WriteLine($"Mensaje: {r.mensaje}");
            System.Diagnostics.Debug.WriteLine($"CodigoError: {r.codigoError}");
            System.Diagnostics.Debug.WriteLine($"Datos es null: {r.datos == null}");

            if (r.datos != null)
            {
                try
                {
                    // Log del contenido XML sin procesar
                    System.Diagnostics.Debug.WriteLine($"Datos XML: {r.datos.OuterXml}");

                    // Intentar diferentes métodos de extracción
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(r.datos.OuterXml);

                    // Método 1: Buscar nodos directamente
                    var numeroMovimientoNode = xmlDoc.SelectSingleNode("//numeroMovimiento") ?? xmlDoc.SelectSingleNode("//NumeroMovimiento");
                    var saldoAnteriorNode = xmlDoc.SelectSingleNode("//saldoAnterior") ?? xmlDoc.SelectSingleNode("//SaldoAnterior");
                    var saldoNuevoNode = xmlDoc.SelectSingleNode("//saldoNuevo") ?? xmlDoc.SelectSingleNode("//SaldoNuevo");
                    var importeNode = xmlDoc.SelectSingleNode("//importeDepositado") ?? xmlDoc.SelectSingleNode("//ImporteDepositado") ?? xmlDoc.SelectSingleNode("//importe") ?? xmlDoc.SelectSingleNode("//Importe");

                    if (numeroMovimientoNode != null || saldoAnteriorNode != null)
                    {
                        response.Datos = new DepositResponseData
                        {
                            NumeroMovimiento = int.TryParse(numeroMovimientoNode?.InnerText, out int numMov) ? numMov : 0,
                            SaldoAnterior = decimal.TryParse(saldoAnteriorNode?.InnerText, out decimal saldoAnt) ? saldoAnt : 0,
                            SaldoNuevo = decimal.TryParse(saldoNuevoNode?.InnerText, out decimal saldoNvo) ? saldoNvo : 0,
                            Importe = decimal.TryParse(importeNode?.InnerText, out decimal imp) ? imp : 0
                        };

                        System.Diagnostics.Debug.WriteLine($"Extracción exitosa - NumMov: {response.Datos.NumeroMovimiento}, SaldoNuevo: {response.Datos.SaldoNuevo}");
                    }
                    else
                    {
                        // Método fallback: convertir XML a JSON como antes
                        string json = JsonConvert.SerializeXmlNode(r.datos);
                        System.Diagnostics.Debug.WriteLine($"JSON convertido: {json}");
                        
                        dynamic data = JsonConvert.DeserializeObject<dynamic>(json)!;

                        response.Datos = new DepositResponseData
                        {
                            NumeroMovimiento = (int?)data.numeroMovimiento ?? (int?)data.NumeroMovimiento ?? 0,
                            SaldoAnterior = (decimal?)data.saldoAnterior ?? (decimal?)data.SaldoAnterior ?? 0,
                            SaldoNuevo = (decimal?)data.saldoNuevo ?? (decimal?)data.SaldoNuevo ?? 0,
                            Importe = (decimal?)data.importeDepositado ?? (decimal?)data.ImporteDepositado ?? (decimal?)data.importe ?? (decimal?)data.Importe ?? 0
                        };
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en mapeo Java: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                    
                    // Si todo falla, al menos devolver respuesta exitosa sin datos
                    if (r.exitoso)
                    {
                        response.Datos = new DepositResponseData();
                    }
                }
            }
            else if (r.exitoso)
            {
                System.Diagnostics.Debug.WriteLine("Respuesta exitosa pero datos es null - creando objeto por defecto");
                response.Datos = new DepositResponseData();
            }

            return response;
        }

        private TransactionResponse<WithdrawResponseData> MapJavaWithdrawResponse(JavaSoapTransaction.respuestaDTO r)
        {
            var response = new TransactionResponse<WithdrawResponseData> { Exitoso = r.exitoso, Mensaje = r.mensaje, CodigoError = r.codigoError };
            
            if (r.exitoso && r.datos != null)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Retiro - Datos XML: {r.datos.OuterXml}");
                    
                    string json = JsonConvert.SerializeXmlNode(r.datos);
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(json)!;

                    response.Datos = new WithdrawResponseData
                    {
                        SaldoAnterior = (decimal?)data.saldoAnterior ?? 0,
                        SaldoNuevo = (decimal?)data.saldoNuevo ?? 0,
                        ImporteRetiro = (decimal?)data.importeRetirado ?? 0,
                        Itf = (decimal?)data.importeITF ?? 0,
                        CostoPorMovimiento = (decimal?)data.importeCargo ?? 0,
                        TotalDescontado = (decimal?)data.totalDescontado ?? 0
                    };
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en mapeo retiro Java: {ex.Message}");
                    if (r.exitoso) response.Datos = new WithdrawResponseData();
                }
            }
            else if (r.exitoso)
            {
                response.Datos = new WithdrawResponseData();
            }
            
            return response;
        }

        private TransactionResponse<TransferResponseData> MapJavaTransferResponse(JavaSoapTransaction.respuestaDTO r)
        {
            var response = new TransactionResponse<TransferResponseData> { Exitoso = r.exitoso, Mensaje = r.mensaje, CodigoError = r.codigoError };
            
            if (r.exitoso && r.datos != null)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Transferencia - Datos XML: {r.datos.OuterXml}");
                    
                    string json = JsonConvert.SerializeXmlNode(r.datos);
                    dynamic data = JsonConvert.DeserializeObject<dynamic>(json)!;

                    response.Datos = new TransferResponseData
                    {
                        CuentaOrigen = new AccountMovement { Codigo = data.cuentaOrigen, SaldoAnterior = (decimal?)data.saldoAnteriorOrigen ?? 0, SaldoNuevo = (decimal?)data.saldoNuevoOrigen ?? 0 },
                        CuentaDestino = new AccountMovement { Codigo = data.cuentaDestino, SaldoAnterior = (decimal?)data.saldoAnteriorDestino ?? 0, SaldoNuevo = (decimal?)data.saldoNuevoDestino ?? 0 },
                        ImporteTransferido = (decimal?)data.importeTransferido ?? 0,
                        Itf = 0,
                        CostoPorMovimiento = 0,
                        TotalDescontado = (decimal?)data.importeTransferido ?? 0
                    };
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error en mapeo transferencia Java: {ex.Message}");
                    if (r.exitoso) response.Datos = new TransferResponseData
                    {
                        CuentaOrigen = new AccountMovement(),
                        CuentaDestino = new AccountMovement(),
                        ImporteTransferido = 0,
                        Itf = 0,
                        CostoPorMovimiento = 0,
                        TotalDescontado = 0
                    };
                }
            }
            else if (r.exitoso)
            {
                response.Datos = new TransferResponseData
                {
                    CuentaOrigen = new AccountMovement(),
                    CuentaDestino = new AccountMovement(),
                    ImporteTransferido = 0,
                    Itf = 0,
                    CostoPorMovimiento = 0,
                    TotalDescontado = 0
                };
            }
            
            return response;
        }
    }
}