namespace CliUniversalConsole.Models
{
    public class TransferenciaRequest
    {
        public string CuentaOrigen { get; set; }
        public string ClaveCuentaOrigen { get; set; }
        public string CuentaDestino { get; set; }
        public decimal Importe { get; set; }
        public string CodigoEmpleado { get; set; }
    }
}
