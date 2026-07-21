using CliUniversalConsole.Config;
using CliUniversalConsole.Models;
using System.Text;
using System.Xml.Linq;

namespace CliUniversalConsole.Services.Soap
{
    public class SoapJavaReporteService : IReporteService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public SoapJavaReporteService()
        {
            _httpClient = new HttpClient();
            _baseUrl = ServiceConfig.SoapJavaReporteUrl;
        }

        public async Task<List<MovimientoDetalle>> ObtenerMovimientosAsync(string codigoCuenta)
        {
            try
            {
                var soapEnvelope = $@"
<soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ws=""http://ws.monster.edu.ec/"">
   <soapenv:Header/>
   <soapenv:Body>
      <ws:obtenerMovimientos>
         <codigoCuenta>{codigoCuenta}</codigoCuenta>
      </ws:obtenerMovimientos>
   </soapenv:Body>
</soapenv:Envelope>";

                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");

                var response = await _httpClient.PostAsync(_baseUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(responseBody))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n❌ El servidor no respondió correctamente");
                    Console.ResetColor();
                    return new List<MovimientoDetalle>();
                }

                return ParseMovimientos(responseBody);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Error: {ex.Message}");
                Console.ResetColor();
                return new List<MovimientoDetalle>();
            }
        }

        private List<MovimientoDetalle> ParseMovimientos(string xmlResponse)
        {
            try
            {
                var xdoc = XDocument.Parse(xmlResponse);
                XNamespace ns2 = "http://ws.monster.edu.ec/";
                XNamespace soapNs = "http://schemas.xmlsoap.org/soap/envelope/";

                var movimientos = new List<MovimientoDetalle>();

                // Buscar el elemento return que contiene la lista
                var returnElements = xdoc.Descendants(ns2 + "obtenerMovimientosResponse")
                    .Descendants()
                    .Where(e => e.Name.LocalName == "return");

                foreach (var returnElement in returnElements)
                {
                    var movimiento = new MovimientoDetalle
                    {
                        CodigoCuenta = returnElement.Element("codigoCuenta")?.Value ?? "",
                        NumeroMovimiento = int.Parse(returnElement.Element("numeroMovimiento")?.Value ?? "0"),
                        Fecha = DateTime.Parse((returnElement.Element("fecha")?.Value ?? DateTime.Now.ToString()).Replace("[UTC]", "")),
                        TipoMovimiento = returnElement.Element("tipoMovimiento")?.Value ?? "",
                        EmpleadoNombre = returnElement.Element("empleadoNombre")?.Value ?? "",
                        CuentaReferencia = returnElement.Element("cuentaReferencia")?.Value,
                        Importe = decimal.Parse(returnElement.Element("importe")?.Value ?? "0")
                    };
                    movimientos.Add(movimiento);
                }

                return movimientos;
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Error al procesar respuesta: {ex.Message}");
                Console.ResetColor();
                return new List<MovimientoDetalle>();
            }
        }
    }
}
