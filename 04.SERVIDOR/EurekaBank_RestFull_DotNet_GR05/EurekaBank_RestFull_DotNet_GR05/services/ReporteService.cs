using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using EurekaBank_RestFull_DotNet_GR05.Models.DTOs;

namespace EurekaBank_RestFull_DotNet_GR05.Services
{
    /// <summary>
    /// Servicio de lógica de negocio para reportes
    /// </summary>
    public class ReporteService
    {
        /// <summary>
        /// Obtiene el reporte de movimientos de una cuenta ordenado por fecha descendente
        /// </summary>
        /// <param name="codigoCuenta">Código de la cuenta</param>
        /// <returns>Lista de movimientos con información detallada</returns>
        public List<MovimientoDetalleDTO> ObtenerMovimientos(string codigoCuenta)
        {
            try
            {
                using (var conn = DAL.ConexionDB.ObtenerConexion())
                {
                    string query = @"SELECT 
                                    m.chr_cuencodigo AS CodigoCuenta,
                                    m.int_movinumero AS NumeroMovimiento,
                                    m.dtt_movifecha AS Fecha,
                                    tm.vch_tipodescripcion AS TipoMovimiento,
                                    tm.vch_tipoaccion AS Accion,
                                    m.dec_moviimporte AS Importe,
                                    (e.vch_emplnombre + ' ' + e.vch_emplpaterno + ' ' + e.vch_emplmaterno) AS EmpleadoNombre,
                                    m.chr_cuenreferencia AS CuentaReferencia
                                    FROM Movimiento m
                                    INNER JOIN TipoMovimiento tm ON m.chr_tipocodigo = tm.chr_tipocodigo
                                    INNER JOIN Empleado e ON m.chr_emplcodigo = e.chr_emplcodigo
                                    WHERE m.chr_cuencodigo = @CodigoCuenta
                                    ORDER BY m.dtt_movifecha DESC, m.int_movinumero DESC";
                    
                    return conn.Query<MovimientoDetalleDTO>(query, new { CodigoCuenta = codigoCuenta }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener movimientos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene el reporte de movimientos de una cuenta en un rango de fechas
        /// </summary>
        /// <param name="codigoCuenta">Código de la cuenta</param>
        /// <param name="fechaInicio">Fecha de inicio del rango</param>
        /// <param name="fechaFin">Fecha de fin del rango</param>
        /// <returns>Lista de movimientos con información detallada</returns>
        public List<MovimientoDetalleDTO> ObtenerMovimientosPorRango(string codigoCuenta, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                using (var conn = DAL.ConexionDB.ObtenerConexion())
                {
                    string query = @"SELECT 
                                    m.chr_cuencodigo AS CodigoCuenta,
                                    m.int_movinumero AS NumeroMovimiento,
                                    m.dtt_movifecha AS Fecha,
                                    tm.vch_tipodescripcion AS TipoMovimiento,
                                    tm.vch_tipoaccion AS Accion,
                                    m.dec_moviimporte AS Importe,
                                    (e.vch_emplnombre + ' ' + e.vch_emplpaterno + ' ' + e.vch_emplmaterno) AS EmpleadoNombre,
                                    m.chr_cuenreferencia AS CuentaReferencia
                                    FROM Movimiento m
                                    INNER JOIN TipoMovimiento tm ON m.chr_tipocodigo = tm.chr_tipocodigo
                                    INNER JOIN Empleado e ON m.chr_emplcodigo = e.chr_emplcodigo
                                    WHERE m.chr_cuencodigo = @CodigoCuenta
                                    AND m.dtt_movifecha BETWEEN @FechaInicio AND @FechaFin
                                    ORDER BY m.dtt_movifecha DESC, m.int_movinumero DESC";
                    
                    return conn.Query<MovimientoDetalleDTO>(query, new { 
                        CodigoCuenta = codigoCuenta,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener movimientos por rango: {ex.Message}", ex);
            }
        }
    }
}
