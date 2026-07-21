using System;
using System.Runtime.Serialization;

namespace EurekaBank_RestFull_DotNet_GR05.Models.DTOs
{
    /// <summary>
    /// DTO para consultar el estado de una cuenta con información completa
    /// </summary>
    [DataContract]
    public class EstadoCuentaDTO
    {
        [DataMember]
        public string CodigoCuenta { get; set; }

        [DataMember]
        public string NombreCliente { get; set; }

        [DataMember]
        public string DNICliente { get; set; }

        [DataMember]
        public decimal Saldo { get; set; }

        [DataMember]
        public string Moneda { get; set; }

        [DataMember]
        public string Estado { get; set; }

        [DataMember]
        public DateTime FechaCreacion { get; set; }

        [DataMember]
        public string Sucursal { get; set; }

        [DataMember]
        public int TotalMovimientos { get; set; }
    }
}
