using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SIPROSHARED.API;
using System.Web.Services.Description;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// Adicione os servi�os ao cont�iner.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();



// Configurar Serilog para gravar logs em um arquivo
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // <-- M�nimo que ser� registrado (Information ou superior)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // <-- Tudo que for do ASP.NET (Microsoft) s� entra se for Warning ou Erro
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File(@"C:\Logs\SIPRO\log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();


// Configura��o da sess�o
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(240); // Tempo de expira��o da sess�o
    options.Cookie.HttpOnly = true; // Cookie acess�vel apenas via HTTP
    options.Cookie.IsEssential = true; // Necess�rio para conformidade com GDPR
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Redireciona para a p�gina de erro
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Redireciona para a p�gina de erro
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Habilitar o uso da sess�o **antes** da autentica��o
app.UseSession();

// Middleware de autentica��o e autoriza��o
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
  //  pattern: "{controller=publicacao}/{action=publicacao}/{id?}");
pattern: "{controller=Autenticacao}/{action=Login}/{id?}");

app.Run();
