using SIPROSHARED.DbContext;
using SIPROSHARED.Service.IRepository;
using SIPROSHARED.Service.Repository;
using Serilog;
using SIPROSHARED.Filtro;
using SIPROSHARED.API;
using Serilog.Events;
using SIRPOEXCEPTIONS.Log;




var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

builder.Services.AddSingleton<DapperContext>();
builder.Services.AddSingleton<IPessoaService, PessoaService>();
builder.Services.AddSingleton<IAtendimentoService, AtendimentoService>();
builder.Services.AddSingleton<IAutenticacaoService, AutenticacaoService>();
builder.Services.AddScoped<ILoggerManagerService, LoggerManagerService>();

builder.Services.AddMvc(options => options.Filters.Add(typeof(ExceptionFilter)));
builder.Services.AddHttpClient<Detran>();



// Configurar Serilog para gravar logs em um arquivo

// Configurar Serilog para gravar logs em um arquivo
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information() // <-- Mínimo que será registrado (Information ou superior)
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning) // <-- Tudo que for do ASP.NET (Microsoft) só entra se for Warning ou Erro
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .WriteTo.Console()
    //.WriteTo.File(@"C:\Logs\SIPRO\log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseExceptionHandler("/Home/InternalServerError"); // Redireciona para a ação "Error" em produção.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();