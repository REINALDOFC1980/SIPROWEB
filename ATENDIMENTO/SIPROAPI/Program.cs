using SIPROSHARED.DbContext;
using SIPROSHARED.Service.IRepository;
using SIPROSHARED.Service.Repository;
using Serilog;
using SIPROSHARED.Filtro;
using SIPROSHARED.API;




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

builder.Services.AddMvc(options => options.Filters.Add(typeof(ExceptionFilter)));
builder.Services.AddHttpClient<Detran>();



// Configurar Serilog para gravar logs em um arquivo
Log.Logger = new LoggerConfiguration()
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