// En: EurekaBank.Core/Models/Requests/LoginRequest.cs
using System.Text.Json.Serialization;

namespace EurekaBank.Core.Models.Requests
{
    public class LoginRequest
    {
        [JsonPropertyName("usuario")]
        public string Usuario { get; set; }

        [JsonPropertyName("clave")]
        public string Clave { get; set; }
    }
}