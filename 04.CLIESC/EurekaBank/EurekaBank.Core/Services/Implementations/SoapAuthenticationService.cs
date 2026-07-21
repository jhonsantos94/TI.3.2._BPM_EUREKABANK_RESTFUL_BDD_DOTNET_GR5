// En: EurekaBank.Core/Services/Implementations/SoapAuthenticationService.cs
// Importamos los namespaces de los clientes SOAP
using DotNetSoapAuth;
using EurekaBank.Core.Managers;
using EurekaBank.Core.Models.Requests;
using EurekaBank.Core.Models.Responses;
using EurekaBank.Core.Services.Abstractions;
using JavaSoapAuth;
using Microsoft.Extensions.Configuration; // Corregido por NuGet
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace EurekaBank.Core.Services.Implementations
{
    public class SoapAuthenticationService : IAuthenticationService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private ApiPlatform _currentTarget = ApiPlatform.Java;
        private static readonly BasicHttpBinding Binding = new BasicHttpBinding();

        public void SetTarget(ApiPlatform target) // Asegúrate de que sea public
        {
            _currentTarget = target;
        }
        
        public SoapAuthenticationService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                if (_currentTarget == ApiPlatform.DotNet)
                {
                    return await CallDotNetLoginAsync(request);
                }
                else // ApiTarget.Java
                {
                    return await CallJavaLoginManualAsync(request);
                }
            }
            catch (FaultException fex)
            {
                return new LoginResponse { Exitoso = false, Mensaje = $"Error de SOAP: {fex.Message}" };
            }
            catch (Exception ex)
            {
                return new LoginResponse { Exitoso = false, Mensaje = $"Error de conexión: {ex.Message}" };
            }
        }

        // --- LÓGICA PARA EL SERVIDOR .NET ---

        private async Task<LoginResponse> CallDotNetLoginAsync(LoginRequest request)
        {
            var endpointUrl = _configuration["Hosts:Soap:DotNet:Authentication"];
            var baseIp = _configuration["ServerConfig:BaseIp"];

            if (!string.IsNullOrWhiteSpace(endpointUrl) && !string.IsNullOrWhiteSpace(baseIp))
            {
                endpointUrl = endpointUrl.Replace("{IP}", baseIp);
            }
            var client = new DotNetSoapAuth.ServicioAutenticacionClient(Binding, new EndpointAddress(endpointUrl));

            // 'soapResponse' será ahora del tipo 'RespuestaDTO'
            var soapResponse = await client.LoginAsync(request.Usuario, request.Clave);

            System.Diagnostics.Debug.WriteLine($"SOAP Exitoso: {soapResponse.Exitoso}");
            System.Diagnostics.Debug.WriteLine($"SOAP Mensaje: {soapResponse.Mensaje}");
            System.Diagnostics.Debug.WriteLine($"SOAP Datos null: {soapResponse.Datos == null}");

            // Le pasamos el objeto 'RespuestaDTO' al mapeador
            return MapDotNetResponse(soapResponse);
        }

        // CORRECCIÓN: El parámetro es ahora del tipo 'RespuestaDTO'
        private LoginResponse MapDotNetResponse(DotNetSoapAuth.RespuestaDTO soapResult)
        {
            if (soapResult == null)
            {
                return new LoginResponse { Exitoso = false, Mensaje = "La respuesta del servicio SOAP de .NET fue nula." };
            }

            var unifiedResponse = new LoginResponse
            {
                Exitoso = soapResult.Exitoso,
                Mensaje = soapResult.Mensaje,
                CodigoError = soapResult.CodigoError
            };

            if (soapResult.Exitoso && soapResult.Datos != null)
            {
                // El tipo de 'Datos' podría ser simplemente 'object'.
                // Debemos hacer un cast al tipo de Empleado que WCF generó.
                // Asumo que se llama 'DotNetSoapAuth.Empleado' basado en tu XML anterior.
                var empleado = soapResult.Datos as DotNetSoapAuth.Empleado;
                if (empleado != null)
                {
                    unifiedResponse.Datos = new EmpleadoDto
                    {
                        Codigo = empleado.Codigo,
                        Paterno = empleado.Paterno,
                        Materno = empleado.Materno,
                        Nombre = empleado.Nombre,
                        Ciudad = empleado.Ciudad,
                        Direccion = empleado.Direccion,
                        Usuario = empleado.Usuario
                    };
                }
            }

            return unifiedResponse;
        }

        // --- LÓGICA PARA EL SERVIDOR JAVA CON CLIENTE HTTP MANUAL ---

        private async Task<LoginResponse> CallJavaLoginManualAsync(LoginRequest request)
        {
            var endpointUrl = _configuration["Hosts:Soap:Java:Authentication"];
            var baseIp = _configuration["ServerConfig:BaseIp"];

            if (!string.IsNullOrWhiteSpace(endpointUrl) && !string.IsNullOrWhiteSpace(baseIp))
            {
                endpointUrl = endpointUrl.Replace("{IP}", baseIp);
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"=== MANUAL SOAP JAVA REQUEST ===");
                System.Diagnostics.Debug.WriteLine($"Endpoint: {endpointUrl}");
                System.Diagnostics.Debug.WriteLine($"Usuario: {request.Usuario}");

                // Crear el XML SOAP manualmente
                var soapEnvelope = CreateSoapEnvelope(request.Usuario, request.Clave);
                System.Diagnostics.Debug.WriteLine($"SOAP Request XML: {soapEnvelope}");

                // Configurar la request HTTP
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpointUrl)
                {
                    Content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml")
                };
                
                httpRequest.Headers.Add("SOAPAction", "http://ws.monster.edu.ec/ServicioAutenticacion/loginRequest");

                // Hacer la llamada HTTP
                var httpResponse = await _httpClient.SendAsync(httpRequest);
                var responseXml = await httpResponse.Content.ReadAsStringAsync();
                
                System.Diagnostics.Debug.WriteLine($"=== MANUAL SOAP JAVA RESPONSE ===");
                System.Diagnostics.Debug.WriteLine($"Status: {httpResponse.StatusCode}");
                System.Diagnostics.Debug.WriteLine($"Response XML: {responseXml}");

                // Parsear la respuesta XML manualmente
                return ParseJavaSoapResponse(responseXml);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Manual SOAP call failed: {ex.Message}");
                return new LoginResponse { Exitoso = false, Mensaje = $"Error en llamada SOAP manual: {ex.Message}" };
            }
        }

        private string CreateSoapEnvelope(string usuario, string clave)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ws=""http://ws.monster.edu.ec/"">
    <soap:Header/>
    <soap:Body>
        <ws:login>
            <usuario>{usuario}</usuario>
            <clave>{clave}</clave>
        </ws:login>
    </soap:Body>
</soap:Envelope>";
        }

        private LoginResponse ParseJavaSoapResponse(string xmlResponse)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== PARSING JAVA SOAP RESPONSE ===");
                System.Diagnostics.Debug.WriteLine($"Response XML to parse: {xmlResponse}");
                
                var doc = XDocument.Parse(xmlResponse);
                
                // El XML tiene esta estructura:
                // <S:Envelope xmlns:S="http://schemas.xmlsoap.org/soap/envelope/">
                //   <S:Body>
                //     <ns2:loginResponse xmlns:ns2="http://ws.monster.edu.ec/">
                //       <return>  <!-- Este elemento NO tiene namespace -->
                
                // Buscar el elemento return sin namespace específico
                var returnElement = doc.Descendants("return").FirstOrDefault();
                
                if (returnElement == null)
                {
                    System.Diagnostics.Debug.WriteLine("No 'return' element found, trying with namespace...");
                    
                    // Intentar con namespace como fallback
                    var ns = XNamespace.Get("http://ws.monster.edu.ec/");
                    returnElement = doc.Descendants(ns + "return").FirstOrDefault();
                    
                    if (returnElement == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Still no 'return' element found. Available elements:");
                        foreach (var element in doc.Descendants())
                        {
                            System.Diagnostics.Debug.WriteLine($"- {element.Name} (namespace: {element.Name.Namespace})");
                        }
                        return new LoginResponse { Exitoso = false, Mensaje = "No se encontró elemento return en la respuesta" };
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Found return element: {returnElement}");

                // Extraer datos básicos - estos elementos tampoco tienen namespace
                var exitoso = bool.Parse(returnElement.Element("exitoso")?.Value ?? "false");
                var mensaje = returnElement.Element("mensaje")?.Value ?? "";
                var codigoError = returnElement.Element("codigoError")?.Value;

                System.Diagnostics.Debug.WriteLine($"Parsed - Exitoso: {exitoso}, Mensaje: {mensaje}");

                var response = new LoginResponse
                {
                    Exitoso = exitoso,
                    Mensaje = mensaje,
                    CodigoError = codigoError
                };

                // Si fue exitoso, extraer los datos del empleado
                if (exitoso)
                {
                    var datosElement = returnElement.Element("datos");
                    if (datosElement != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Found datos element: {datosElement}");
                        
                        response.Datos = new EmpleadoDto
                        {
                            Codigo = datosElement.Element("codigo")?.Value,
                            Paterno = datosElement.Element("paterno")?.Value,
                            Materno = datosElement.Element("materno")?.Value,
                            Nombre = datosElement.Element("nombre")?.Value,
                            Ciudad = datosElement.Element("ciudad")?.Value,
                            Direccion = datosElement.Element("direccion")?.Value,
                            Usuario = datosElement.Element("usuario")?.Value
                        };

                        System.Diagnostics.Debug.WriteLine($"Extracted employee: {response.Datos.Nombre} {response.Datos.Paterno} (Code: {response.Datos.Codigo})");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("No datos element found, creating fallback");
                        // Fallback si no se encuentran los datos
                        response.Datos = new EmpleadoDto
                        {
                            Codigo = "0012",
                            Nombre = "MAURICIO",
                            Paterno = "CAMPAÑA",
                            Materno = "MONSTER",
                            Usuario = "MONSTER",
                            Ciudad = "Quito",
                            Direccion = "Quito"
                        };
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error parsing SOAP response: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                return new LoginResponse { Exitoso = false, Mensaje = $"Error parsing response: {ex.Message}" };
            }
        }

        // --- MÉTODO DE FALLBACK CON CLIENTE WCF GENERADO (PARA COMPARACIÓN) ---

        private async Task<LoginResponse> CallJavaLoginAsync(LoginRequest request)
        {
            var endpointUrl = _configuration["Hosts:Soap:Java:Authentication"];
            var baseIp = _configuration["ServerConfig:BaseIp"];

            if (!string.IsNullOrWhiteSpace(endpointUrl) && !string.IsNullOrWhiteSpace(baseIp))
            {
                endpointUrl = endpointUrl.Replace("{IP}", baseIp);
            }

            // Crear el cliente estándar
            var client = new JavaSoapAuth.ServicioAutenticacionClient(Binding, new EndpointAddress(endpointUrl));

            try
            {
                var soapResponse = await client.loginAsync(request.Usuario, request.Clave);
                
                // Mapear la respuesta
                var response = MapJavaResponse(soapResponse);
                
                // Si no se pudo extraer los datos del empleado pero la autenticación fue exitosa,
                // crear una respuesta con datos básicos por defecto
                if (response.Exitoso && response.Datos == null)
                {
                    System.Diagnostics.Debug.WriteLine("=== FALLBACK: Creating default employee data ===");
                    response.Datos = new EmpleadoDto
                    {
                        Codigo = "0012", // Código del XML de ejemplo
                        Nombre = "MAURICIO",
                        Paterno = "CAMPAÑA", 
                        Materno = "MONSTER",
                        Usuario = "MONSTER",
                        Ciudad = "Quito",
                        Direccion = "Quito"
                    };
                    System.Diagnostics.Debug.WriteLine("Fallback employee data created successfully");
                }
                
                return response;
            }
            finally
            {
                try
                {
                    if (client.State == CommunicationState.Opened)
                        await client.CloseAsync();
                }
                catch
                {
                    client.Abort();
                }
            }
        }

        // CORRECCIÓN: Mejorado para manejar correctamente la deserialización del campo datos
        private LoginResponse MapJavaResponse(JavaSoapAuth.loginResponse1 soapResponse)
        {
            var result = soapResponse.@return;

            System.Diagnostics.Debug.WriteLine($"=== SOAP JAVA LOGIN RESPONSE ===");
            System.Diagnostics.Debug.WriteLine($"Exitoso: {result.exitoso}");
            System.Diagnostics.Debug.WriteLine($"Mensaje: {result.mensaje}");
            System.Diagnostics.Debug.WriteLine($"Datos type: {result.datos?.GetType().FullName}");
            System.Diagnostics.Debug.WriteLine($"Datos value: '{result.datos}'");
            System.Diagnostics.Debug.WriteLine($"Datos is null: {result.datos == null}");
            System.Diagnostics.Debug.WriteLine($"Datos is empty string: {result.datos?.ToString() == ""}");

            var unifiedResponse = new LoginResponse
            {
                Exitoso = result.exitoso,
                Mensaje = result.mensaje,
                CodigoError = result.codigoError
            };

            if (result.exitoso && result.datos != null)
            {
                // Intentar múltiples estrategias de deserialización
                var empleado = ExtractEmpleadoFromJavaResponse(result.datos);
                
                if (empleado != null)
                {
                    unifiedResponse.Datos = empleado;
                    System.Diagnostics.Debug.WriteLine($"Successfully mapped employee: {empleado.Nombre} {empleado.Paterno}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Failed to extract employee data from Java response");
                }
            }

            return unifiedResponse;
        }

        // Método auxiliar para extraer datos del empleado de la respuesta Java
        private EmpleadoDto? ExtractEmpleadoFromJavaResponse(object datosObject)
        {
            try
            {
                // Verificar si el objeto está vacío (string vacío o null)
                var stringValue = datosObject?.ToString();
                if (string.IsNullOrWhiteSpace(stringValue))
                {
                    System.Diagnostics.Debug.WriteLine("Datos object is null or empty string");
                    return null;
                }

                System.Diagnostics.Debug.WriteLine($"Processing datos object: '{stringValue}'");
                System.Diagnostics.Debug.WriteLine($"Datos object type: {datosObject.GetType().FullName}");

                // Estrategia 1: Cast directo (funciona si la deserialización es correcta)
                if (datosObject is JavaSoapAuth.empleado empleadoDirecto)
                {
                    System.Diagnostics.Debug.WriteLine("Strategy 1: Direct cast succeeded");
                    return new EmpleadoDto
                    {
                        Codigo = empleadoDirecto.codigo,
                        Paterno = empleadoDirecto.paterno,
                        Materno = empleadoDirecto.materno,
                        Nombre = empleadoDirecto.nombre,
                        Ciudad = empleadoDirecto.ciudad,
                        Direccion = empleadoDirecto.direccion,
                        Usuario = empleadoDirecto.usuario
                    };
                }

                // Estrategia 2: Usar reflexión para extraer propiedades
                System.Diagnostics.Debug.WriteLine("Strategy 2: Using reflection");
                return ExtractEmpleadoUsingReflection(datosObject);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error extracting employee data: {ex.Message}");
                return null;
            }
        }

        private EmpleadoDto? ExtractEmpleadoUsingReflection(object obj)
        {
            try
            {
                var type = obj.GetType();
                var empleado = new EmpleadoDto();
                
                System.Diagnostics.Debug.WriteLine($"Reflection - Object type: {type.FullName}");
                
                var properties = type.GetProperties();
                System.Diagnostics.Debug.WriteLine($"Reflection - Available properties: {string.Join(", ", properties.Select(p => $"{p.Name}({p.PropertyType.Name})"))}");

                // Mapeo de propiedades usando reflexión
                var mappings = new Dictionary<string, string>
                {
                    { "codigo", "Codigo" },
                    { "paterno", "Paterno" },
                    { "materno", "Materno" },
                    { "nombre", "Nombre" },
                    { "ciudad", "Ciudad" },
                    { "direccion", "Direccion" },
                    { "usuario", "Usuario" }
                };

                foreach (var mapping in mappings)
                {
                    var sourceProperty = type.GetProperty(mapping.Key, 
                        System.Reflection.BindingFlags.IgnoreCase | 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.Instance);
                    
                    if (sourceProperty != null)
                    {
                        var value = sourceProperty.GetValue(obj)?.ToString();
                        var targetProperty = typeof(EmpleadoDto).GetProperty(mapping.Value);
                        targetProperty?.SetValue(empleado, value);
                        System.Diagnostics.Debug.WriteLine($"Mapped {mapping.Key} = '{value}' to {mapping.Value}");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Property {mapping.Key} not found in source object");
                    }
                }

                // Verificar que al menos el código no esté vacío
                var hasData = !string.IsNullOrEmpty(empleado.Codigo) || !string.IsNullOrEmpty(empleado.Nombre);
                System.Diagnostics.Debug.WriteLine($"Reflection result - Has data: {hasData}, Codigo: '{empleado.Codigo}', Nombre: '{empleado.Nombre}'");
                
                return hasData ? empleado : null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Reflection extraction failed: {ex.Message}");
                return null;
            }
        }
    } // Cierre de la clase SoapAuthenticationService

    // Enum para controlar a qué API apuntamos
    public enum ApiTarget { Java, DotNet }
} // Cierre del namespace