using CliUniversalConsole.Models;

namespace CliUniversalConsole.Services
{
    public interface IReporteService
    {
        Task<List<MovimientoDetalle>> ObtenerMovimientosAsync(string codigoCuenta);
    }
}
