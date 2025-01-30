using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SIPROSHARED.API;
using System.Web.Services.Description;

var builder = WebApplication.CreateBuilder(args);


// Adicione os servi�os ao cont�iner.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();


// Configura��o da sess�o
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60); // Define o tempo de expira��o da sess�o
    options.Cookie.HttpOnly = true; // Define o cookie como HTTP only
    options.Cookie.IsEssential = true; // Necess�rio para conformidade com GDPR
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Remova ou comente o DeveloperExceptionPage para testar o redirecionamento
    app.UseExceptionHandler("/Home/Error"); // Redireciona para a p�gina de erro
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Redireciona para a p�gina de erro
    app.UseHsts();
}
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Middleware de autentica��o e autoriza��o
app.UseAuthentication();
app.UseAuthorization();

// Habilitar o uso da sess�o
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Autenticacao}/{action=Login}/{id?}");
     //pattern: "{controller=Home}/{action=Apresentacao}/{id?}");


app.Run();
