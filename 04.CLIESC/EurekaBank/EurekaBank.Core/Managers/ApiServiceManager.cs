// En: EurekaBank.Core/Managers/ApiServiceManager.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EurekaBank.Core.Managers
{
    // Usaremos INotifyPropertyChanged para que la UI pueda reaccionar a los cambios
    public class ApiServiceManager : INotifyPropertyChanged
    {
        private ApiProtocol _currentProtocol = ApiProtocol.Rest;
        public ApiProtocol CurrentProtocol
        {
            get => _currentProtocol;
            set
            {
                if (_currentProtocol != value)
                {
                    _currentProtocol = value;
                    OnPropertyChanged(); // Notifica a la UI que esta propiedad cambió
                }
            }
        }

        private ApiPlatform _currentPlatform = ApiPlatform.Java;
        public ApiPlatform CurrentPlatform
        {
            get => _currentPlatform;
            set
            {
                if (_currentPlatform != value)
                {
                    _currentPlatform = value;
                    OnPropertyChanged(); // Notifica a la UI
                }
            }
        }

        // --- Implementación de INotifyPropertyChanged ---
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Enums para definir nuestras opciones
    public enum ApiProtocol { Rest, Soap }
    public enum ApiPlatform { Java, DotNet }
}