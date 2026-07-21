using EurekaBank_RestFull_DotNet_GR05.Models;
using EurekaBank_RestFull_DotNet_GR05.Constants;

namespace EurekaBank_RestFull_DotNet_GR05.Validators
{
    /// <summary>
    /// Validador para operaciones relacionadas con cuentas
    /// </summary>
    public static class CuentaValidator
    {
        /// <summary>
        /// Verifica si una cuenta está activa
        /// </summary>
        public static bool EsActiva(Cuenta cuenta)
        {
            return cuenta != null && cuenta.Estado == EstadosConstants.ACTIVO;
        }
        
        /// <summary>
        /// Verifica si el saldo es suficiente para una operación
        /// </summary>
        public static bool TieneSaldoSuficiente(decimal saldo, decimal montoRequerido)
        {
            return saldo >= montoRequerido;
        }
        
        /// <summary>
        /// Verifica si la clave proporcionada es correcta
        /// </summary>
        public static bool ClaveCorrecta(Cuenta cuenta, string clave)
        {
            return cuenta != null && cuenta.Clave.Trim() == clave.Trim();
        }
        
        /// <summary>
        /// Verifica si dos cuentas son de la misma moneda
        /// </summary>
        public static bool MismaMoneda(Cuenta cuenta1, Cuenta cuenta2)
        {
            return cuenta1 != null && 
                   cuenta2 != null && 
                   cuenta1.CodigoMoneda == cuenta2.CodigoMoneda;
        }
        
        /// <summary>
        /// Verifica si la cuenta existe
        /// </summary>
        public static bool Existe(Cuenta cuenta)
        {
            return cuenta != null;
        }
    }
}
