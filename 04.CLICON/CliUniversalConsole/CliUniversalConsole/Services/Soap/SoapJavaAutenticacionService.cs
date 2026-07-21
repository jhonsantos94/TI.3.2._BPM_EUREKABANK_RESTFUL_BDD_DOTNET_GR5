using CliUniversalConsole.Models;
using CliUniversalConsole.Config;
using System.Text;
using System.Xml.Linq;

namespace CliUniversalConsole.Services.Soap
{
    public class SoapJavaAutenticacionService : IAutenticacionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = ServiceConfig.SoapJavaBaseUrl;

        public SoapJavaAutenticacionService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<LoginResult> LoginAsync(string usuario, string clave)
        {
            try
            {
                var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ws=""http://ws.monster.edu.ec/"">
   <soap:Header/>
   <soap:Body>
      <ws:login>
         <usuario>{usuario}</usuario>
         <clave>{clave}</clave>
      </ws:login>
   </soap:Body>
</soap:Envelope>";

                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "\"\"");
                var response = await _httpClient.PostAsync(_baseUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                return ParseSoapResponse(responseString);
            }
            catch (Exception ex)
            {
                return new LoginResult
                {
                    IsSuccess = false,
                    Message = $"Error al conectar con el servicio SOAP Java: {ex.Message}"
                };
            }
        }

        private LoginResult ParseSoapResponse(string soapResponse)
        {
            try
            {
                var doc = XDocument.Parse(soapResponse);
                XNamespace ns = "http://ws.monster.edu.ec/";
                XNamespace ns2 = "http://schemas.xmlsoap.org/soap/envelope/";

                var loginResponse = doc.Descendants(ns + "loginResponse").FirstOrDefault();
                if (loginResponse == null)
                {
                    loginResponse = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "loginResponse");
                }

                var result = new LoginResult();

                var returnElement = loginResponse?.Descendants().FirstOrDefault(e => e.Name.LocalName == "return");

                if (returnElement != null)
                {
                    // Los elementos dentro de <return> NO tienen namespace
                    var exitoso = returnElement.Descendants().FirstOrDefault(e => e.Name.LocalName == "exitoso");
                    result.IsSuccess = exitoso?.Value.ToLower() == "true";

                    var mensaje = returnElement.Descendants().FirstOrDefault(e => e.Name.LocalName == "mensaje");
                    result.Message = mensaje?.Value;

                    if (result.IsSuccess)
                    {
                        var datos = returnElement.Descendants().FirstOrDefault(e => e.Name.LocalName == "datos");

                        if (datos != null)
                        {
                            result.EmpleadoInfo = new Empleado
                            {
                                Codigo = datos.Descendants().FirstOrDefault(e => e.Name.LocalName == "codigo")?.Value,
                                Paterno = datos.Descendants().FirstOrDefault(e => e.Name.LocalName == "paterno")?.Value,
                                Materno = datos.Descendants().FirstOrDefault(e => e.Name.LocalName == "materno")?.Value,
                                Nombre = datos.Descendants().FirstOrDefault(e => e.Name.LocalName == "nombre")?.Value,
                                NombreCompleto = datos.Descendants().FirstOrDefault(e => e.Name.LocalName == "nombreCompleto")?.Value,
                                Ciudad = datos.Descendants().FirstOrDefault(e => e.Name.LocalName == "ciudad")?.Value,
                                Direccion = datos.Descendants().FirstOrDefault(e => e.Name.LocalName == "direccion")?.Value,
                                Usuario = datos.Descendants().FirstOrDefault(e => e.Name.LocalName == "usuario")?.Value
                            };
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                return new LoginResult
                {
                    IsSuccess = false,
                    Message = $"Error al parsear respuesta SOAP: {ex.Message}"
                };
            }
        }
    }
}
