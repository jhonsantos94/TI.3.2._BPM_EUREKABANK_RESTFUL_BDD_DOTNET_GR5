using System;
using System.Runtime.Serialization;

namespace EurekaBank_RestFull_DotNet_GR05.Models
{
    /// <summary>
    /// Representa un tipo de movimiento bancario
    /// </summary>
    [DataContract]
    public class TipoMovimiento
    {
        [DataMember]
        public string Codigo { get; set; }

        [DataMember]
        public string Descripcion { get; set; }

        [DataMember]
        public string Accion { get; set; } // INGRESO o SALIDA

        [DataMember]
        public string Estado { get; set; }
    }
}
