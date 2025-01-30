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
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Define o tempo de expiração da sessão
    options.Cookie.HttpOnly = true; // Define o cookie como HTTP only
    options.Cookie.IsEssential = true; // Necessário para conformidade com GDPR
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Remova ou comente o DeveloperExceptionPage para testar o redirecionamento
    app.UseExceptionHandler("/Home/Error"); // Redireciona para a página de erro
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Redireciona para a página de erro
    app.UseHsts();
}
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Middleware de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

// Habilitar o uso da sessão
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Autenticacao}/{action=Login}/{id?}");
     //pattern: "{controller=Home}/{action=Apresentacao}/{id?}");


app.Run();
