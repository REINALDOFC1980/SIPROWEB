using Microsoft.AspNetCore.Mvc;
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


        public AutenticacaoController(HttpClient httpClient, IConfiguration configuration)
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

                    // Salvar o token onde necessário (por exemplo, em cookies ou na sessão)
                    HttpContext.Session.SetString("Token", token);


                    //var handler = new JwtSecurityTokenHandler();
                    //var jwtToken = handler.ReadJwtToken(token);
                    //var roleClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;

                    //var AtendRoles = new[] { "ADM", "ATENDIMENTO_ADM" };
                    //if (AtendRoles.Contains(roleClaim))
                    //    return RedirectToAction("Index", "Home");

                    // Redirecionar para a página autorizada
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

        public IActionResult Logout()
        {
            // Limpa a sessão do usuário
            HttpContext.Session.Clear();

            // Redireciona para a página de login
            return RedirectToAction("Login", "Autenticacao");
        }

    }
}
