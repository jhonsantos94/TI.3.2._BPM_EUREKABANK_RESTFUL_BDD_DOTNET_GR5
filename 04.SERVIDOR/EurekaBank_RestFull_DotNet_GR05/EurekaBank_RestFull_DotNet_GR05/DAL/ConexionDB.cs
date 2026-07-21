using System;
using System.Data;
using System.Data.SqlClient;

namespace EurekaBank_RestFull_DotNet_GR05.DAL
{
    /// <summary>
    /// Clase para gestionar la conexión a la base de datos SQL Server
    /// Adaptada desde SOAP .NET a RESTful .NET (ASP.NET Core)
    /// </summary>
    public class ConexionDB
    {
        private static string connectionString;

        /// <summary>
        /// Configura la cadena de conexión desde appsettings.json
        /// Se debe llamar desde Program.cs durante la inicialización
        /// </summary>
        public static void ConfigurarConexion(string connString)
        {
            connectionString = connString;
        }

        /// <summary>
        /// Crea y retorna una nueva conexión a la base de datos SQL Server
        /// </summary>
        /// <returns>SqlConnection configurada</returns>
        public static SqlConnection ObtenerConexion()
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("La cadena de conexión no ha sido configurada. Llame a ConfigurarConexion primero.");
            }
            SqlConnection conn = new SqlConnection(connectionString);
            return conn;
        }

        /// <summary>
        /// Prueba la conexión a la base de datos
        /// </summary>
        /// <returns>True si la conexión es exitosa, False en caso contrario</returns>
        public static bool ProbarConexion()
        {
            try
            {
                using (SqlConnection conn = ObtenerConexion())
                {
                    conn.Open();
                    return conn.State == ConnectionState.Open;
                }
            }
            catch (Exception ex)
            {
                // Log del error
                System.Diagnostics.Debug.WriteLine($"Error al conectar con la BD: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ejecuta un comando SQL y retorna un SqlDataReader
        /// </summary>
        /// <param name="query">Consulta SQL a ejecutar</param>
        /// <param name="parametros">Parámetros de la consulta</param>
        /// <returns>SqlDataReader con los resultados</returns>
        public static SqlDataReader EjecutarConsulta(string query, SqlParameter[] parametros = null)
        {
            SqlConnection conn = ObtenerConexion();
            SqlCommand cmd = new SqlCommand(query, conn);

            if (parametros != null)
            {
                cmd.Parameters.AddRange(parametros);
            }

            conn.Open();
            return cmd.ExecuteReader(CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// Ejecuta un comando SQL sin retornar datos (INSERT, UPDATE, DELETE)
        /// </summary>
        /// <param name="query">Comando SQL a ejecutar</param>
        /// <param name="parametros">Parámetros del comando</param>
        /// <returns>Número de filas afectadas</returns>
        public static int EjecutarComando(string query, SqlParameter[] parametros = null)
        {
            using (SqlConnection conn = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand(query, conn);

                if (parametros != null)
                {
                    cmd.Parameters.AddRange(parametros);
                }

                conn.Open();
                return cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Ejecuta un comando SQL y retorna un valor escalar
        /// </summary>
        /// <param name="query">Consulta SQL</param>
        /// <param name="parametros">Parámetros de la consulta</param>
        /// <returns>Valor escalar resultante</returns>
        public static object EjecutarEscalar(string query, SqlParameter[] parametros = null)
        {
            using (SqlConnection conn = ObtenerConexion())
            {
                SqlCommand cmd = new SqlCommand(query, conn);

                if (parametros != null)
                {
                    cmd.Parameters.AddRange(parametros);
                }

                conn.Open();
                return cmd.ExecuteScalar();
            }
        }
    }
}
