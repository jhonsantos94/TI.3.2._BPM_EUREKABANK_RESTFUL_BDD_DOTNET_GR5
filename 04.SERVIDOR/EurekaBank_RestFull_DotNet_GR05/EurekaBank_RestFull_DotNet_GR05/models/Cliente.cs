using System;
using System.Runtime.Serialization;

namespace EurekaBank_RestFull_DotNet_GR05.Models
{
    /// <summary>
    /// Representa un cliente del banco
    /// </summary>
    [DataContract]
    public class Cliente
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
        public string DNI { get; set; }

        [DataMember]
        public string Ciudad { get; set; }

        [DataMember]
        public string Direccion { get; set; }

        [DataMember]
        public string Telefono { get; set; }

        [DataMember]
        public string Email { get; set; }

        /// <summary>
        /// Retorna el nombre completo del cliente
        /// </summary>
        public string NombreCompleto
        {
            get { return $"{Nombre} {Paterno} {Materno}"; }
        }
    }
}
