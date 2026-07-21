using System;
using System.Runtime.Serialization;

namespace EurekaBank_RestFull_DotNet_GR05.Models
{
    /// <summary>
    /// Representa una moneda en el sistema
    /// </summary>
    [DataContract]
    public class Moneda
    {
        [DataMember]
        public string Codigo { get; set; }

        [DataMember]
        public string Descripcion { get; set; }
    }
}
