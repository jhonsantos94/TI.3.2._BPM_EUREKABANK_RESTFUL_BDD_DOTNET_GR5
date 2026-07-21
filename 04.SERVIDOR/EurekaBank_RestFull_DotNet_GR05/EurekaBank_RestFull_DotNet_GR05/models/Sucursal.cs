using System;
using System.Runtime.Serialization;

namespace EurekaBank_RestFull_DotNet_GR05.Models
{
    /// <summary>
    /// Representa una sucursal del banco
    /// </summary>
    [DataContract]
    public class Sucursal
    {
        [DataMember]
        public string Codigo { get; set; }

        [DataMember]
        public string Nombre { get; set; }

        [DataMember]
        public string Ciudad { get; set; }

        [DataMember]
        public string Direccion { get; set; }

        [DataMember]
        public int ContadorCuentas { get; set; }
    }
}
