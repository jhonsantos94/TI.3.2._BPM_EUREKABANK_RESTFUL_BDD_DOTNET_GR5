using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using EurekaBank_RestFull_DotNet_GR05.Models;

namespace EurekaBank_RestFull_DotNet_GR05.DAL
{
    /// <summary>
    /// Data Access Object para la entidad Empleado usando Dapper
    /// </summary>
    public class EmpleadoDAO
    {
        /// <summary>
        /// Valida las credenciales de un empleado
        /// </summary>
        /// <param name="usuario">Usuario del empleado</param>
        /// <param name="clave">Contraseña del empleado</param>
        /// <returns>Empleado si las credenciales son correctas, null en caso contrario</returns>
        public Empleado ValidarCredenciales(string usuario, string clave)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"SELECT 
                                    chr_emplcodigo AS Codigo,
                                    vch_emplpaterno AS Paterno,
                                    vch_emplmaterno AS Materno,
                                    vch_emplnombre AS Nombre,
                                    vch_emplciudad AS Ciudad,
                                    vch_empldireccion AS Direccion,
                                    vch_emplusuario AS Usuario,
                                    vch_emplclave AS Clave
                                    FROM Empleado 
                                    WHERE vch_emplusuario = @Usuario 
                                    AND vch_emplclave = @Clave";
                    
                    return conn.QueryFirstOrDefault<Empleado>(query, new { Usuario = usuario, Clave = clave });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al validar credenciales: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene un empleado por su código
        /// </summary>
        /// <param name="codigo">Código del empleado</param>
        /// <returns>Empleado encontrado o null</returns>
        public Empleado ObtenerPorCodigo(string codigo)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"SELECT 
                                    chr_emplcodigo AS Codigo,
                                    vch_emplpaterno AS Paterno,
                                    vch_emplmaterno AS Materno,
                                    vch_emplnombre AS Nombre,
                                    vch_emplciudad AS Ciudad,
                                    vch_empldireccion AS Direccion,
                                    vch_emplusuario AS Usuario,
                                    vch_emplclave AS Clave
                                    FROM Empleado 
                                    WHERE chr_emplcodigo = @Codigo";
                    
                    return conn.QueryFirstOrDefault<Empleado>(query, new { Codigo = codigo });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener empleado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene un empleado por su usuario
        /// </summary>
        /// <param name="usuario">Usuario del empleado</param>
        /// <returns>Empleado encontrado o null</returns>
        public Empleado ObtenerPorUsuario(string usuario)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"SELECT 
                                    chr_emplcodigo AS Codigo,
                                    vch_emplpaterno AS Paterno,
                                    vch_emplmaterno AS Materno,
                                    vch_emplnombre AS Nombre,
                                    vch_emplciudad AS Ciudad,
                                    vch_empldireccion AS Direccion,
                                    vch_emplusuario AS Usuario,
                                    vch_emplclave AS Clave
                                    FROM Empleado 
                                    WHERE vch_emplusuario = @Usuario";
                    
                    return conn.QueryFirstOrDefault<Empleado>(query, new { Usuario = usuario });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener empleado por usuario: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Registra un nuevo empleado en la base de datos
        /// </summary>
        /// <param name="empleado">Empleado a registrar</param>
        /// <returns>True si se registró correctamente</returns>
        public bool Registrar(Empleado empleado)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"INSERT INTO Empleado 
                                    (chr_emplcodigo, vch_emplpaterno, vch_emplmaterno, 
                                    vch_emplnombre, vch_emplciudad, vch_empldireccion, 
                                    vch_emplusuario, vch_emplclave)
                                    VALUES 
                                    (@Codigo, @Paterno, @Materno, @Nombre, @Ciudad, 
                                    @Direccion, @Usuario, @Clave)";
                    
                    int filasAfectadas = conn.Execute(query, empleado);
                    return filasAfectadas > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al registrar empleado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza la contraseña de un empleado
        /// </summary>
        /// <param name="codigo">Código del empleado</param>
        /// <param name="claveNueva">Nueva contraseña</param>
        /// <returns>True si se actualizó correctamente</returns>
        public bool ActualizarClave(string codigo, string claveNueva)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"UPDATE Empleado 
                                    SET vch_emplclave = @ClaveNueva 
                                    WHERE chr_emplcodigo = @Codigo";
                    
                    int filasAfectadas = conn.Execute(query, new { Codigo = codigo, ClaveNueva = claveNueva });
                    return filasAfectadas > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar contraseña: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene todos los empleados
        /// </summary>
        /// <returns>Lista de empleados</returns>
        public List<Empleado> ObtenerTodos()
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"SELECT 
                                    chr_emplcodigo AS Codigo,
                                    vch_emplpaterno AS Paterno,
                                    vch_emplmaterno AS Materno,
                                    vch_emplnombre AS Nombre,
                                    vch_emplciudad AS Ciudad,
                                    vch_empldireccion AS Direccion,
                                    vch_emplusuario AS Usuario,
                                    vch_emplclave AS Clave
                                    FROM Empleado";
                    
                    return conn.Query<Empleado>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener empleados: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Genera el siguiente código de empleado disponible
        /// </summary>
        /// <returns>Código de empleado (formato: 0001, 0002, etc.)</returns>
        public string GenerarCodigoEmpleado()
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"SELECT int_contitem 
                                    FROM Contador 
                                    WHERE vch_conttabla = 'Empleado'";
                    
                    int? contador = conn.QueryFirstOrDefault<int?>(query);
                    
                    if (contador.HasValue)
                    {
                        int nuevoContador = contador.Value + 1;
                        
                        // Actualizar contador
                        string updateQuery = @"UPDATE Contador 
                                              SET int_contitem = @NuevoContador 
                                              WHERE vch_conttabla = 'Empleado'";
                        conn.Execute(updateQuery, new { NuevoContador = nuevoContador });
                        
                        return nuevoContador.ToString("D4"); // Formato: 0001, 0002, etc.
                    }
                    else
                    {
                        throw new Exception("No se encontró el contador para Empleado");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al generar código de empleado: {ex.Message}", ex);
            }
        }
    }
}
