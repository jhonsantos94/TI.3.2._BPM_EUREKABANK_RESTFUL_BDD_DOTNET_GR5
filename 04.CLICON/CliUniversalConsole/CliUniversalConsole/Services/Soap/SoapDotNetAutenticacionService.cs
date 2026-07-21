using CliUniversalConsole.Models;
using CliUniversalConsole.Config;
using System.Text;
using System.Xml.Linq;

namespace CliUniversalConsole.Services.Soap
{
    public class SoapDotNetAutenticacionService : IAutenticacionService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl = ServiceConfig.SoapDotNetBaseUrl;

        public SoapDotNetAutenticacionService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<LoginResult> LoginAsync(string usuario, string clave)
        {
            try
            {
                

                var soapEnvelope = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">
   <soap:Header/>
   <soap:Body>
      <tem:Login>
         <tem:usuario>{usuario}</tem:usuario>
         <tem:clave>{clave}</tem:clave>
      </tem:Login>
   </soap:Body>
</soap:Envelope>";

                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "\"http://tempuri.org/IServicioAutenticacion/Login\"");
                var response = await _httpClient.PostAsync(_baseUrl, content);
                var responseString = await response.Content.ReadAsStringAsync();

                return ParseSoapResponse(responseString);
            }
            catch (Exception ex)
            {
                return new LoginResult
                {
                    IsSuccess = false,
                    Message = $"Error al conectar con el servicio SOAP .NET: {ex.Message}"
                };
            }
        }

        private LoginResult ParseSoapResponse(string soapResponse)
        {
            try
            {
                var doc = XDocument.Parse(soapResponse);
                XNamespace tem = "http://tempuri.org/";
                XNamespace ns = "http://schemas.datacontract.org/2004/07/EurekaBank_Soap_DotNet_GR01";

                var loginResponse = doc.Descendants(tem + "LoginResponse").FirstOrDefault();
                if (loginResponse == null)
                {
                    loginResponse = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == "LoginResponse");
                }

                var result = new LoginResult();

                var loginResult = loginResponse?.Descendants().FirstOrDefault(e => e.Name.LocalName == "LoginResult");
                if (loginResult != null)
                {
                    var exitoso = loginResult.Descendants().FirstOrDefault(e => e.Name.LocalName == "Exitoso");
                    result.IsSuccess = exitoso?.Value.ToLower() == "true";

                    var mensaje = loginResult.Descendants().FirstOrDefault(e => e.Name.LocalName == "Mensaje");
                    result.Message = mensaje?.Value;

                    if (result.IsSuccess)
                    {
                        var datos = loginResult.Descendants().FirstOrDefault(e => e.Name.LocalName == "Datos");
                        if (datos != null)
                        {
                            result.EmpleadoInfo = new Empleado
                            {
                                Codigo = datos.Descendants().FirstOrDefault(e => e.Name.LocalName == "Codigo")?.Value,
                                Paterno = datos.Descendants().FirstOrDefault(e => e.Name.LocalName == "Paterno")?.Value,
                                Materno = datos.Descendants().FirstOrDefault(e => e.Name.LocalName == "Materno")?.Value,
                                Nombre = datos.Descendants().FirstOrDefault(e => e.Name.LocalName == "Nombre")?.Value,
                                NombreCompleto = datos.Descendants().FirstOrDefault(e => e.Name.LocalName == "NombreCompleto")?.Value,
                                Ciudad = datos.Descendants().FirstOrDefault(e => e.Name.LocalName == "Ciudad")?.Value,
                                Direccion = datos.Descendants().FirstOrDefault(e => e.Name.LocalName == "Direccion")?.Value,
                                Usuario = datos.Descendants().FirstOrDefault(e => e.Name.LocalName == "Usuario")?.Value
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
