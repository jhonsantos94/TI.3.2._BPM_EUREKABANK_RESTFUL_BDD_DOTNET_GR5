using System;
using System.Runtime.Serialization;

namespace EurekaBank_RestFull_DotNet_GR05.Models
{
    /// <summary>
    /// Representa una cuenta bancaria
    /// </summary>
    [DataContract]
    public class Cuenta
    {
        [DataMember]
        public string Codigo { get; set; }

        [DataMember]
        public string CodigoMoneda { get; set; }

        [DataMember]
        public string CodigoSucursal { get; set; }

        [DataMember]
        public string CodigoEmpleadoCreador { get; set; }

        [DataMember]
        public string CodigoCliente { get; set; }

        [DataMember]
        public decimal Saldo { get; set; }

        [DataMember]
        public DateTime FechaCreacion { get; set; }

        [DataMember]
        public string Estado { get; set; }

        [DataMember]
        public int ContadorMovimientos { get; set; }

        [DataMember]
        public string Clave { get; set; }
    }
}
