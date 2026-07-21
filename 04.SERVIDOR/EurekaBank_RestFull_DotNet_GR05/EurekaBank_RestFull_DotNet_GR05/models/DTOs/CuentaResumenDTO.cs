using System.Runtime.Serialization;

namespace EurekaBank_RestFull_DotNet_GR05.Models.DTOs
{
    /// <summary>
    /// DTO para resumen de cuenta en operaciones
    /// </summary>
    [DataContract]
    public class CuentaResumenDTO
    {
        [DataMember]
        public string Codigo { get; set; }

        [DataMember]
        public decimal SaldoAnterior { get; set; }

        [DataMember]
        public decimal SaldoNuevo { get; set; }
    }
}
