using System;
using System.Runtime.Serialization;

namespace EurekaBank_RestFull_DotNet_GR05.Models.DTOs
{
    /// <summary>
    /// DTO para mostrar el detalle de movimientos con información completa
    /// </summary>
    [DataContract]
    public class MovimientoDetalleDTO
    {
        [DataMember]
        public string CodigoCuenta { get; set; }

        [DataMember]
        public int NumeroMovimiento { get; set; }

        [DataMember]
        public DateTime Fecha { get; set; }

        [DataMember]
        public string TipoMovimiento { get; set; }

        [DataMember]
        public string Accion { get; set; } // INGRESO o SALIDA

        [DataMember]
        public decimal Importe { get; set; }

        [DataMember]
        public string EmpleadoNombre { get; set; }

        [DataMember]
        public string CuentaReferencia { get; set; }
    }
}
