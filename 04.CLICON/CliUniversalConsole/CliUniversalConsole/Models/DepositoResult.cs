namespace CliUniversalConsole.Models
{
    public class DepositoResult
    {
        public string CodigoCuenta { get; set; }
        public decimal ImporteDepositado { get; set; }
        public decimal SaldoAnterior { get; set; }
        public decimal SaldoNuevo { get; set; }
        public int NumeroMovimiento { get; set; }

        public void Print()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n╔════════════════════════════════════════╗");
            Console.WriteLine("║       DEPÓSITO REALIZADO EXITOSO       ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.WriteLine($"\n💰 Cuenta: {CodigoCuenta}");
            Console.WriteLine($"💵 Importe Depositado: S/ {ImporteDepositado:N2}");
            Console.WriteLine($"📊 Saldo Anterior: S/ {SaldoAnterior:N2}");
            Console.WriteLine($"📈 Saldo Nuevo: S/ {SaldoNuevo:N2}");
            Console.WriteLine($"🔢 Número de Movimiento: {NumeroMovimiento}");
            Console.ResetColor();
        }
    }
}
