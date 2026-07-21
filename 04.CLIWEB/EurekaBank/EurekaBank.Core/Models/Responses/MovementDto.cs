// En: EurekaBank.Core/Models/Responses/MovementDto.cs
using System.Text.Json.Serialization;

namespace EurekaBank.Core.Models.Responses
{
    public class MovementDto
    {
        [JsonPropertyName("codigoCuenta")]
        public string? CodigoCuenta { get; set; }

        [JsonPropertyName("numeroMovimiento")]
        public int NumeroMovimiento { get; set; }

        [JsonPropertyName("fecha")]
        public DateTime Fecha { get; set; }

        [JsonPropertyName("tipoMovimiento")]
        public string? TipoMovimiento { get; set; }

        [JsonPropertyName("accion")]
        public string? Accion { get; set; }

        [JsonPropertyName("importe")]
        public decimal Importe { get; set; }

        [JsonPropertyName("empleadoNombre")]
        public string? EmpleadoNombre { get; set; }

        [JsonPropertyName("cuentaReferencia")]
        public string? CuentaReferencia { get; set; }
    }
}