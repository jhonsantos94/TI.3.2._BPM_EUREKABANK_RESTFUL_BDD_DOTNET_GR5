using CliUniversalConsole.Models;

namespace CliUniversalConsole.Services
{
    public interface IAutenticacionService
    {
        Task<LoginResult> LoginAsync(string usuario, string clave);
    }
}
