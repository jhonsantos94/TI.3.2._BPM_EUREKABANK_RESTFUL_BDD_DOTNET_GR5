// En: EurekaBank.Core/Models/Responses/DepositResponseData.cs
using System.Text.Json.Serialization;

namespace EurekaBank.Core.Models.Responses
{
    public class DepositResponseData
    {
        [JsonPropertyName("numeroMovimiento")]
        public int NumeroMovimiento { get; set; }

        [JsonPropertyName("saldoAnterior")]
        public decimal SaldoAnterior { get; set; }

        [JsonPropertyName("saldoNuevo")]
        public decimal SaldoNuevo { get; set; }

        [JsonPropertyName("importe")]
        public decimal Importe { get; set; }
    }
}