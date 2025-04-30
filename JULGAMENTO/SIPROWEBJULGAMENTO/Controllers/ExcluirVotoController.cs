using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SIPROSHARED.Filtro;
using SIPROSHARED.Models;
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
            ViewBag.Protocolo = await  LocalizarProcessosExcluirVoto( "Todos","Todos");
            return View();
        }


        [HttpGet]
        public async Task<PartialViewResult> BuscarProcessos(string Situacao, string Vlobusca)
        {
            ViewBag.Protocolo = await LocalizarProcessosExcluirVoto(Situacao, Vlobusca);

            return PartialView("_ListaProcesso");
        }

        [HttpGet]
        public async Task<List<JulgamentoProcessoModel>> BuscarVotacao(string Protocolo)
        {
            try
            {
                string apiUrl = $"{_baseApiUrl}homologacao/buscar-votacao/{Protocolo}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.StatusCode == HttpStatusCode.OK)
                    return await response.Content.ReadFromJsonAsync<List<JulgamentoProcessoModel>>();
                else
                    return new List<JulgamentoProcessoModel>();

            }
            catch (Exception)
            {

                throw;
            }

        }


        public async Task<List<ExcluirModel>> LocalizarProcessosExcluirVoto(string situacao, string processo)
        {

            string processo_ = (processo ?? "").Replace("/", "");
            string usuario = userMatrix;
            string situacaoEsc = string.IsNullOrWhiteSpace(situacao) ? "TODOS" : situacao;
            string processoEsc = string.IsNullOrWhiteSpace(processo_) ? "TODOS" : processo_;


            string apiUrl = $"{_baseApiUrl}julgamento/buscar-processo-excluir/{usuario}/{situacaoEsc}/{processoEsc}";

           
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<List<ExcluirModel>>();
                return data ?? new List<ExcluirModel>();
            }
            else
            {
                // Aqui você pode logar ou mostrar uma mensagem
                Console.WriteLine($"Erro na chamada: {response.StatusCode}");
                return new List<ExcluirModel>();
            }            
            
        } 

    }
}
