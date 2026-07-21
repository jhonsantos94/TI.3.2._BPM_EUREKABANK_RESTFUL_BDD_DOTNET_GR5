namespace CliUniversalConsole.Models
{
    public class RetiroResult
    {
        public string CodigoCuenta { get; set; }
        public decimal ImporteRetirado { get; set; }
        public decimal ImporteITF { get; set; }
        public decimal ImporteCargo { get; set; }
        public decimal TotalDescontado { get; set; }
        public decimal SaldoAnterior { get; set; }
        public decimal SaldoNuevo { get; set; }
        public int NumeroMovimientoRetiro { get; set; }
        public int? NumeroMovimientoITF { get; set; }
        public int? NumeroMovimientoCargo { get; set; }

        public void Print()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n╔════════════════════════════════════════╗");
            Console.WriteLine("║        RETIRO REALIZADO EXITOSO        ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.WriteLine($"\n💰 Cuenta: {CodigoCuenta}");
            Console.WriteLine($"💵 Importe Retirado: S/ {ImporteRetirado:N2}");
            Console.WriteLine($"🏦 ITF (0.005%): S/ {ImporteITF:N2}");
            Console.WriteLine($"💳 Cargo por Movimiento: S/ {ImporteCargo:N2}");
            Console.WriteLine($"➖ Total Descontado: S/ {TotalDescontado:N2}");
            Console.WriteLine($"📊 Saldo Anterior: S/ {SaldoAnterior:N2}");
            Console.WriteLine($"📉 Saldo Nuevo: S/ {SaldoNuevo:N2}");
            Console.WriteLine($"\n🔢 Movimientos registrados:");
            Console.WriteLine($"   • Retiro: #{NumeroMovimientoRetiro}");
            if (NumeroMovimientoITF.HasValue)
                Console.WriteLine($"   • ITF: #{NumeroMovimientoITF}");
            if (NumeroMovimientoCargo.HasValue)
                Console.WriteLine($"   • Cargo: #{NumeroMovimientoCargo}");
            Console.ResetColor();
        }
    }
}
