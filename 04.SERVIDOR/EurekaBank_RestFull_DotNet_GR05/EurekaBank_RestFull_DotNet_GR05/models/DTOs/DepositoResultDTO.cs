using System.Runtime.Serialization;

namespace EurekaBank_RestFull_DotNet_GR05.Models.DTOs
{
    /// <summary>
    /// DTO para resultado de operación de depósito
    /// </summary>
    [DataContract]
    public class DepositoResultDTO
    {
        [DataMember]
        public int NumeroMovimiento { get; set; }

        [DataMember]
        public decimal SaldoAnterior { get; set; }

        [DataMember]
        public decimal SaldoNuevo { get; set; }

        [DataMember]
        public decimal Importe { get; set; }
    }
}
