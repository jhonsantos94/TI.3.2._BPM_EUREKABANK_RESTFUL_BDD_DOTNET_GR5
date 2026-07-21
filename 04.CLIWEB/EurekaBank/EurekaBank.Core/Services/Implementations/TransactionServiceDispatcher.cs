// En: EurekaBank.Core/Services/Implementations/TransactionServiceDispatcher.cs
using EurekaBank.Core.Managers;
using EurekaBank.Core.Models.Requests;
using EurekaBank.Core.Models.Responses;
using EurekaBank.Core.Services.Abstractions;

namespace EurekaBank.Core.Services.Implementations
{
    public class TransactionServiceDispatcher : ITransactionService
    {
        private readonly ApiServiceManager _apiManager;
        private readonly RestTransactionService _restService;
        private readonly SoapTransactionService _soapService;

        public TransactionServiceDispatcher(
            ApiServiceManager apiManager,
            RestTransactionService restService,
            SoapTransactionService soapService)
        {
            _apiManager = apiManager;
            _restService = restService;
            _soapService = soapService;
        }

        private ITransactionService GetActiveService()
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

        public Task<TransactionResponse<DepositResponseData>> RealizarDepositoAsync(DepositRequest request)
        {
            return GetActiveService().RealizarDepositoAsync(request);
        }

        public Task<TransactionResponse<WithdrawResponseData>> RealizarRetiroAsync(DepositRequest request)
        {
            return GetActiveService().RealizarRetiroAsync(request);
        }

        public Task<TransactionResponse<TransferResponseData>> RealizarTransferenciaAsync(TransferRequest request)
        {
            return GetActiveService().RealizarTransferenciaAsync(request);
        }
    }
}