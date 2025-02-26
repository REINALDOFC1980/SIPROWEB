using SIPROSHARED.DbContext;
using SIPROSHARED.Filtro;
using SIPROSHAREDINSTRUCAO.Service.IRepository;
using SIPROSHAREDINSTRUCAO.Service.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddSingleton<IInstrucaoService, InstrucaoService>();
builder.Services.AddSingleton<IInstrucaoDistribuicaoService, InstrucaoDistribuicaoService>();

builder.Services.AddMvc(options => options.Filters.Add(typeof(ExceptionFilter)));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/InternalServerError"); // Redireciona para a ação "Error" em produção.
    app.UseHsts();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
