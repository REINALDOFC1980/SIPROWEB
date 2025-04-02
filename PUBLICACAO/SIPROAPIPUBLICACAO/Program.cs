using SIPROSHARED.DbContext;
using SIPROSHARED.Filtro;
using SIPROSHAREDPUBLICACAO.Service.IRepository;
using SIPROSHAREDPUBLICACAO.Service.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddHttpClient();
builder.Services.AddSingleton<DapperContext>();
builder.Services.AddSingleton<IPublicacaoService, PublicacaoService>();
builder.Services.AddMvc(options => options.Filters.Add(typeof(ExceptionFilter)));
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

app.Run();
