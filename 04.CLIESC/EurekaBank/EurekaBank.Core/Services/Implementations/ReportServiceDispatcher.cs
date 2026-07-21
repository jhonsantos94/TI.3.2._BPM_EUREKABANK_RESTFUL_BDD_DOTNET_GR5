// En: EurekaBank.Core/Services/Implementations/ReportServiceDispatcher.cs
using EurekaBank.Core.Managers;
using EurekaBank.Core.Models.Responses;
using EurekaBank.Core.Services.Abstractions;

namespace EurekaBank.Core.Services.Implementations
{
    public class ReportServiceDispatcher : IReportService
    {
        private readonly ApiServiceManager _apiManager;
        private readonly RestReportService _restService;
        private readonly SoapReportService _soapService;

        public ReportServiceDispatcher(
            ApiServiceManager apiManager,
            RestReportService restService,
            SoapReportService soapService)
        {
            _apiManager = apiManager;
            _restService = restService;
            _soapService = soapService;
        }

        private IReportService GetActiveService()
        {
            if (_apiManager.CurrentProtocol == ApiProtocol.Rest)
            {
                _restService.SetTarget(_apiManager.CurrentPlatform);
                return _restService;
            }
            else // SOAP
            {
                _soapService.SetTarget(_apiManager.CurrentPlatform);
                return _soapService;
            }
        }

        public Task<IEnumerable<MovementDto>> ObtenerMovimientosAsync(string codigoCuenta)
        {
            // Simplemente delegamos la llamada al servicio activo
            return GetActiveService().ObtenerMovimientosAsync(codigoCuenta);
        }
    }
}