// En: EurekaBank.Core/Services/Implementations/AuthenticationServiceDispatcher.cs
using EurekaBank.Core.Managers;
using EurekaBank.Core.Models.Requests;
using EurekaBank.Core.Models.Responses;
using EurekaBank.Core.Services.Abstractions;

namespace EurekaBank.Core.Services.Implementations
{
    public class AuthenticationServiceDispatcher : IAuthenticationService
    {
        private readonly ApiServiceManager _apiManager;
        private readonly RestAuthenticationService _restService;
        private readonly SoapAuthenticationService _soapService;

        // El constructor recibe el gestor y las dos implementaciones concretas
        public AuthenticationServiceDispatcher(
            ApiServiceManager apiManager,
            RestAuthenticationService restService,
            SoapAuthenticationService soapService)
        {
            _apiManager = apiManager;
            _restService = restService;
            _soapService = soapService;
        }

        public Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            // Decidimos a qué servicio llamar basándonos en el protocolo actual
            if (_apiManager.CurrentProtocol == ApiProtocol.Rest)
            {
                // Le decimos al servicio REST a qué plataforma apuntar
                _restService.SetTarget(_apiManager.CurrentPlatform);
                return _restService.LoginAsync(request);
            }
            else // SOAP
            {
                // Le decimos al servicio SOAP a qué plataforma apuntar
                _soapService.SetTarget(_apiManager.CurrentPlatform);
                return _soapService.LoginAsync(request);
            }
        }
    }
}