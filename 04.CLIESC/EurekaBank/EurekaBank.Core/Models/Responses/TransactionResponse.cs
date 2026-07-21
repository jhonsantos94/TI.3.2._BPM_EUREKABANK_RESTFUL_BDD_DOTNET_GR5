// En: EurekaBank.Core/Models/Responses/TransactionResponse.cs
using System.Text.Json.Serialization;

namespace EurekaBank.Core.Models.Responses
{
    // Esta es la estructura común a TODAS las respuestas de transacción
    public class TransactionResponse<T>
    {
        [JsonPropertyName("exitoso")]
        public bool Exitoso { get; set; }

        [JsonPropertyName("mensaje")]
        public string? Mensaje { get; set; }

        [JsonPropertyName("codigoError")]
        public string? CodigoError { get; set; }

        [JsonPropertyName("datos")]
        public T? Datos { get; set; }
    }
}