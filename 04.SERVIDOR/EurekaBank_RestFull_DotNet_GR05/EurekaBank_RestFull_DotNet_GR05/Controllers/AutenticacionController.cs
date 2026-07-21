using EurekaBank_RestFull_DotNet_GR05.Models;
using EurekaBank_RestFull_DotNet_GR05.Models.DTOs;
using EurekaBank_RestFull_DotNet_GR05.Services;
using Microsoft.AspNetCore.Mvc;

namespace EurekaBank_RestFull_DotNet_GR05.Controllers
{
    /// <summary>
    /// API Controller para operaciones de autenticación
    /// Replica la funcionalidad del servicio SOAP ServicioAutenticacion
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AutenticacionController : ControllerBase
    {
        private readonly AutenticacionService _autenticacionService;

        public AutenticacionController()
        {
            _autenticacionService = new AutenticacionService();
        }

        /// <summary>
        /// Autentica un usuario empleado
        /// </summary>
        /// <param name="request">Objeto con usuario y clave</param>
        /// <returns>RespuestaDTO con datos del empleado si es exitoso</returns>
        [HttpPost("login")]
        public ActionResult<RespuestaDTO> Login([FromBody] LoginRequest request)
        {
            var resultado = _autenticacionService.Login(request.Usuario, request.Clave);

            if (resultado.Exitoso)
            {
                return Ok(resultado);
            }
            return BadRequest(resultado);
        }

        /// <summary>
        /// Registra un nuevo empleado en el sistema
        /// </summary>
        /// <param name="empleado">Datos del empleado a registrar</param>
        /// <returns>RespuestaDTO indicando el resultado de la operación</returns>
        [HttpPost("registrar")]
        public ActionResult<RespuestaDTO> RegistrarEmpleado([FromBody] Empleado empleado)
        {
            var resultado = _autenticacionService.RegistrarEmpleado(empleado);

            if (resultado.Exitoso)
            {
                return Ok(resultado);
            }
            return BadRequest(resultado);
        }

        /// <summary>
        /// Cambia la clave de un empleado
        /// </summary>
        /// <param name="request">Objeto con código, clave actual y nueva</param>
        /// <returns>RespuestaDTO indicando el resultado de la operación</returns>
        [HttpPut("cambiar-clave")]
        public ActionResult<RespuestaDTO> CambiarClave([FromBody] CambiarClaveRequest request)
        {
            var resultado = _autenticacionService.CambiarClave(
                request.Codigo,
                request.ClaveActual,
                request.ClaveNueva
            );

            if (resultado.Exitoso)
            {
                return Ok(resultado);
            }
            return BadRequest(resultado);
        }
    }

    // DTOs para requests
    public class LoginRequest
    {
        public string Usuario { get; set; }
        public string Clave { get; set; }
    }

    public class CambiarClaveRequest
    {
        public string Codigo { get; set; }
        public string ClaveActual { get; set; }
        public string ClaveNueva { get; set; }
    }
}
