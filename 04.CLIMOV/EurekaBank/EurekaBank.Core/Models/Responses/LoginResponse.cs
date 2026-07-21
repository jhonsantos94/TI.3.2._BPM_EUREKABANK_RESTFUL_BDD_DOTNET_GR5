// En: EurekaBank.Core/Models/Responses/LoginResponse.cs
using System.Text.Json.Serialization;

namespace EurekaBank.Core.Models.Responses
{
    // Modelo unificado que puede contener la respuesta de cualquier servicio de Login
    public class LoginResponse
    {
        [JsonPropertyName("exitoso")]
        public bool Exitoso { get; set; }

        [JsonPropertyName("mensaje")]
        public string? Mensaje { get; set; }

        [JsonPropertyName("codigoError")]
        public string? CodigoError { get; set; }

        [JsonPropertyName("datos")]
        public EmpleadoDto? Datos { get; set; }
    }

    // Modelo que representa los datos del empleado, unificando todos los campos
    public class EmpleadoDto
    {
        [JsonPropertyName("codigo")]
        public string? Codigo { get; set; }

        [JsonPropertyName("paterno")]
        public string? Paterno { get; set; }

        [JsonPropertyName("materno")]
        public string? Materno { get; set; }

        [JsonPropertyName("nombre")]
        public string? Nombre { get; set; }

        [JsonPropertyName("ciudad")]
        public string? Ciudad { get; set; }

        [JsonPropertyName("direccion")]
        public string? Direccion { get; set; }

        [JsonPropertyName("usuario")]
        public string? Usuario { get; set; }

        [JsonPropertyName("clave")]
        public string? Clave { get; set; }

        [JsonPropertyName("nombreCompleto")]
        public string? NombreCompleto { get; set; } // Este campo solo viene del REST
    }
}