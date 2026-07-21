namespace EurekaBank_RestFull_DotNet_GR05.Constants
{
    /// <summary>
    /// Mensajes estándar para respuestas del sistema
    /// </summary>
    public static class MensajesConstants
    {
        // Mensajes de éxito
        public const string DEPOSITO_EXITOSO = "Depósito realizado exitosamente";
        public const string RETIRO_EXITOSO = "Retiro realizado exitosamente";
        public const string TRANSFERENCIA_EXITOSA = "Transferencia realizada exitosamente";
        public const string LOGIN_EXITOSO = "Autenticación exitosa";
        public const string REGISTRO_EXITOSO = "Empleado registrado exitosamente";
        
        // Errores de cuenta
        public const string ERROR_CUENTA_NO_EXISTE = "La cuenta no existe";
        public const string ERROR_CUENTA_INACTIVA = "La cuenta no está activa";
        public const string ERROR_CLAVE_INCORRECTA = "Clave incorrecta";
        public const string ERROR_SALDO_INSUFICIENTE = "Saldo insuficiente para realizar la operación";
        
        // Errores de validación
        public const string ERROR_IMPORTE_INVALIDO = "El importe debe ser mayor a cero";
        public const string ERROR_IMPORTE_DECIMALES = "El importe no puede tener más de 2 decimales";
        public const string ERROR_MONEDA_DIFERENTE = "Las cuentas deben ser de la misma moneda";
        
        // Errores de autenticación
        public const string ERROR_CREDENCIALES_INVALIDAS = "Usuario o contraseña incorrectos";
        public const string ERROR_USUARIO_EXISTENTE = "El usuario ya existe";
        public const string ERROR_USUARIO_VACIO = "El usuario no puede estar vacío";
        public const string ERROR_CLAVE_CORTA = "La contraseña debe tener al menos 6 caracteres";
        
        // Errores generales
        public const string ERROR_DATOS_INCOMPLETOS = "Faltan datos obligatorios";
        public const string ERROR_OPERACION_FALLIDA = "La operación no pudo completarse";
        public const string ERROR_BASE_DATOS = "Error al conectar con la base de datos";
    }
}
