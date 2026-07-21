using System;
using System.Runtime.Serialization;

namespace EurekaBank_RestFull_DotNet_GR05.Models
{
    /// <summary>
    /// Representa un empleado del banco
    /// </summary>
    [DataContract]
    public class Empleado
    {
        [DataMember]
        public string Codigo { get; set; }

        [DataMember]
        public string Paterno { get; set; }

        [DataMember]
        public string Materno { get; set; }

        [DataMember]
        public string Nombre { get; set; }

        [DataMember]
        public string Ciudad { get; set; }

        [DataMember]
        public string Direccion { get; set; }

        [DataMember]
        public string Usuario { get; set; }

        [DataMember]
        public string Clave { get; set; }

        /// <summary>
        /// Retorna el nombre completo del empleado
        /// </summary>
        public string NombreCompleto
        {
            get { return $"{Nombre} {Paterno} {Materno}"; }
        }
    }
}
