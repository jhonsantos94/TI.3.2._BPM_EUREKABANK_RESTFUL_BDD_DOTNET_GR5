// En: EurekaBank.Core/Services/Abstractions/IReportService.cs
using EurekaBank.Core.Models.Responses;

namespace EurekaBank.Core.Services.Abstractions
{
    public interface IReportService
    {
        // El método recibirá el código de la cuenta y devolverá una lista de movimientos.
        Task<IEnumerable<MovementDto>> ObtenerMovimientosAsync(string codigoCuenta);
    }
}