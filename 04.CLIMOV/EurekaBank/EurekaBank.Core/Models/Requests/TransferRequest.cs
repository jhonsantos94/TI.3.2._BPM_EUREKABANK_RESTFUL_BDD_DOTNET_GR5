// En: EurekaBank.Core/Models/Requests/TransferRequest.cs
using System.Text.Json.Serialization;

namespace EurekaBank.Core.Models.Requests
{
    public class TransferRequest
    {
        [JsonPropertyName("cuentaOrigen")]
        public string CuentaOrigen { get; set; }

        [JsonPropertyName("cuentaDestino")]
        public string CuentaDestino { get; set; }

        [JsonPropertyName("claveCuentaOrigen")]
        public string ClaveCuentaOrigen { get; set; }

        [JsonPropertyName("importe")]
        public decimal Importe { get; set; }

        [JsonPropertyName("codigoEmpleado")]
        public string CodigoEmpleado { get; set; }
    }
}