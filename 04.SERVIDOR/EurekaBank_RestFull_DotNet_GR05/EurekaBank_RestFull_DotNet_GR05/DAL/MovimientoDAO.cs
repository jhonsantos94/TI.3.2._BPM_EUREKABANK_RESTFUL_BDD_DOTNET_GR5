using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using EurekaBank_RestFull_DotNet_GR05.Models;

namespace EurekaBank_RestFull_DotNet_GR05.DAL
{
    /// <summary>
    /// Data Access Object para la entidad Movimiento usando Dapper
    /// </summary>
    public class MovimientoDAO
    {
        /// <summary>
        /// Inserta un nuevo movimiento en la base de datos
        /// </summary>
        /// <param name="movimiento">Movimiento a insertar</param>
        /// <returns>True si se insertó correctamente</returns>
        public bool Insertar(Movimiento movimiento)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"INSERT INTO Movimiento 
                                    (chr_cuencodigo, int_movinumero, dtt_movifecha, 
                                    chr_emplcodigo, chr_tipocodigo, dec_moviimporte, 
                                    chr_cuenreferencia)
                                    VALUES 
                                    (@CodigoCuenta, @Numero, @Fecha, @CodigoEmpleado, 
                                    @CodigoTipo, @Importe, @CuentaReferencia)";
                    
                    int filasAfectadas = conn.Execute(query, movimiento);
                    return filasAfectadas > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al insertar movimiento: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene el último número de movimiento de una cuenta
        /// </summary>
        /// <param name="codigoCuenta">Código de la cuenta</param>
        /// <returns>Último número de movimiento (0 si no tiene movimientos)</returns>
        public int ObtenerUltimoNumero(string codigoCuenta)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"SELECT ISNULL(MAX(int_movinumero), 0) 
                                    FROM Movimiento 
                                    WHERE chr_cuencodigo = @CodigoCuenta";
                    
                    return conn.ExecuteScalar<int>(query, new { CodigoCuenta = codigoCuenta });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener último número de movimiento: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene el último número de movimiento de una cuenta dentro de una transacción activa
        /// </summary>
        /// <param name="conn">Conexión activa</param>
        /// <param name="transaction">Transacción activa</param>
        /// <param name="codigoCuenta">Código de la cuenta</param>
        /// <returns>Último número de movimiento (0 si no tiene movimientos)</returns>
        public int ObtenerUltimoNumero(SqlConnection conn, SqlTransaction transaction, string codigoCuenta)
        {
            try
            {
                string query = @"SELECT ISNULL(MAX(int_movinumero), 0) 
                                FROM Movimiento 
                                WHERE chr_cuencodigo = @CodigoCuenta";
                
                return conn.ExecuteScalar<int>(query, new { CodigoCuenta = codigoCuenta }, transaction);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener último número de movimiento: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lista los movimientos de una cuenta en un rango de fechas
        /// </summary>
        /// <param name="codigoCuenta">Código de la cuenta</param>
        /// <param name="fechaInicio">Fecha de inicio</param>
        /// <param name="fechaFin">Fecha de fin</param>
        /// <returns>Lista de movimientos</returns>
        public List<Movimiento> ListarPorCuenta(string codigoCuenta, DateTime fechaInicio, DateTime fechaFin)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"SELECT 
                                    chr_cuencodigo AS CodigoCuenta,
                                    int_movinumero AS Numero,
                                    dtt_movifecha AS Fecha,
                                    chr_emplcodigo AS CodigoEmpleado,
                                    chr_tipocodigo AS CodigoTipo,
                                    dec_moviimporte AS Importe,
                                    chr_cuenreferencia AS CuentaReferencia
                                    FROM Movimiento 
                                    WHERE chr_cuencodigo = @CodigoCuenta 
                                    AND dtt_movifecha BETWEEN @FechaInicio AND @FechaFin
                                    ORDER BY dtt_movifecha DESC, int_movinumero DESC";
                    
                    return conn.Query<Movimiento>(query, new { 
                        CodigoCuenta = codigoCuenta, 
                        FechaInicio = fechaInicio, 
                        FechaFin = fechaFin 
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al listar movimientos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lista todos los movimientos de una cuenta ordenados por fecha descendente
        /// </summary>
        /// <param name="codigoCuenta">Código de la cuenta</param>
        /// <returns>Lista de movimientos</returns>
        public List<Movimiento> ListarPorCuentaDescendente(string codigoCuenta)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"SELECT 
                                    chr_cuencodigo AS CodigoCuenta,
                                    int_movinumero AS Numero,
                                    dtt_movifecha AS Fecha,
                                    chr_emplcodigo AS CodigoEmpleado,
                                    chr_tipocodigo AS CodigoTipo,
                                    dec_moviimporte AS Importe,
                                    chr_cuenreferencia AS CuentaReferencia
                                    FROM Movimiento 
                                    WHERE chr_cuencodigo = @CodigoCuenta
                                    ORDER BY dtt_movifecha DESC, int_movinumero DESC";
                    
                    return conn.Query<Movimiento>(query, new { CodigoCuenta = codigoCuenta }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al listar movimientos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene un movimiento específico
        /// </summary>
        /// <param name="codigoCuenta">Código de la cuenta</param>
        /// <param name="numeroMovimiento">Número del movimiento</param>
        /// <returns>Movimiento encontrado o null</returns>
        public Movimiento ObtenerPorNumero(string codigoCuenta, int numeroMovimiento)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"SELECT 
                                    chr_cuencodigo AS CodigoCuenta,
                                    int_movinumero AS Numero,
                                    dtt_movifecha AS Fecha,
                                    chr_emplcodigo AS CodigoEmpleado,
                                    chr_tipocodigo AS CodigoTipo,
                                    dec_moviimporte AS Importe,
                                    chr_cuenreferencia AS CuentaReferencia
                                    FROM Movimiento 
                                    WHERE chr_cuencodigo = @CodigoCuenta 
                                    AND int_movinumero = @NumeroMovimiento";
                    
                    return conn.QueryFirstOrDefault<Movimiento>(query, new { 
                        CodigoCuenta = codigoCuenta, 
                        NumeroMovimiento = numeroMovimiento 
                    });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener movimiento: {ex.Message}", ex);
            }
        }
    }
}
