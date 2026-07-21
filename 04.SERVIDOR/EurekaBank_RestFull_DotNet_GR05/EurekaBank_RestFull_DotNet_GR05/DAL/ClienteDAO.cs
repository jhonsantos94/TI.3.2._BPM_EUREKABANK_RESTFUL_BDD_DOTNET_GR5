using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using EurekaBank_RestFull_DotNet_GR05.Models;

namespace EurekaBank_RestFull_DotNet_GR05.DAL
{
    /// <summary>
    /// Data Access Object para la entidad Cliente usando Dapper
    /// </summary>
    public class ClienteDAO
    {
        /// <summary>
        /// Obtiene todos los clientes de la base de datos
        /// </summary>
        /// <returns>Lista de clientes</returns>
        public List<Cliente> ObtenerTodos()
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"SELECT 
                                    chr_cliecodigo AS Codigo,
                                    vch_cliepaterno AS Paterno,
                                    vch_cliematerno AS Materno,
                                    vch_clienombre AS Nombre,
                                    chr_cliedni AS DNI,
                                    vch_clieciudad AS Ciudad,
                                    vch_cliedireccion AS Direccion,
                                    vch_clietelefono AS Telefono,
                                    vch_clieemail AS Email
                                    FROM Cliente";
                    
                    return conn.Query<Cliente>(query).ToList();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener clientes: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene un cliente por su código
        /// </summary>
        /// <param name="codigo">Código del cliente</param>
        /// <returns>Cliente encontrado o null</returns>
        public Cliente ObtenerPorCodigo(string codigo)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"SELECT 
                                    chr_cliecodigo AS Codigo,
                                    vch_cliepaterno AS Paterno,
                                    vch_cliematerno AS Materno,
                                    vch_clienombre AS Nombre,
                                    chr_cliedni AS DNI,
                                    vch_clieciudad AS Ciudad,
                                    vch_cliedireccion AS Direccion,
                                    vch_clietelefono AS Telefono,
                                    vch_clieemail AS Email
                                    FROM Cliente 
                                    WHERE chr_cliecodigo = @Codigo";
                    
                    return conn.QueryFirstOrDefault<Cliente>(query, new { Codigo = codigo });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener cliente: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Busca clientes por DNI
        /// </summary>
        /// <param name="dni">DNI del cliente</param>
        /// <returns>Cliente encontrado o null</returns>
        public Cliente ObtenerPorDNI(string dni)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"SELECT 
                                    chr_cliecodigo AS Codigo,
                                    vch_cliepaterno AS Paterno,
                                    vch_cliematerno AS Materno,
                                    vch_clienombre AS Nombre,
                                    chr_cliedni AS DNI,
                                    vch_clieciudad AS Ciudad,
                                    vch_cliedireccion AS Direccion,
                                    vch_clietelefono AS Telefono,
                                    vch_clieemail AS Email
                                    FROM Cliente 
                                    WHERE chr_cliedni = @DNI";
                    
                    return conn.QueryFirstOrDefault<Cliente>(query, new { DNI = dni });
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar cliente por DNI: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Inserta un nuevo cliente en la base de datos
        /// </summary>
        /// <param name="cliente">Cliente a insertar</param>
        /// <returns>True si se insertó correctamente</returns>
        public bool Insertar(Cliente cliente)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"INSERT INTO Cliente 
                                    (chr_cliecodigo, vch_cliepaterno, vch_cliematerno, 
                                    vch_clienombre, chr_cliedni, vch_clieciudad, vch_cliedireccion, 
                                    vch_clietelefono, vch_clieemail)
                                    VALUES 
                                    (@Codigo, @Paterno, @Materno, @Nombre, @DNI, @Ciudad, 
                                    @Direccion, @Telefono, @Email)";
                    
                    int filasAfectadas = conn.Execute(query, cliente);
                    return filasAfectadas > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al insertar cliente: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Actualiza los datos de un cliente
        /// </summary>
        /// <param name="cliente">Cliente con los datos actualizados</param>
        /// <returns>True si se actualizó correctamente</returns>
        public bool Actualizar(Cliente cliente)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = @"UPDATE Cliente SET 
                                    vch_cliepaterno = @Paterno,
                                    vch_cliematerno = @Materno,
                                    vch_clienombre = @Nombre,
                                    chr_cliedni = @DNI,
                                    vch_clieciudad = @Ciudad,
                                    vch_cliedireccion = @Direccion,
                                    vch_clietelefono = @Telefono,
                                    vch_clieemail = @Email
                                    WHERE chr_cliecodigo = @Codigo";
                    
                    int filasAfectadas = conn.Execute(query, cliente);
                    return filasAfectadas > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar cliente: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Elimina un cliente por su código
        /// </summary>
        /// <param name="codigo">Código del cliente a eliminar</param>
        /// <returns>True si se eliminó correctamente</returns>
        public bool Eliminar(string codigo)
        {
            try
            {
                using (var conn = ConexionDB.ObtenerConexion())
                {
                    string query = "DELETE FROM Cliente WHERE chr_cliecodigo = @Codigo";
                    int filasAfectadas = conn.Execute(query, new { Codigo = codigo });
                    return filasAfectadas > 0;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al eliminar cliente: {ex.Message}", ex);
            }
        }
    }
}
