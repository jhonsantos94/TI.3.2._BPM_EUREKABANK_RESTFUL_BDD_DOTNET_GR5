namespace CliUniversalConsole.Models
{
    public class MovimientoDetalle
    {
        public string CodigoCuenta { get; set; } = "";
        public int NumeroMovimiento { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoMovimiento { get; set; } = "";
        public string EmpleadoNombre { get; set; } = "";
        public string? CuentaReferencia { get; set; }
        public decimal Importe { get; set; }

        public void Print()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine($"\n┌─────────────────────────────────────────┐");
            Console.WriteLine($"│ Movimiento #{NumeroMovimiento}          ");
            Console.WriteLine($"└─────────────────────────────────────────┘");
            Console.ResetColor();
            
            Console.WriteLine($"Fecha:             {Fecha:dd/MM/yyyy HH:mm:ss}");
            Console.WriteLine($"Tipo:              {TipoMovimiento}");
            Console.WriteLine($"Importe:           S/ {Importe:N2}");
            Console.WriteLine($"Empleado:          {EmpleadoNombre}");
            
            if (!string.IsNullOrEmpty(CuentaReferencia))
            {
                Console.WriteLine($"Cuenta Ref:        {CuentaReferencia}");
            }
        }
    }
}
