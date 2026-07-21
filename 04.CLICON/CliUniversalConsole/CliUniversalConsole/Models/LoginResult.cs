using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliUniversalConsole.Models
{
    public class LoginResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public Empleado EmpleadoInfo { get; set; } // Anidamos el objeto Empleado

        public void Print()
        {
            Console.ForegroundColor = IsSuccess ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine("\n------ Resultado del Login ------");
            Console.WriteLine($"Éxito: {IsSuccess}");
            Console.WriteLine($"Mensaje: {Message}");

            if (IsSuccess && EmpleadoInfo != null)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("Datos del Empleado:");
                Console.WriteLine($"  Nombre: {EmpleadoInfo.NombreCompleto}");
                Console.WriteLine($"  Usuario: {EmpleadoInfo.Usuario}");
                Console.WriteLine($"  Código: {EmpleadoInfo.Codigo}");
                Console.WriteLine($"  Ciudad: {EmpleadoInfo.Ciudad}");
            }
            Console.WriteLine("---------------------------------");
            Console.ResetColor();
        }
    }
}
