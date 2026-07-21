using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliUniversalConsole.Models
{
    public class Empleado
    {
        public string Codigo { get; set; }
        public string Paterno { get; set; }
        public string Materno { get; set; }
        public string Nombre { get; set; }
        public string NombreCompleto { get; set; }
        public string Ciudad { get; set; }
        public string Direccion { get; set; }
        public string Usuario { get; set; }
    }
}
