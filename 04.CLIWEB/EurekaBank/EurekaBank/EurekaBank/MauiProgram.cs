// Asegúrate de tener todos estos 'usings'
using EurekaBank.Core.Managers;
using EurekaBank.Core.Services.Abstractions;   // <--- USING FALTANTE
using EurekaBank.Core.Services.Implementations; // <--- USING FALTANTE
using EurekaBank.Services;
using EurekaBank.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection; // Para AddHttpClient (después de instalar el paquete)
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace EurekaBank
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            // --- 1. CARGAR LA CONFIGURACIÓN ---
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream("EurekaBank.appsettings.json");
            var config = new ConfigurationBuilder()
                        .AddJsonStream(stream)
                        .Build();
            builder.Configuration.AddConfiguration(config);

            // --- 2. REGISTRAR SERVICIOS DE LA PLANTILLA ---
            builder.Services.AddSingleton<IFormFactor, FormFactor>();
            builder.Services.AddMauiBlazorWebView();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            // --- 3. REGISTRAR HTTPCLIENT ---
            // Esta línea ahora funcionará después de instalar Microsoft.Extensions.Http
            builder.Services.AddHttpClient();


            // 1. Registramos el gestor de estado como Singleton
            builder.Services.AddSingleton<Core.Managers.ApiServiceManager>();

            // 2. Registramos las implementaciones concretas como Singleton
            builder.Services.AddSingleton<SoapAuthenticationService>();
            builder.Services.AddSingleton<RestAuthenticationService>();
            builder.Services.AddSingleton<SessionService>();

            builder.Services.AddSingleton<SoapTransactionService>();
            builder.Services.AddSingleton<RestTransactionService>();

            builder.Services.AddSingleton<SoapReportService>();
            builder.Services.AddSingleton<RestReportService>();
            builder.Services.AddSingleton<IReportService, ReportServiceDispatcher>();
            // 3. ¡EL CAMBIO CLAVE! Registramos el Despachador como la implementación
            //    por defecto para IAuthenticationService. El contenedor de DI le pasará
            //    automáticamente las 3 dependencias que necesita (el manager y los 2 servicios).
            builder.Services.AddSingleton<IAuthenticationService, AuthenticationServiceDispatcher>();
            builder.Services.AddSingleton<ITransactionService, TransactionServiceDispatcher>();
            return builder.Build();
        }
    }
}