namespace CliUniversalConsole.Models
{
    public class TransaccionResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public void Print()
        {
            Console.ForegroundColor = IsSuccess ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine($"\n{(IsSuccess ? "✅" : "❌")} {Message}");
            Console.ResetColor();
        }
    }
}
