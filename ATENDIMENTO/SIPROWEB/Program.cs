using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SIPROSHARED.API;
using System.Web.Services.Description;

var builder = WebApplication.CreateBuilder(args);

// Adicione os serviços ao contêiner.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


// Configuração da sessão
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(120); // Tempo de expiração da sessão
    options.Cookie.HttpOnly = true; // Cookie acessível apenas via HTTP
    options.Cookie.IsEssential = true; // Necessário para conformidade com GDPR
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Redireciona para a página de erro
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Redireciona para a página de erro
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Habilitar o uso da sessão **antes** da autenticação
app.UseSession();

// Middleware de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Autenticacao}/{action=Login}/{id?}"); 

app.Run();
