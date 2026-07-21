// En: EurekaBank.Core/Managers/SessionService.cs
using EurekaBank.Core.Models.Responses;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EurekaBank.Core.Managers
{
    public class SessionService : INotifyPropertyChanged
    {
        private EmpleadoDto? _currentUser;
        public EmpleadoDto? CurrentUser
        {
            get => _currentUser;
            private set // El setter es privado para que solo se pueda modificar desde dentro del servicio
            {
                if (_currentUser != value)
                {
                    var wasLoggedIn = IsLoggedIn;
                    _currentUser = value;
                    var isNowLoggedIn = IsLoggedIn;
                    
                    // Logging para debugging
                    System.Diagnostics.Debug.WriteLine($"SessionService: CurrentUser changed. Was logged in: {wasLoggedIn}, Now logged in: {isNowLoggedIn}");
                    System.Diagnostics.Debug.WriteLine($"SessionService: New user: {(_currentUser?.Nombre ?? "null")}");
                    
                    OnPropertyChanged(); // Notifica a la UI que el usuario ha cambiado
                    OnPropertyChanged(nameof(IsLoggedIn)); // Notifica también que el estado de login ha cambiado
                    
                    System.Diagnostics.Debug.WriteLine($"SessionService: PropertyChanged events fired");
                }
            }
        }

        // Una propiedad computada para saber fácilmente si hay una sesión activa
        public bool IsLoggedIn => CurrentUser != null;

        // Método para iniciar sesión
        public void Login(EmpleadoDto employee)
        {
            System.Diagnostics.Debug.WriteLine($"SessionService.Login called with user: {employee?.Nombre}");
            CurrentUser = employee;
        }

        // Método para cerrar sesión
        public void Logout()
        {
            System.Diagnostics.Debug.WriteLine("SessionService.Logout called");
            CurrentUser = null;
        }

        // --- Implementación de INotifyPropertyChanged ---
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            System.Diagnostics.Debug.WriteLine($"SessionService.OnPropertyChanged: {propertyName}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}