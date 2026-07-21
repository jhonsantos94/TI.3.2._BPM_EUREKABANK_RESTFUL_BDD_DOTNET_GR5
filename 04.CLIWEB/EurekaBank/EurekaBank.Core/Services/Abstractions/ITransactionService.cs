// En: EurekaBank.Core/Services/Abstractions/ITransactionService.cs
using EurekaBank.Core.Models.Requests;
using EurekaBank.Core.Models.Responses;

namespace EurekaBank.Core.Services.Abstractions
{
    public interface ITransactionService
    {
        Task<TransactionResponse<DepositResponseData>> RealizarDepositoAsync(DepositRequest request);
        Task<TransactionResponse<WithdrawResponseData>> RealizarRetiroAsync(DepositRequest request); // Reutiliza DepositRequest
        Task<TransactionResponse<TransferResponseData>> RealizarTransferenciaAsync(TransferRequest request);
    }
}