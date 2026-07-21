using System;
using System.Runtime.Serialization;

namespace EurekaBank_RestFull_DotNet_GR05.Models
{
    /// <summary>
    /// Representa un movimiento bancario en una cuenta
    /// </summary>
    [DataContract]
    public class Movimiento
    {
        [DataMember]
        public string CodigoCuenta { get; set; }

        [DataMember]
        public int Numero { get; set; }

        [DataMember]
        public DateTime Fecha { get; set; }

        [DataMember]
        public string CodigoEmpleado { get; set; }

        [DataMember]
        public string CodigoTipo { get; set; }

        [DataMember]
        public decimal Importe { get; set; }

        [DataMember]
        public string CuentaReferencia { get; set; }
    }
}
