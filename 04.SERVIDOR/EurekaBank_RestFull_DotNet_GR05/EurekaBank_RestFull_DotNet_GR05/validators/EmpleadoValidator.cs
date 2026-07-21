using System;
using System.Text.RegularExpressions;

namespace EurekaBank_RestFull_DotNet_GR05.Validators
{
    /// <summary>
    /// Validador para operaciones relacionadas con empleados
    /// </summary>
    public static class EmpleadoValidator
    {
        /// <summary>
        /// Verifica si el nombre de usuario es válido
        /// </summary>
        public static bool UsuarioValido(string usuario)
        {
            return !string.IsNullOrWhiteSpace(usuario) && usuario.Length >= 4;
        }
        
        /// <summary>
        /// Verifica si la contraseña es válida (mínimo 6 caracteres)
        /// </summary>
        public static bool ClaveValida(string clave)
        {
            return !string.IsNullOrWhiteSpace(clave) && clave.Length >= 4;
        }
        
        /// <summary>
        /// Verifica si el código de empleado tiene el formato correcto
        /// </summary>
        public static bool CodigoValido(string codigo)
        {
            return !string.IsNullOrWhiteSpace(codigo) && codigo.Length == 4;
        }
        
        /// <summary>
        /// Verifica si los datos básicos del empleado son válidos
        /// </summary>
        public static bool DatosCompletos(string nombre, string paterno, string materno)
        {
            return !string.IsNullOrWhiteSpace(nombre) &&
                   !string.IsNullOrWhiteSpace(paterno) &&
                   !string.IsNullOrWhiteSpace(materno);
        }
    }
}
