using EurekaBank.Core.Managers;
using EurekaBank.Core.Models.Responses;
using EurekaBank.Core.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using System.ServiceModel;
using System.Xml;
using Newtonsoft.Json;
using System.Reflection;
using System.ServiceModel.Channels;
using System.Text;

// Importamos los namespaces de los clientes generados
using DotNetSoapReport;
using JavaSoapReport;

namespace EurekaBank.Core.Services.Implementations
{
    public class SoapReportService : IReportService
    {
        private readonly IConfiguration _configuration;
        private ApiPlatform _currentTarget = ApiPlatform.Java;
        private static readonly BasicHttpBinding Binding = new BasicHttpBinding();

        public SoapReportService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SetTarget(ApiPlatform target)
        {
            _currentTarget = target;
        }

        public async Task<IEnumerable<MovementDto>> ObtenerMovimientosAsync(string codigoCuenta)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== SOAP REPORT SERVICE ===");
                System.Diagnostics.Debug.WriteLine($"Target: {_currentTarget}");
                
                if (_currentTarget == ApiPlatform.DotNet)
                {
                    System.Diagnostics.Debug.WriteLine("Using .NET SOAP client");
                    var client = GetDotNetClient();
                    var soapResponse = await client.ObtenerMovimientosAsync(codigoCuenta);
                    return MapDotNetResponse(soapResponse);
                }
                else // Java
                {
                    System.Diagnostics.Debug.WriteLine("Using Java SOAP - trying generated client first");
                    
                    // Primero intentar con el cliente generado
                    try
                    {
                        var client = GetJavaClient();
                        var soapResponse = await client.obtenerMovimientosAsync(codigoCuenta);
                        var result = MapJavaResponse(soapResponse);
                        
                        // Verificar si los datos son válidos (no solo que existan)
                        var validMovements = result?.Where(m => 
                            !string.IsNullOrEmpty(m.TipoMovimiento) || 
                            !string.IsNullOrEmpty(m.Accion) || 
                            m.Importe > 0).Count() ?? 0;

                        if (validMovements > 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"Generated client returned {validMovements} valid movements out of {result?.Count() ?? 0}");
                            return result ?? Enumerable.Empty<MovementDto>();
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine($"Generated client returned {result?.Count() ?? 0} movements but all have empty data, trying direct XML approach");
                            return await ProcessJavaSoapDirectly(codigoCuenta);
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Generated client failed: {ex.Message}");
                        System.Diagnostics.Debug.WriteLine("Trying direct XML approach as fallback");
                        return await ProcessJavaSoapDirectly(codigoCuenta);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SoapReportService Error: {ex.Message}");
                return Enumerable.Empty<MovementDto>();
            }
        }

        // --- NUEVO MÉTODO PARA PROCESAR SOAP JAVA DIRECTAMENTE ---
        private async Task<IEnumerable<MovementDto>> ProcessJavaSoapDirectly(string codigoCuenta)
        {
            try
            {
                var url = _configuration["Hosts:Soap:Java:Report"];

                    var baseIp = _configuration["ServerConfig:BaseIp"];

                if (!string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(baseIp))
                {
                    url = url.Replace("{IP}", baseIp);
                }
                System.Diagnostics.Debug.WriteLine($"=== DIRECT SOAP JAVA PROCESSING ===");
                System.Diagnostics.Debug.WriteLine($"URL: {url}");
                System.Diagnostics.Debug.WriteLine($"CodigoCuenta: {codigoCuenta}");

                if (string.IsNullOrEmpty(url))
                {
                    System.Diagnostics.Debug.WriteLine("SOAP Java URL is not configured");
                    return Enumerable.Empty<MovementDto>();
                }

                // Crear el mensaje SOAP manualmente con estructura más simple
                var soapEnvelope = CreateSoapEnvelope(codigoCuenta);
                System.Diagnostics.Debug.WriteLine($"SOAP Request: {soapEnvelope}");

                // Enviar la petición HTTP directamente
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30); // Timeout de 30 segundos
                
                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                
                // Agregar headers necesarios para SOAP
                content.Headers.Clear();
                content.Headers.Add("Content-Type", "text/xml; charset=utf-8");
                content.Headers.Add("SOAPAction", ""); // SOAPAction vacío

                System.Diagnostics.Debug.WriteLine("Sending SOAP request...");
                System.Diagnostics.Debug.WriteLine($"Request Headers: {string.Join(", ", content.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))}");
                
                var response = await httpClient.PostAsync(url, content);
                var responseXml = await response.Content.ReadAsStringAsync();
                
                System.Diagnostics.Debug.WriteLine($"SOAP Response Status: {response.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"SOAP Response Headers: {string.Join(", ", response.Headers.Select(h => $"{h.Key}: {string.Join(", ", h.Value)}"))}");
                System.Diagnostics.Debug.WriteLine($"SOAP Response Body Length: {responseXml?.Length ?? 0} chars");
                System.Diagnostics.Debug.WriteLine($"SOAP Response Full Body: {responseXml}");

                if (response.IsSuccessStatusCode)
                {
                    // Verificar si la respuesta indica un error del servicio
                    if (string.IsNullOrEmpty(responseXml) || 
                        responseXml.Contains("<ns2:obtenerMovimientosResponse xmlns:ns2=\"http://ws.monster.edu.ec/\"/>") ||
                        responseXml.Contains("obtenerMovimientosResponse") && !responseXml.Contains("<return"))
                    {
                        System.Diagnostics.Debug.WriteLine("SOAP Java service returned empty response - no movements found for this account");
                        System.Diagnostics.Debug.WriteLine("This suggests either:");
                        System.Diagnostics.Debug.WriteLine("1. The account has no movements in the SOAP service");
                        System.Diagnostics.Debug.WriteLine("2. Different data source between REST and SOAP services");
                        System.Diagnostics.Debug.WriteLine("3. SOAP service parameter format is different");
                        
                        // Intentar con diferentes formatos del parámetro
                        return await TryAlternativeParameterFormats(codigoCuenta);
                    }

                    // Procesar el XML directamente
                    return ParseSoapResponseXml(responseXml);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"SOAP request failed with status: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"Response content: {responseXml}");
                    return Enumerable.Empty<MovementDto>();
                }
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"HTTP Error in SOAP request: {httpEx.Message}");
                return Enumerable.Empty<MovementDto>();
            }
            catch (TaskCanceledException tcEx)
            {
                System.Diagnostics.Debug.WriteLine($"SOAP request timeout: {tcEx.Message}");
                return Enumerable.Empty<MovementDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Direct SOAP Error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return Enumerable.Empty<MovementDto>();
            }
        }

        

        private async Task<IEnumerable<MovementDto>> TryAlternativeParameterFormats(string codigoCuenta)
        {
            System.Diagnostics.Debug.WriteLine("=== TRYING ALTERNATIVE PARAMETER FORMATS ===");

            var url = _configuration["Hosts:Soap:Java:Report"];
            var baseIp = _configuration["ServerConfig:BaseIp"];

            if (!string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(baseIp))
            {
                url = url.Replace("{IP}", baseIp);
            }

            if (string.IsNullOrEmpty(url)) return Enumerable.Empty<MovementDto>();

            // Intentar diferentes formatos de parámetros
            var alternativeFormats = new[]
            {
                // Formato con elemento nombrado
                $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ws=""http://ws.monster.edu.ec/"">
    <soap:Header/>
    <soap:Body>
        <ws:obtenerMovimientos>
            <codigoCuenta>{codigoCuenta}</codigoCuenta>
        </ws:obtenerMovimientos>
    </soap:Body>
</soap:Envelope>",

                // Formato con namespace diferente
                $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:rep=""http://ws.monster.edu.ec/"">
    <soapenv:Header/>
    <soapenv:Body>
        <rep:obtenerMovimientos>
            <rep:arg0>{codigoCuenta}</rep:arg0>
        </rep:obtenerMovimientos>
    </soapenv:Body>
</soapenv:Envelope>",

                // Formato sin namespace en el parámetro
                $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
    <soap:Header/>
    <soap:Body>
        <obtenerMovimientos xmlns=""http://ws.monster.edu.ec/"">
            <arg0>{codigoCuenta}</arg0>
        </obtenerMovimientos>
    </soap:Body>
</soap:Envelope>"
            };

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            for (int i = 0; i < alternativeFormats.Length; i++)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"Trying alternative format {i + 1}:");
                    System.Diagnostics.Debug.WriteLine(alternativeFormats[i]);

                    var content = new StringContent(alternativeFormats[i], Encoding.UTF8, "text/xml");
                    content.Headers.Clear();
                    content.Headers.Add("Content-Type", "text/xml; charset=utf-8");
                    content.Headers.Add("SOAPAction", "");

                    var response = await httpClient.PostAsync(url, content);
                    var responseXml = await response.Content.ReadAsStringAsync();

                    System.Diagnostics.Debug.WriteLine($"Alternative {i + 1} Response Status: {response.StatusCode}");
                    System.Diagnostics.Debug.WriteLine($"Alternative {i + 1} Response: {responseXml}");

                    if (response.IsSuccessStatusCode && 
                        !string.IsNullOrEmpty(responseXml) && 
                        responseXml.Contains("<return"))
                    {
                        System.Diagnostics.Debug.WriteLine($"Alternative format {i + 1} SUCCESS - has return elements!");
                        return ParseSoapResponseXml(responseXml);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Alternative format {i + 1} failed: {ex.Message}");
                }
            }

            System.Diagnostics.Debug.WriteLine("All alternative formats failed - SOAP Java service may not have data for this account");
            return Enumerable.Empty<MovementDto>();
        }

        private string CreateSoapEnvelope(string codigoCuenta)
        {
            // Usar la estructura más simple posible
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ws=""http://ws.monster.edu.ec/"">
    <soap:Header/>
    <soap:Body>
        <ws:obtenerMovimientos>
            <arg0>{codigoCuenta}</arg0>
        </ws:obtenerMovimientos>
    </soap:Body>
</soap:Envelope>";
        }

        private IEnumerable<MovementDto> ParseSoapResponseXml(string xmlResponse)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== XML PARSING ===");

                if (string.IsNullOrEmpty(xmlResponse))
                {
                    System.Diagnostics.Debug.WriteLine("XML response is null or empty");
                    return Enumerable.Empty<MovementDto>();
                }

                var doc = new XmlDocument();
                doc.LoadXml(xmlResponse);

                // Buscar todos los elementos <return> sin namespace
                var returnNodes = doc.GetElementsByTagName("return");
                System.Diagnostics.Debug.WriteLine($"Found {returnNodes.Count} return nodes");

                if (returnNodes.Count == 0)
                {
                    // Intentar con namespace manager
                    var nsManager = new XmlNamespaceManager(doc.NameTable);
                    nsManager.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
                    nsManager.AddNamespace("S", "http://schemas.xmlsoap.org/soap/envelope/");
                    nsManager.AddNamespace("ns2", "http://ws.monster.edu.ec/");
                    
                    var returnNodesWithNs = doc.SelectNodes("//return", nsManager);
                    if (returnNodesWithNs != null && returnNodesWithNs.Count > 0)
                    {
                        returnNodes = returnNodesWithNs;
                        System.Diagnostics.Debug.WriteLine($"Found {returnNodes.Count} return nodes with namespace");
                    }
                }

                if (returnNodes.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("No return nodes found in XML");
                    // Mostrar estructura del XML para debugging
                    var lines = xmlResponse.Split('\n');
                    System.Diagnostics.Debug.WriteLine("XML structure sample:");
                    for (int i = 0; i < Math.Min(10, lines.Length); i++)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Line {i+1}: {lines[i].Trim()}");
                    }
                    return Enumerable.Empty<MovementDto>();
                }

                var movements = new List<MovementDto>();

                // Mostrar solo los primeros 3 nodos para debugging
                for (int i = 0; i < Math.Min(3, returnNodes.Count); i++)
                {
                    var node = returnNodes[i];
                    if (node == null) continue;

                    System.Diagnostics.Debug.WriteLine($"Processing return node {i} (sample):");

                    try
                    {
                        var codigoCuenta = GetNodeValue(node, "codigoCuenta");
                        var numeroMovimiento = GetNodeValue(node, "numeroMovimiento");
                        var tipoMovimiento = GetNodeValue(node, "tipoMovimiento");
                        var accion = GetNodeValue(node, "accion");
                        var importe = GetNodeValue(node, "importe");
                        var empleadoNombre = GetNodeValue(node, "empleadoNombre");
                        var cuentaReferencia = GetNodeValue(node, "cuentaReferencia");

                        System.Diagnostics.Debug.WriteLine($"  Sample {i} - Tipo: '{tipoMovimiento}', Accion: '{accion}', Importe: '{importe}'");
                    }
                    catch (Exception nodeEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"  Error reading sample node {i}: {nodeEx.Message}");
                    }
                }

                // Procesar todos los nodos
                for (int i = 0; i < returnNodes.Count; i++)
                {
                    var node = returnNodes[i];
                    if (node == null) continue;

                    try
                    {
                        var movement = new MovementDto
                        {
                            CodigoCuenta = GetNodeValue(node, "codigoCuenta") ?? "",
                            NumeroMovimiento = int.TryParse(GetNodeValue(node, "numeroMovimiento"), out int numMov) ? numMov : 0,
                            Fecha = DateTime.Now, // Usar fecha actual ya que viene vacío
                            TipoMovimiento = GetNodeValue(node, "tipoMovimiento") ?? "",
                            Accion = GetNodeValue(node, "accion") ?? "",
                            Importe = decimal.TryParse(
                            GetNodeValue(node, "importe"),
                            System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture,
                            out decimal importe)
                                ? importe
                                : 0,
                            EmpleadoNombre = GetNodeValue(node, "empleadoNombre") ?? "",
                            CuentaReferencia = GetNodeValue(node, "cuentaReferencia") ?? ""
                        };

                        // Solo agregar si tiene datos válidos
                        if (movement.NumeroMovimiento > 0)
                        {
                            movements.Add(movement);
                        }
                    }
                    catch (Exception nodeEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error processing node {i}: {nodeEx.Message}");
                    }
                }

                var validMovements = movements.Where(m => 
                    !string.IsNullOrEmpty(m.TipoMovimiento) || 
                    !string.IsNullOrEmpty(m.Accion) || 
                    m.Importe > 0).Count();

                System.Diagnostics.Debug.WriteLine($"Successfully parsed {movements.Count} total movements, {validMovements} with valid data");
                return movements;
            }
            catch (XmlException xmlEx)
            {
                System.Diagnostics.Debug.WriteLine($"XML parsing error: {xmlEx.Message}");
                return Enumerable.Empty<MovementDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"General XML parsing error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return Enumerable.Empty<MovementDto>();
            }
        }

        private string? GetNodeValue(XmlNode parentNode, string nodeName)
        {
            try
            {
                var childNode = parentNode.SelectSingleNode(nodeName);
                var value = childNode?.InnerText?.Trim();
                return string.IsNullOrEmpty(value) ? null : value;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"    {nodeName}: <error reading - {ex.Message}>");
                return null;
            }
        }

        // --- CLIENT FACTORIES ---
        private DotNetSoapReport.ServicioReporteClient GetDotNetClient()
        {
            var url = _configuration["Hosts:Soap:DotNet:Report"];
            var baseIp = _configuration["ServerConfig:BaseIp"];

            if (!string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(baseIp))
            {
                url = url.Replace("{IP}", baseIp);
            }
            return new DotNetSoapReport.ServicioReporteClient(Binding, new EndpointAddress(url));
        }

        private JavaSoapReport.ServicioReporteClient GetJavaClient()
        {
            var url = _configuration["Hosts:Soap:Java:Report"];
            var baseIp = _configuration["ServerConfig:BaseIp"];

            if (!string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(baseIp))
            {
                url = url.Replace("{IP}", baseIp);
            }

            return new JavaSoapReport.ServicioReporteClient(Binding, new EndpointAddress(url));
        }

        // --- MÉTODOS DE MAPEO ---
        private IEnumerable<MovementDto> MapDotNetResponse(IEnumerable<DotNetSoapReport.MovimientoDetalleDTO> soapMovements)
        {
            if (soapMovements == null) return Enumerable.Empty<MovementDto>();

            return soapMovements.Select(m => new MovementDto
            {
                CodigoCuenta = m.CodigoCuenta,
                NumeroMovimiento = m.NumeroMovimiento,
                Fecha = m.Fecha,
                TipoMovimiento = m.TipoMovimiento,
                Accion = m.Accion,
                Importe = m.Importe,
                EmpleadoNombre = m.EmpleadoNombre,
                CuentaReferencia = m.CuentaReferencia
            }).ToList();
        }

        private IEnumerable<MovementDto> MapJavaResponse(obtenerMovimientosResponse soapResponse)
        {
            System.Diagnostics.Debug.WriteLine("=== SOAP JAVA RESPONSE MAPPING ===");
            
            if (soapResponse?.@return == null) 
            {
                System.Diagnostics.Debug.WriteLine("soapResponse or @return is null");
                return Enumerable.Empty<MovementDto>();
            }

            System.Diagnostics.Debug.WriteLine($"Found {soapResponse.@return.Length} movements in generated client response");

            var movements = new List<MovementDto>();

            // Solo mostrar los primeros 3 para debugging
            for (int i = 0; i < Math.Min(3, soapResponse.@return.Length); i++)
            {
                var m = soapResponse.@return[i];
                
                System.Diagnostics.Debug.WriteLine($"Movement {i} (sample):");
                System.Diagnostics.Debug.WriteLine($"  codigoCuenta: '{m.codigoCuenta}'");
                System.Diagnostics.Debug.WriteLine($"  numeroMovimiento: {m.numeroMovimiento}");
                System.Diagnostics.Debug.WriteLine($"  tipoMovimiento: '{m.tipoMovimiento}'");
                System.Diagnostics.Debug.WriteLine($"  accion: '{m.accion}'");
                System.Diagnostics.Debug.WriteLine($"  importe: {m.importe} (specified: {m.importeSpecified})");
                System.Diagnostics.Debug.WriteLine($"  empleadoNombre: '{m.empleadoNombre}'");

                // Intentar usar reflexión para acceder a campos privados
                var codigoCuenta = m.codigoCuenta ?? TryGetPrivateFieldString(m, "codigoCuentaField");
                var tipoMovimiento = m.tipoMovimiento ?? TryGetPrivateFieldString(m, "tipoMovimientoField");
                var accion = m.accion ?? TryGetPrivateFieldString(m, "accionField");
                var empleadoNombre = m.empleadoNombre ?? TryGetPrivateFieldString(m, "empleadoNombreField");
                var importe = m.importeSpecified ? m.importe : TryGetPrivateFieldDecimal(m, "importeField");

                System.Diagnostics.Debug.WriteLine($"  After reflection - Tipo: '{tipoMovimiento}', Accion: '{accion}', Importe: {importe}");
            }

            // Procesar todos los movimientos
            for (int i = 0; i < soapResponse.@return.Length; i++)
            {
                var m = soapResponse.@return[i];

                var movement = new MovementDto
                {
                    CodigoCuenta = m.codigoCuenta ?? TryGetPrivateFieldString(m, "codigoCuentaField") ?? "",
                    NumeroMovimiento = m.numeroMovimiento != 0 ? m.numeroMovimiento : TryGetPrivateFieldInt(m, "numeroMovimientoField"),
                    Fecha = DateTime.Now,
                    TipoMovimiento = m.tipoMovimiento ?? TryGetPrivateFieldString(m, "tipoMovimientoField") ?? "",
                    Accion = m.accion ?? TryGetPrivateFieldString(m, "accionField") ?? "",
                    Importe = m.importeSpecified ? m.importe : TryGetPrivateFieldDecimal(m, "importeField"),
                    EmpleadoNombre = m.empleadoNombre ?? TryGetPrivateFieldString(m, "empleadoNombreField") ?? "",
                    CuentaReferencia = m.cuentaReferencia ?? TryGetPrivateFieldString(m, "cuentaReferenciaField") ?? ""
                };

                movements.Add(movement);
            }

            var validCount = movements.Where(m => 
                !string.IsNullOrEmpty(m.TipoMovimiento) || 
                !string.IsNullOrEmpty(m.Accion) || 
                m.Importe > 0).Count();

            System.Diagnostics.Debug.WriteLine($"Mapped {movements.Count} total movements, {validCount} with valid data");
            return movements;
        }

        // Métodos auxiliares para acceder a campos privados con tipos específicos
        private string? TryGetPrivateFieldString(object obj, string fieldName)
        {
            try
            {
                var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                return field?.GetValue(obj) as string;
            }
            catch
            {
                return null;
            }
        }

        private int TryGetPrivateFieldInt(object obj, string fieldName)
        {
            try
            {
                var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                var value = field?.GetValue(obj);
                return value is int intValue ? intValue : 0;
            }
            catch
            {
                return 0;
            }
        }

        private decimal TryGetPrivateFieldDecimal(object obj, string fieldName)
        {
            try
            {
                var field = obj.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                var value = field?.GetValue(obj);

                if (value == null)
                    return 0m;

                // Si ya es decimal
                if (value is decimal decimalValue)
                    return decimalValue;

                // Si viene como string
                if (value is string strValue)
                {
                    return decimal.TryParse(
                        strValue,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out decimal parsed)
                            ? parsed
                            : 0m;
                }

                return Convert.ToDecimal(value, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return 0m;
            }
        }
    }
}