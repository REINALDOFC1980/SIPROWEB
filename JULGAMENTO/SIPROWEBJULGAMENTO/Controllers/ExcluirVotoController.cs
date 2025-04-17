using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SIPROSHARED.Filtro;
using SIPROSHAREDJULGAMENTO.Models;
using System.Net;

namespace SIPROWEBJULGAMENTO.Controllers
{
    [AutorizacaoTokenAttribute("ADM", "JULGAMENTO_1")]
    public class ExcluirVotoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string? _baseApiUrl;
        private readonly string? _baseSipApiUrl;
        private string? userMatrix;       


        public ExcluirVotoController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseApiUrl = configuration.GetValue<string>("JulgamentoApiUrl");
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            userMatrix = HttpContext.Items["UserMatrix"] as string;
            base.OnActionExecuting(context);
        }

        public async Task<IActionResult> ExcluirVoto()
        {
            ViewBag.Protocolo = await  LocalizarProcessosExcluirVoto();
            return View();
        }

        [HttpGet]
        public async Task<List<JulgamentoProcessoModel>> LocalizarProcessosExcluirVoto()
        {
            

            string apiUrl = $"{_baseApiUrl}buscar-processo-excluir/buscar-membros/{userMatrix}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<List<JulgamentoProcessoModel>>();
            else
                return new List<JulgamentoProcessoModel>();

        }


    }
}
