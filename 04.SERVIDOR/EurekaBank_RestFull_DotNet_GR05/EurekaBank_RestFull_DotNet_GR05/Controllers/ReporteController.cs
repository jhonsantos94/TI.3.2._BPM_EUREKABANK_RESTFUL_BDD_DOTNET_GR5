using System;
using System.Collections.Generic;
using EurekaBank_RestFull_DotNet_GR05.Models.DTOs;
using EurekaBank_RestFull_DotNet_GR05.Services;
using Microsoft.AspNetCore.Mvc;

namespace EurekaBank_RestFull_DotNet_GR05.Controllers
{
    /// <summary>
    /// API Controller para operaciones de reportes y consultas
    /// Replica la funcionalidad del servicio SOAP ServicioReporte
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ReporteController : ControllerBase
    {
        private readonly ReporteService _reporteService;

        public ReporteController()
        {
            _reporteService = new ReporteService();
        }

        /// <summary>
        /// Obtiene todos los movimientos de una cuenta
        /// </summary>
        /// <param name="codigoCuenta">Código de la cuenta</param>
        /// <returns>Lista de movimientos con información detallada</returns>
        [HttpGet("movimientos/{codigoCuenta}")]
        public ActionResult<List<MovimientoDetalleDTO>> ObtenerMovimientos(string codigoCuenta)
        {
            try
            {
                var movimientos = _reporteService.ObtenerMovimientos(codigoCuenta);
                return Ok(movimientos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene los movimientos de una cuenta en un rango de fechas
        /// </summary>
        /// <param name="codigoCuenta">Código de la cuenta</param>
        /// <param name="fechaInicio">Fecha de inicio del rango (formato: yyyy-MM-dd)</param>
        /// <param name="fechaFin">Fecha de fin del rango (formato: yyyy-MM-dd)</param>
        /// <returns>Lista de movimientos filtrados con información detallada</returns>
        [HttpGet("movimientos/{codigoCuenta}/rango")]
        public ActionResult<List<MovimientoDetalleDTO>> ObtenerMovimientosPorRango(
            string codigoCuenta,
            [FromQuery] DateTime fechaInicio,
            [FromQuery] DateTime fechaFin)
        {
            try
            {
                var movimientos = _reporteService.ObtenerMovimientosPorRango(
                    codigoCuenta,
                    fechaInicio,
                    fechaFin
                );
                return Ok(movimientos);
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
        }
    }
}
