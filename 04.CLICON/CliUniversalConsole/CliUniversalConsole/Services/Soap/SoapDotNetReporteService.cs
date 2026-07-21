using CliUniversalConsole.Config;
using CliUniversalConsole.Models;
using System.Text;
using System.Xml.Linq;

namespace CliUniversalConsole.Services.Soap
{
    public class SoapDotNetReporteService : IReporteService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public SoapDotNetReporteService()
        {
            _httpClient = new HttpClient();
            _baseUrl = ServiceConfig.SoapDotNetReporteUrl;
        }

        public async Task<List<MovimientoDetalle>> ObtenerMovimientosAsync(string codigoCuenta)
        {
            try
            {
                var soapEnvelope = $@"
<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:tem=""http://tempuri.org/"">
   <soap:Header/>
   <soap:Body>
      <tem:ObtenerMovimientos>
         <tem:codigoCuenta>{codigoCuenta}</tem:codigoCuenta>
      </tem:ObtenerMovimientos>
   </soap:Body>
</soap:Envelope>";

                var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "http://tempuri.org/IServicioReporte/ObtenerMovimientos");

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
                XNamespace tem = "http://tempuri.org/";
                XNamespace a = "http://schemas.datacontract.org/2004/07/EurekaBank_Soap_DotNet_GR01.Models.DTOs";

                var movimientos = new List<MovimientoDetalle>();

                var movimientoElements = xdoc.Descendants(tem + "ObtenerMovimientosResponse")
                    .Descendants(tem + "ObtenerMovimientosResult")
                    .Descendants(a + "MovimientoDetalleDTO");

                foreach (var element in movimientoElements)
                {
                    var movimiento = new MovimientoDetalle
                    {
                        CodigoCuenta = element.Element(a + "CodigoCuenta")?.Value ?? "",
                        NumeroMovimiento = int.Parse(element.Element(a + "NumeroMovimiento")?.Value ?? "0"),
                        Fecha = DateTime.Parse((element.Element(a + "Fecha")?.Value ?? DateTime.Now.ToString()).Replace("[UTC]", "")),
                        TipoMovimiento = element.Element(a + "TipoMovimiento")?.Value ?? "",
                        EmpleadoNombre = element.Element(a + "EmpleadoNombre")?.Value ?? "",
                        CuentaReferencia = element.Element(a + "CuentaReferencia")?.Value,
                        Importe = decimal.Parse(element.Element(a + "Importe")?.Value ?? "0")
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
