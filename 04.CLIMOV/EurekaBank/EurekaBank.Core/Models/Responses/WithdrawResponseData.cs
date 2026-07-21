// En: EurekaBank.Core/Models/Responses/WithdrawResponseData.cs
using System.Text.Json.Serialization;

namespace EurekaBank.Core.Models.Responses
{
    public class WithdrawResponseData
    {
        [JsonPropertyName("saldoAnterior")]
        public decimal SaldoAnterior { get; set; }

        [JsonPropertyName("saldoNuevo")]
        public decimal SaldoNuevo { get; set; }

        [JsonPropertyName("importeRetiro")]
        public decimal ImporteRetiro { get; set; }

        [JsonPropertyName("itf")]
        public decimal Itf { get; set; }

        [JsonPropertyName("costoPorMovimiento")]
        public decimal CostoPorMovimiento { get; set; }

        [JsonPropertyName("totalDescontado")]
        public decimal TotalDescontado { get; set; }
    }
}