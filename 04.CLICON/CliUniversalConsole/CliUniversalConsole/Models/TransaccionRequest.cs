namespace CliUniversalConsole.Models
{
    public class TransaccionRequest
    {
        public string CodigoCuenta { get; set; }
        public string ClaveCuenta { get; set; }
        public decimal Importe { get; set; }
        public string CodigoEmpleado { get; set; }
    }
}
