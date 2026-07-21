using CliUniversalConsole.Models;
using CliUniversalConsole.Services;
using CliUniversalConsole.Services.Rest;
using CliUniversalConsole.Services.Soap;
using System.Globalization;

class Program
{
    static string? codigoEmpleadoActual = null;
    static int tipoServicioActual = 0;
    static int tecnologiaActual = 0;

    static async Task Main(string[] args)
    {
        // Configurar cultura para decimales con punto
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        Console.Clear();
        MostrarBanner();

        while (true)
        {
            try
            {
                // 1. Selección de tipo de servicio
                tipoServicioActual = SeleccionarTipoServicio();
                if (tipoServicioActual == 0) break; // Salir

                // 2. Selección de tecnología
                tecnologiaActual = SeleccionarTecnologia();
                if (tecnologiaActual == 0) continue; // Volver al menú anterior

                // 3. Login obligatorio
                bool loginExitoso = await RealizarLogin();
                if (!loginExitoso)
                {
                    Console.WriteLine("\nPresione cualquier tecla para volver al menú principal...");
                    Console.ReadKey();
                    Console.Clear();
                    MostrarBanner();
                    continue;
                }

                // 4. Menú de transacciones (loop hasta que el usuario quiera salir)
                await MenuTransaccionesLoop();

                // Limpiar sesión al volver al menú principal
                codigoEmpleadoActual = null;
                Console.Clear();
                MostrarBanner();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Error inesperado: {ex.Message}");
                Console.ResetColor();
                Console.WriteLine("\nPresione cualquier tecla para continuar...");
                Console.ReadKey();
                Console.Clear();
                MostrarBanner();
            }
        }

        Console.WriteLine("\n👋 Gracias por usar el sistema EurekaBank. ¡Hasta pronto!");
    }

    static void MostrarBanner()
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════╗");
        Console.WriteLine("║                                                      ║");
        Console.WriteLine("║            🏦  SISTEMA EUREKABANK  🏦                ║");
        Console.WriteLine("║              Cliente Universal Console               ║");
        Console.WriteLine("║                                                      ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════╝");
        Console.ResetColor();
        Console.WriteLine();

        if (codigoEmpleadoActual != null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"👤 Sesión activa - Empleado: {codigoEmpleadoActual}");
            Console.ResetColor();
            Console.WriteLine();
        }
    }

    static async Task MenuTransaccionesLoop()
    {
        while (true)
        {
            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ReadKey();
            Console.Clear();
            MostrarBanner();

            var opcion = MostrarMenuTransacciones();
            if (opcion == 0)
            {
                // Cerrar sesión
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\n👋 Cerrando sesión...");
                Console.ResetColor();
                System.Threading.Thread.Sleep(1000);
                return;
            }

            ITransaccionService servicio = ObtenerServicioTransaccion(tipoServicioActual, tecnologiaActual);
            IReporteService servicioReporte = ObtenerServicioReporte(tipoServicioActual, tecnologiaActual);

            switch (opcion)
            {
                case 1:
                    await RealizarDeposito(servicio);
                    break;
                case 2:
                    await RealizarRetiro(servicio);
                    break;
                case 3:
                    await RealizarTransferencia(servicio);
                    break;
                case 4:
                    await ConsultarMovimientos(servicioReporte);
                    break;
            }
        }
    }

    static int SeleccionarTipoServicio()
    {
        Console.WriteLine("┌──────────────────────────────────────┐");
        Console.WriteLine("│   SELECCIONE EL TIPO DE SERVICIO    │");
        Console.WriteLine("└──────────────────────────────────────┘");
        Console.WriteLine("  1. REST (RESTful Web Services)");
        Console.WriteLine("  2. SOAP (Simple Object Access Protocol)");
        Console.WriteLine("  0. Salir");
        Console.Write("\nOpción: ");

        if (int.TryParse(Console.ReadLine(), out int opcion))
        {
            if (opcion >= 0 && opcion <= 2)
            {
                Console.Clear();
                MostrarBanner();
                return opcion;
            }
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n⚠️  Opción inválida. Por favor, intente nuevamente.");
        Console.ResetColor();
        System.Threading.Thread.Sleep(1500);
        Console.Clear();
        MostrarBanner();
        return SeleccionarTipoServicio();
    }

    static int SeleccionarTecnologia()
    {
        Console.WriteLine("┌──────────────────────────────────────┐");
        Console.WriteLine("│   SELECCIONE LA TECNOLOGÍA           │");
        Console.WriteLine("└──────────────────────────────────────┘");
        Console.WriteLine("  1. Java");
        Console.WriteLine("  2. .NET");
        Console.WriteLine("  0. Volver");
        Console.Write("\nOpción: ");

        if (int.TryParse(Console.ReadLine(), out int opcion))
        {
            if (opcion >= 0 && opcion <= 2)
            {
                if (opcion != 0)
                {
                    Console.Clear();
                    MostrarBanner();
                }
                return opcion;
            }
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n⚠️  Opción inválida. Por favor, intente nuevamente.");
        Console.ResetColor();
        System.Threading.Thread.Sleep(1500);
        Console.Clear();
        MostrarBanner();
        return SeleccionarTecnologia();
    }

    static async Task<bool> RealizarLogin()
    {
        IAutenticacionService servicio = ObtenerServicioAutenticacion(tipoServicioActual, tecnologiaActual);

        Console.WriteLine("\n╔════════════════════════════════════════╗");
        Console.WriteLine("║           LOGIN DE EMPLEADO            ║");
        Console.WriteLine("╚════════════════════════════════════════╝\n");

        Console.Write("Usuario: ");
        var usuario = Console.ReadLine();

        Console.Write("Clave: ");
        var clave = LeerClaveOculta();

        Console.WriteLine("\n⏳ Autenticando...\n");

        var resultado = await servicio.LoginAsync(usuario, clave);
        resultado.Print();

        if (resultado.IsSuccess && resultado.EmpleadoInfo != null)
        {
            codigoEmpleadoActual = resultado.EmpleadoInfo.Codigo;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n✅ Sesión iniciada como: {codigoEmpleadoActual}");
            Console.ResetColor();
            return true;
        }

        return false;
    }

    static int MostrarMenuTransacciones()
    {
        string tipoServicioNombre = tipoServicioActual == 1 ? "REST" : "SOAP";
        string tecnologiaNombre = tecnologiaActual == 1 ? "Java" : ".NET";

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"📡 Protocolo: {tipoServicioNombre} | 💻 Tecnología: {tecnologiaNombre}");
        Console.ResetColor();
        Console.WriteLine();

        Console.WriteLine("┌──────────────────────────────────────┐");
        Console.WriteLine("│     MENÚ DE TRANSACCIONES            │");
        Console.WriteLine("└──────────────────────────────────────┘");
        Console.WriteLine("  1. 💵 Depósito");
        Console.WriteLine("  2. 💸 Retiro");
        Console.WriteLine("  3. 🔄 Transferencia");
        Console.WriteLine("  4. 📋 Consultar Movimientos");
        Console.WriteLine("  0. 🔓 Cerrar Sesión");
        Console.Write("\nOpción: ");

        if (int.TryParse(Console.ReadLine(), out int opcion))
        {
            if (opcion >= 0 && opcion <= 4)
            {
                if (opcion != 0)
                {
                    Console.Clear();
                    MostrarBanner();
                }
                return opcion;
            }
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("\n⚠️  Opción inválida. Por favor, intente nuevamente.");
        Console.ResetColor();
        System.Threading.Thread.Sleep(1500);
        Console.Clear();
        MostrarBanner();
        return MostrarMenuTransacciones();
    }

    static async Task RealizarDeposito(ITransaccionService servicio)
    {
        Console.WriteLine("\n╔════════════════════════════════════════╗");
        Console.WriteLine("║              DEPÓSITO                  ║");
        Console.WriteLine("╚════════════════════════════════════════╝\n");

        Console.Write("Código de Cuenta: ");
        var codigoCuenta = Console.ReadLine() ?? "";

        Console.Write("Clave de Cuenta: ");
        var claveCuenta = LeerClaveOculta();

        Console.Write("Importe a Depositar: S/ ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal importe))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n❌ Importe inválido");
            Console.ResetColor();
            return;
        }

        var request = new TransaccionRequest
        {
            CodigoCuenta = codigoCuenta,
            ClaveCuenta = claveCuenta,
            Importe = importe,
            CodigoEmpleado = codigoEmpleadoActual!
        };

        Console.WriteLine("\n⏳ Procesando depósito...\n");
        var resultado = await servicio.RealizarDepositoAsync(request);

        if (resultado.IsSuccess && resultado.Data is DepositoResult depositoResult)
        {
            depositoResult.Print();
        }
        else
        {
            resultado.Print();
        }
    }

    static async Task RealizarRetiro(ITransaccionService servicio)
    {
        Console.WriteLine("\n╔════════════════════════════════════════╗");
        Console.WriteLine("║               RETIRO                   ║");
        Console.WriteLine("╚════════════════════════════════════════╝\n");

        Console.Write("Código de Cuenta: ");
        var codigoCuenta = Console.ReadLine() ?? "";

        Console.Write("Clave de Cuenta: ");
        var claveCuenta = LeerClaveOculta();

        Console.Write("Importe a Retirar: S/ ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal importe))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n❌ Importe inválido");
            Console.ResetColor();
            return;
        }

        var request = new TransaccionRequest
        {
            CodigoCuenta = codigoCuenta,
            ClaveCuenta = claveCuenta,
            Importe = importe,
            CodigoEmpleado = codigoEmpleadoActual!
        };

        Console.WriteLine("\n⏳ Procesando retiro...\n");
        var resultado = await servicio.RealizarRetiroAsync(request);

        if (resultado.IsSuccess && resultado.Data is RetiroResult retiroResult)
        {
            retiroResult.Print();
        }
        else
        {
            resultado.Print();
        }
    }

    static async Task RealizarTransferencia(ITransaccionService servicio)
    {
        Console.WriteLine("\n╔════════════════════════════════════════╗");
        Console.WriteLine("║            TRANSFERENCIA               ║");
        Console.WriteLine("╚════════════════════════════════════════╝\n");

        Console.Write("Cuenta Origen: ");
        var cuentaOrigen = Console.ReadLine() ?? "";

        Console.Write("Clave Cuenta Origen: ");
        var claveCuentaOrigen = LeerClaveOculta();

        Console.Write("Cuenta Destino: ");
        var cuentaDestino = Console.ReadLine() ?? "";

        Console.Write("Importe a Transferir: S/ ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal importe))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n❌ Importe inválido");
            Console.ResetColor();
            return;
        }

        var request = new TransferenciaRequest
        {
            CuentaOrigen = cuentaOrigen,
            ClaveCuentaOrigen = claveCuentaOrigen,
            CuentaDestino = cuentaDestino,
            Importe = importe,
            CodigoEmpleado = codigoEmpleadoActual!
        };

        Console.WriteLine("\n⏳ Procesando transferencia...\n");
        var resultado = await servicio.RealizarTransferenciaAsync(request);

        if (resultado.IsSuccess && resultado.Data is TransferenciaResult transferenciaResult)
        {
            transferenciaResult.Print();
        }
        else
        {
            resultado.Print();
        }
    }

    static IAutenticacionService ObtenerServicioAutenticacion(int tipoServicio, int tecnologia)
    {
        return (tipoServicio, tecnologia) switch
        {
            (1, 1) => new RestJavaAutenticacionService(),
            (1, 2) => new RestDotNetAutenticacionService(),
            (2, 1) => new SoapJavaAutenticacionService(),
            (2, 2) => new SoapDotNetAutenticacionService(),
            _ => throw new InvalidOperationException("Combinación de servicio y tecnología no válida")
        };
    }

    static ITransaccionService ObtenerServicioTransaccion(int tipoServicio, int tecnologia)
    {
        return (tipoServicio, tecnologia) switch
        {
            (1, 1) => new RestJavaTransaccionService(),
            (1, 2) => new RestDotNetTransaccionService(),
            (2, 1) => new SoapJavaTransaccionService(),
            (2, 2) => new SoapDotNetTransaccionService(),
            _ => throw new InvalidOperationException("Combinación de servicio y tecnología no válida")
        };
    }

    static IReporteService ObtenerServicioReporte(int tipoServicio, int tecnologia)
    {
        return (tipoServicio, tecnologia) switch
        {
            (1, 1) => new RestJavaReporteService(),
            (1, 2) => new RestDotNetReporteService(),
            (2, 1) => new SoapJavaReporteService(),
            (2, 2) => new SoapDotNetReporteService(),
            _ => throw new InvalidOperationException("Combinación de servicio y tecnología no válida")
        };
    }

    static async Task ConsultarMovimientos(IReporteService servicio)
    {
        Console.WriteLine("\n╔════════════════════════════════════════╗");
        Console.WriteLine("║        CONSULTAR MOVIMIENTOS           ║");
        Console.WriteLine("╚════════════════════════════════════════╝\n");

        Console.Write("Código de Cuenta: ");
        var codigoCuenta = Console.ReadLine() ?? "";

        Console.WriteLine("\n⏳ Consultando movimientos...\n");
        var movimientos = await servicio.ObtenerMovimientosAsync(codigoCuenta);

        if (movimientos.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("📭 No se encontraron movimientos para esta cuenta.");
            Console.ResetColor();
            return;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ Se encontraron {movimientos.Count} movimientos:\n");
        Console.ResetColor();

        foreach (var movimiento in movimientos)
        {
            movimiento.Print();
        }
    }

    static string LeerClaveOculta()
    {
        string clave = string.Empty;
        ConsoleKeyInfo key;

        do
        {
            key = Console.ReadKey(true);

            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
            {
                clave += key.KeyChar;
                Console.Write("*");
            }
            else if (key.Key == ConsoleKey.Backspace && clave.Length > 0)
            {
                clave = clave.Substring(0, clave.Length - 1);
                Console.Write("\b \b");
            }
        }
        while (key.Key != ConsoleKey.Enter);

        Console.WriteLine();
        return clave;
    }
}
