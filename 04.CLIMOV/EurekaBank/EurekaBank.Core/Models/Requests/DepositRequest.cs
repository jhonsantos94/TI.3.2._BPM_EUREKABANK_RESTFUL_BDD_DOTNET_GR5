// En: EurekaBank.Core/Models/Requests/DepositRequest.cs
using System.Text.Json.Serialization;

namespace EurekaBank.Core.Models.Requests
{
    public class DepositRequest
    {
        [JsonPropertyName("codigoCuenta")]
        public string CodigoCuenta { get; set; }

        [JsonPropertyName("claveCuenta")]
        public string ClaveCuenta { get; set; }

        [JsonPropertyName("importe")]
        public decimal Importe { get; set; }

        [JsonPropertyName("codigoEmpleado")]
        public string CodigoEmpleado { get; set; }

        // Este campo no se usa en Retiro, pero no afecta tenerlo
        [JsonPropertyName("codigoTipoMovimiento")]
        public string CodigoTipoMovimiento { get; set; }
    }
}