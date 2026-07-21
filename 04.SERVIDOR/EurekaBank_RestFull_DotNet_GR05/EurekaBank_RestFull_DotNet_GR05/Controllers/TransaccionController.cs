using EurekaBank_RestFull_DotNet_GR05.Models.DTOs;
using EurekaBank_RestFull_DotNet_GR05.Services;
using Microsoft.AspNetCore.Mvc;

namespace EurekaBank_RestFull_DotNet_GR05.Controllers
{
    /// <summary>
    /// API Controller para operaciones de transacciones bancarias
    /// Replica la funcionalidad del servicio SOAP ServicioTransaccion
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TransaccionController : ControllerBase
    {
        private readonly TransaccionService _transaccionService;

        public TransaccionController()
        {
            _transaccionService = new TransaccionService();
        }

        /// <summary>
        /// Realiza un depósito en una cuenta bancaria
        /// </summary>
        /// <param name="datos">Datos de la transacción (cuenta, importe, clave, empleado)</param>
        /// <returns>RespuestaDTO con el resultado del depósito</returns>
        [HttpPost("deposito")]
        public ActionResult<RespuestaDTO> RealizarDeposito([FromBody] TransaccionDTO datos)
        {
            var resultado = _transaccionService.RealizarDeposito(datos);

            if (resultado.Exitoso)
            {
                return Ok(resultado);
            }
            return BadRequest(resultado);
        }

        /// <summary>
        /// Realiza un retiro de una cuenta bancaria
        /// </summary>
        /// <param name="datos">Datos de la transacción (cuenta, importe, clave, empleado)</param>
        /// <returns>RespuestaDTO con el resultado del retiro</returns>
        [HttpPost("retiro")]
        public ActionResult<RespuestaDTO> RealizarRetiro([FromBody] TransaccionDTO datos)
        {
            var resultado = _transaccionService.RealizarRetiro(datos);

            if (resultado.Exitoso)
            {
                return Ok(resultado);
            }
            return BadRequest(resultado);
        }

        /// <summary>
        /// Realiza una transferencia entre dos cuentas bancarias
        /// </summary>
        /// <param name="datos">Datos de la transferencia (cuenta origen, cuenta destino, importe, empleado)</param>
        /// <returns>RespuestaDTO con el resultado de la transferencia</returns>
        [HttpPost("transferencia")]
        public ActionResult<RespuestaDTO> RealizarTransferencia([FromBody] TransferenciaDTO datos)
        {
            var resultado = _transaccionService.RealizarTransferencia(datos);

            if (resultado.Exitoso)
            {
                return Ok(resultado);
            }
            return BadRequest(resultado);
        }
    }
}
