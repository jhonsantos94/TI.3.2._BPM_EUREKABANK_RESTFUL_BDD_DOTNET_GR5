using CliUniversalConsole.Models;

namespace CliUniversalConsole.Services
{
    public interface ITransaccionService
    {
        Task<TransaccionResult> RealizarDepositoAsync(TransaccionRequest request);
        Task<TransaccionResult> RealizarRetiroAsync(TransaccionRequest request);
        Task<TransaccionResult> RealizarTransferenciaAsync(TransferenciaRequest request);
    }
}
