using System;
using System.Runtime.Serialization;

namespace EurekaBank_RestFull_DotNet_GR05.Models.DTOs
{
    /// <summary>
    /// DTO para transferencias entre cuentas
    /// </summary>
    [DataContract]
    public class TransferenciaDTO
    {
        [DataMember]
        public string CuentaOrigen { get; set; }

        [DataMember]
        public string CuentaDestino { get; set; }

        [DataMember]
        public string ClaveCuentaOrigen { get; set; }

        [DataMember]
        public decimal Importe { get; set; }

        [DataMember]
        public string CodigoEmpleado { get; set; }
    }
}
