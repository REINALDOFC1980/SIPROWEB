


using Serilog;
using SIPROSHARED.DbContext;
using SIPROSHARED.Filtro;
using SIPROSHAREDDISTRIBUICAO.Service.IRepository;
using SIPROSHAREDDISTRIBUICAO.Service.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

builder.Services.AddSingleton<DapperContext>();
builder.Services.AddSingleton<IDistribuicaoService, DistribuicaoService>();
builder.Services.AddMvc(options => options.Filters.Add(typeof(ExceptionFilter)));



//// Configurar Serilog para gravar logs em um arquivo
//Log.Logger = new LoggerConfiguration()
//    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
//    .CreateLogger();

//builder.Logging.ClearProviders();
//builder.Logging.AddSerilog();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

//app.UseCors("AllowSpecificOrigins");
//app.MapDefaultControllerRoute();

app.Run();
