using System;
using EurekaBank_RestFull_DotNet_GR05.DAL;
using EurekaBank_RestFull_DotNet_GR05.Models;
using EurekaBank_RestFull_DotNet_GR05.Models.DTOs;
using EurekaBank_RestFull_DotNet_GR05.Validators;
using EurekaBank_RestFull_DotNet_GR05.Constants;

namespace EurekaBank_RestFull_DotNet_GR05.Services
{
    /// <summary>
    /// Servicio de l�gica de negocio para autenticaci�n y gesti�n de empleados
    /// </summary>
    public class AutenticacionService
    {
        private readonly EmpleadoDAO empleadoDAO;

        public AutenticacionService()
        {
            empleadoDAO = new EmpleadoDAO();
        }

        /// <summary>
        /// Realiza el login de un empleado
        /// </summary>
        /// <param name="usuario">Usuario del empleado</param>
        /// <param name="clave">Contrase�a del empleado</param>
        /// <returns>RespuestaDTO con el resultado de la operaci�n</returns>
        public RespuestaDTO Login(string usuario, string clave)
        {
            try
            {
                // Validar datos de entrada
                if (!EmpleadoValidator.UsuarioValido(usuario))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_USUARIO_VACIO,
                        CodigoError = "VAL001"
                    };
                }

                if (!EmpleadoValidator.ClaveValida(clave))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_CLAVE_CORTA,
                        CodigoError = "VAL002"
                    };
                }

                // Validar credenciales
                Empleado empleado = empleadoDAO.ValidarCredenciales(usuario, clave);

                if (empleado == null)
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_CREDENCIALES_INVALIDAS,
                        CodigoError = "AUTH001"
                    };
                }

                // Ocultar la contrase�a en la respuesta
                empleado.Clave = null;

                return new RespuestaDTO
                {
                    Exitoso = true,
                    Mensaje = MensajesConstants.LOGIN_EXITOSO,
                    Datos = empleado
                };
            }
            catch (Exception ex)
            {
                return new RespuestaDTO
                {
                    Exitoso = false,
                    Mensaje = $"{MensajesConstants.ERROR_BASE_DATOS}: {ex.Message}",
                    CodigoError = "DB001"
                };
            }
        }

        /// <summary>
        /// Registra un nuevo empleado en el sistema
        /// </summary>
        /// <param name="empleado">Datos del empleado a registrar</param>
        /// <returns>RespuestaDTO con el resultado de la operaci�n</returns>
        public RespuestaDTO RegistrarEmpleado(Empleado empleado)
        {
            try
            {
                // Validar datos completos
                if (!EmpleadoValidator.DatosCompletos(empleado.Nombre, empleado.Paterno, empleado.Materno))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_DATOS_INCOMPLETOS,
                        CodigoError = "VAL003"
                    };
                }

                // Validar usuario
                if (!EmpleadoValidator.UsuarioValido(empleado.Usuario))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_USUARIO_VACIO,
                        CodigoError = "VAL001"
                    };
                }

                // Validar clave
                if (!EmpleadoValidator.ClaveValida(empleado.Clave))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_CLAVE_CORTA,
                        CodigoError = "VAL002"
                    };
                }

                // Verificar si el usuario ya existe
                Empleado empleadoExistente = empleadoDAO.ObtenerPorUsuario(empleado.Usuario);
                if (empleadoExistente != null)
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_USUARIO_EXISTENTE,
                        CodigoError = "AUTH002"
                    };
                }

                // Generar c�digo de empleado
                string codigoGenerado = empleadoDAO.GenerarCodigoEmpleado();
                empleado.Codigo = codigoGenerado;

                // Registrar empleado
                bool registrado = empleadoDAO.Registrar(empleado);

                if (!registrado)
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_OPERACION_FALLIDA,
                        CodigoError = "DB002"
                    };
                }

                // Ocultar contrase�a en la respuesta
                empleado.Clave = null;

                return new RespuestaDTO
                {
                    Exitoso = true,
                    Mensaje = MensajesConstants.REGISTRO_EXITOSO,
                    Datos = empleado
                };
            }
            catch (Exception ex)
            {
                return new RespuestaDTO
                {
                    Exitoso = false,
                    Mensaje = $"{MensajesConstants.ERROR_BASE_DATOS}: {ex.Message}",
                    CodigoError = "DB001"
                };
            }
        }

        /// <summary>
        /// Cambia la contrase�a de un empleado
        /// </summary>
        /// <param name="codigo">C�digo del empleado</param>
        /// <param name="claveActual">Contrase�a actual</param>
        /// <param name="claveNueva">Nueva contrase�a</param>
        /// <returns>RespuestaDTO con el resultado de la operaci�n</returns>
        public RespuestaDTO CambiarClave(string codigo, string claveActual, string claveNueva)
        {
            try
            {
                // Validar c�digo
                if (!EmpleadoValidator.CodigoValido(codigo))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = "C�digo de empleado inv�lido",
                        CodigoError = "VAL004"
                    };
                }

                // Validar nueva clave
                if (!EmpleadoValidator.ClaveValida(claveNueva))
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_CLAVE_CORTA,
                        CodigoError = "VAL002"
                    };
                }

                // Obtener empleado
                Empleado empleado = empleadoDAO.ObtenerPorCodigo(codigo);
                if (empleado == null)
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = "Empleado no encontrado",
                        CodigoError = "AUTH003"
                    };
                }

                // Validar clave actual
                if (empleado.Clave != claveActual)
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_CLAVE_INCORRECTA,
                        CodigoError = "AUTH004"
                    };
                }

                // Actualizar clave
                bool actualizado = empleadoDAO.ActualizarClave(codigo, claveNueva);

                if (!actualizado)
                {
                    return new RespuestaDTO
                    {
                        Exitoso = false,
                        Mensaje = MensajesConstants.ERROR_OPERACION_FALLIDA,
                        CodigoError = "DB002"
                    };
                }

                return new RespuestaDTO
                {
                    Exitoso = true,
                    Mensaje = "Contrase�a actualizada exitosamente"
                };
            }
            catch (Exception ex)
            {
                return new RespuestaDTO
                {
                    Exitoso = false,
                    Mensaje = $"{MensajesConstants.ERROR_BASE_DATOS}: {ex.Message}",
                    CodigoError = "DB001"
                };
            }
        }
    }
}
