// Asegúrate de tener todos estos 'usings'
using EurekaBank.Shared.Services;
using EurekaBank.Web.Components;
using EurekaBank.Web.Services;
using EurekaBank.Core.Services.Implementations;
using EurekaBank.Core.Services.Abstractions;
using EurekaBank.Core.Managers;

var builder = WebApplication.CreateBuilder(args);

// Configurar URLs para permitir conexiones externas
builder.WebHost.UseUrls("http://0.0.0.0:5000", "https://0.0.0.0:5001");

// El builder aquí ya lee appsettings.json por defecto, no hay que hacer nada.

// --- 1. REGISTRAR SERVICIOS DE LA PLANTILLA ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<IFormFactor, FormFactor>();

// --- 2. REGISTRO DE SERVICIOS CORE ---

// Forma correcta de registrar HttpClient en una app de servidor.
// Se crea una instancia por cada petición HTTP para evitar problemas de agotamiento de sockets.
builder.Services.AddHttpClient();

// Managers
builder.Services.AddSingleton<SessionService>();
builder.Services.AddSingleton<ApiServiceManager>();

// Authentication Services
builder.Services.AddSingleton<SoapAuthenticationService>();
builder.Services.AddSingleton<RestAuthenticationService>();
builder.Services.AddSingleton<IAuthenticationService, AuthenticationServiceDispatcher>();

// Transaction Services
builder.Services.AddSingleton<SoapTransactionService>();
builder.Services.AddSingleton<RestTransactionService>();
builder.Services.AddSingleton<ITransactionService, TransactionServiceDispatcher>();

// Report Services
builder.Services.AddSingleton<SoapReportService>();
builder.Services.AddSingleton<RestReportService>();
builder.Services.AddSingleton<IReportService, ReportServiceDispatcher>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

// Comentar esta línea para evitar redirección HTTPS forzada
// app.UseHttpsRedirection();

app.UseStaticFiles(); // Es importante tener esto para el CSS/JS
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(
        typeof(EurekaBank.Shared._Imports).Assembly);

app.Run();