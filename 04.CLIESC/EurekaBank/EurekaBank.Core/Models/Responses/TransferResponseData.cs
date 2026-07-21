// En: EurekaBank.Core/Models/Responses/TransferResponseData.cs
using System.Text.Json.Serialization;

namespace EurekaBank.Core.Models.Responses
{
    public class TransferResponseData
    {
        [JsonPropertyName("cuentaOrigen")]
        public AccountMovement CuentaOrigen { get; set; }

        [JsonPropertyName("cuentaDestino")]
        public AccountMovement CuentaDestino { get; set; }

        [JsonPropertyName("importeTransferido")]
        public decimal ImporteTransferido { get; set; }

        [JsonPropertyName("itf")]
        public decimal Itf { get; set; }

        [JsonPropertyName("costoPorMovimiento")]
        public decimal CostoPorMovimiento { get; set; }

        [JsonPropertyName("totalDescontado")]
        public decimal TotalDescontado { get; set; }
    }

    public class AccountMovement
    {
        [JsonPropertyName("codigo")]
        public string Codigo { get; set; }

        [JsonPropertyName("saldoAnterior")]
        public decimal SaldoAnterior { get; set; }

        [JsonPropertyName("saldoNuevo")]
        public decimal SaldoNuevo { get; set; }
    }
}