using System;
using System.Runtime.Serialization;
using EurekaBank_RestFull_DotNet_GR05.Models;

namespace EurekaBank_RestFull_DotNet_GR05.Models.DTOs
{
    /// <summary>
    /// Respuesta est�ndar para operaciones
    /// </summary>
    [DataContract]
    [KnownType(typeof(Empleado))]
    [KnownType(typeof(Cliente))]
    [KnownType(typeof(Cuenta))]
    [KnownType(typeof(DepositoResultDTO))]
    [KnownType(typeof(RetiroResultDTO))]
    [KnownType(typeof(TransferenciaResultDTO))]
    [KnownType(typeof(CuentaResumenDTO))]
    public class RespuestaDTO
    {
        [DataMember]
        public bool Exitoso { get; set; }

        [DataMember]
        public string Mensaje { get; set; }

        [DataMember]
        public string CodigoError { get; set; }

        [DataMember]
        public object Datos { get; set; }
    }
}
