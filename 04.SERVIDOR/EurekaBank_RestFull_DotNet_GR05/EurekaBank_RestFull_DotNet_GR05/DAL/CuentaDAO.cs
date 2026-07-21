using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using EurekaBank_RestFull_DotNet_GR05.Models;

namespace EurekaBank_RestFull_DotNet_GR05.DAL
{
    /// <summary>
    /// Data Access Object para la entidad Cuenta usando Dapper
    /// </summary>
    public class CuentaDAO
    {
        /// <summary>
        /// Obtiene una cuenta por su código
        /// </summary>
        /// <param name="codigo">Código de la cuenta</param>
        /// <returns>Cuenta encontrada o null</returns>
        public Cuenta ObtenerPorCodigo(string codigo)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"SELECT 
                                    chr_cuencodigo AS Codigo,
                                    chr_monecodigo AS CodigoMoneda,
                                    chr_sucucodigo AS CodigoSucursal,
                                    chr_emplcreacuenta AS CodigoEmpleadoCreador,
                                    chr_cliecodigo AS CodigoCliente,
                                    dec_cuensaldo AS Saldo,
                                    dtt_cuenfechacreacion AS FechaCreacion,
                                    vch_cuenestado AS Estado,
                                    int_cuencontmov AS ContadorMovimientos,
                                    chr_cuenclave AS Clave
                                    FROM Cuenta 
                                    WHERE chr_cuencodigo = @Codigo";
                    
                    return conn.QueryFirstOrDefault<Cuenta>(query, new { Codigo = codigo });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener cuenta: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Valida la clave de una cuenta
        /// </summary>
        /// <param name="codigoCuenta">Código de la cuenta</param>
        /// <param name="clave">Clave a validar</param>
        /// <returns>True si la clave es correcta</returns>
        public bool ValidarClave(string codigoCuenta, string clave)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"SELECT COUNT(*) 
                                    FROM Cuenta 
                                    WHERE chr_cuencodigo = @CodigoCuenta 
                                    AND chr_cuenclave = @Clave";
                    
                    int count = conn.ExecuteScalar<int>(query, new { CodigoCuenta = codigoCuenta, Clave = clave });
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al validar clave: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza el saldo de una cuenta
        /// </summary>
        /// <param name="codigoCuenta">Código de la cuenta</param>
        /// <param name="nuevoSaldo">Nuevo saldo de la cuenta</param>
        /// <returns>True si se actualizó correctamente</returns>
        public bool ActualizarSaldo(string codigoCuenta, decimal nuevoSaldo)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"UPDATE Cuenta 
                                    SET dec_cuensaldo = @NuevoSaldo 
                                    WHERE chr_cuencodigo = @CodigoCuenta";
                    
                    int filasAfectadas = conn.Execute(query, new { CodigoCuenta = codigoCuenta, NuevoSaldo = nuevoSaldo });
                    return filasAfectadas > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar saldo: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Incrementa el contador de movimientos de una cuenta
        /// </summary>
        /// <param name="codigoCuenta">Código de la cuenta</param>
        /// <returns>True si se incrementó correctamente</returns>
        public bool IncrementarContadorMovimientos(string codigoCuenta)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"UPDATE Cuenta 
                                    SET int_cuencontmov = int_cuencontmov + 1 
                                    WHERE chr_cuencodigo = @CodigoCuenta";
                    
                    int filasAfectadas = conn.Execute(query, new { CodigoCuenta = codigoCuenta });
                    return filasAfectadas > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al incrementar contador de movimientos: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene el saldo actual de una cuenta
        /// </summary>
        /// <param name="codigoCuenta">Código de la cuenta</param>
        /// <returns>Saldo de la cuenta</returns>
        public decimal ObtenerSaldo(string codigoCuenta)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"SELECT dec_cuensaldo 
                                    FROM Cuenta 
                                    WHERE chr_cuencodigo = @CodigoCuenta";
                    
                    return conn.ExecuteScalar<decimal>(query, new { CodigoCuenta = codigoCuenta });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener saldo: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lista todas las cuentas activas
        /// </summary>
        /// <returns>Lista de cuentas activas</returns>
        public List<Cuenta> ListarCuentasActivas()
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"SELECT 
                                    chr_cuencodigo AS Codigo,
                                    chr_monecodigo AS CodigoMoneda,
                                    chr_sucucodigo AS CodigoSucursal,
                                    chr_emplcreacuenta AS CodigoEmpleadoCreador,
                                    chr_cliecodigo AS CodigoCliente,
                                    dec_cuensaldo AS Saldo,
                                    dtt_cuenfechacreacion AS FechaCreacion,
                                    vch_cuenestado AS Estado,
                                    int_cuencontmov AS ContadorMovimientos,
                                    chr_cuenclave AS Clave
                                    FROM Cuenta 
                                    WHERE vch_cuenestado = 'ACTIVO'";
                    
                    return conn.Query<Cuenta>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al listar cuentas activas: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene las cuentas de un cliente específico
        /// </summary>
        /// <param name="codigoCliente">Código del cliente</param>
        /// <returns>Lista de cuentas del cliente</returns>
        public List<Cuenta> ObtenerCuentasPorCliente(string codigoCliente)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"SELECT 
                                    chr_cuencodigo AS Codigo,
                                    chr_monecodigo AS CodigoMoneda,
                                    chr_sucucodigo AS CodigoSucursal,
                                    chr_emplcreacuenta AS CodigoEmpleadoCreador,
                                    chr_cliecodigo AS CodigoCliente,
                                    dec_cuensaldo AS Saldo,
                                    dtt_cuenfechacreacion AS FechaCreacion,
                                    vch_cuenestado AS Estado,
                                    int_cuencontmov AS ContadorMovimientos,
                                    chr_cuenclave AS Clave
                                    FROM Cuenta 
                                    WHERE chr_cliecodigo = @CodigoCliente";
                    
                    return conn.Query<Cuenta>(query, new { CodigoCliente = codigoCliente }).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener cuentas del cliente: {ex.Message}", ex);
            }
        }
    }
}
