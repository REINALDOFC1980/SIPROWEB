using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SIPROSHARED.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace SIPROWEB.Controllers
{
    public class AutenticacaoController : Controller
    {

        private readonly HttpClient _httpClient;
        private readonly string _baseApiUrl;

        public AutenticacaoController(HttpClient httpClient, IConfiguration configuration, ILogger<AutenticacaoController> logger)
        {
            _httpClient = httpClient;
            _baseApiUrl = configuration.GetValue<string>("AutenticacaoApiUrl");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string login, string senha)
        {
            try
            {
                var loginRequest = new AutenticacaoModel
                {
                    Usu_Login = login,
                    Usu_Senha = senha
                };


                var apiUrl = $"{_baseApiUrl}";
                var response = await _httpClient.PostAsJsonAsync(apiUrl, loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    var token = await response.Content.ReadAsStringAsync();
                    HttpContext.Session.SetString("Token", token);

                    return RedirectToAction("Apresentacao", "Home");
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    // Passar a mensagem de erro para a view
                    ViewBag.ErrorMessage = "Solicitante não encontrado.";
                    return View();
                }
                else if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Passar a mensagem de erro para a view
                    ViewBag.ErrorMessage = "Não autorizado.";
                    return View();
                }
                else
                {
                    // Tratar outros códigos de erro
                    ViewBag.ErrorMessage = "Erro na autenticação.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Ocorreu um erro interno no servidor.";
                return View();
            }
        }


        public async Task<IActionResult> Logout()
        {
            var token = HttpContext.Session.GetString("Token");

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            var login = jwtToken.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            HttpContext.Session.Clear();
            var apiUrl = $"{_baseApiUrl}logout?login={login}";
            var response = await _httpClient.GetAsync(apiUrl);
            return RedirectToAction("Login", "Autenticacao");
        }


    }
}
