// En: EurekaBank.Core/Services/Abstractions/IAuthenticationService.cs
using EurekaBank.Core.Models.Requests;
using EurekaBank.Core.Models.Responses;

namespace EurekaBank.Core.Services.Abstractions
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
    }
}