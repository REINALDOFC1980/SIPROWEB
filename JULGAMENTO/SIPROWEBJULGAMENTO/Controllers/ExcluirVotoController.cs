
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using SIPROSHARED.Filtro;
using SIPROSHARED.Models;
using SIPROSHAREDJULGAMENTO.Models;
using System.Net;
using System.Text.RegularExpressions;

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

        public static string RemoveHtmlTags(string input)
        {
            return Regex.Replace(input, "<.*?>", "").Replace("&nbsp;", "").Trim();
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
        public async Task<List<ExcluirDetalheModel>> BuscarVotacao(string Protocolo)
        {
           
            string apiUrl = $"{_baseApiUrl}excluirvoto/buscar-votacao/{Protocolo}";
              var response = await _httpClient.GetAsync(apiUrl);

              if (response.StatusCode == HttpStatusCode.NoContent)
                  return new List<ExcluirDetalheModel>();

            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<List<ExcluirDetalheModel>>();


            ViewBag.MensagemErro = $"Erro ao buscar os lotes: {response.StatusCode}";
            return new List<ExcluirDetalheModel>();         

        }

        [HttpGet]
        public async Task<IActionResult> BuscarParecer(string prt_numero)
        {           
            var protocolo = prt_numero.Replace("/", "");
            string apiUrl = $"{_baseApiUrl}excluirvoto/buscar-parecer/{protocolo}";
            var response = await _httpClient.GetAsync(apiUrl);

            // Verifica erros tratados
            var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                ViewBag.Parecer = await response.Content.ReadFromJsonAsync<ExcluirDetalheModel>();
                ViewBag.Parecer.Disjug_Parecer_Relatorio = RemoveHtmlTags(ViewBag.Parecer.Disjug_Parecer_Relatorio);
                ViewBag.Votacao = await BuscarVotacao(prt_numero.Replace("/", ""));
            }
            else {
                ViewBag.Parecer = new ExcluirDetalheModel();
            }

                

            return PartialView("_ParecerRelator");           
        }
   

        public async Task<List<ExcluirModel>> LocalizarProcessosExcluirVoto(string situacao, string processo)
        {

            string processo_ = (processo ?? "").Replace("/", "");
            string usuario = userMatrix;
            string situacaoEsc = string.IsNullOrWhiteSpace(situacao) ? "TODOS" : situacao;
            string processoEsc = string.IsNullOrWhiteSpace(processo_) ? "TODOS" : processo_;

            string apiUrl = $"{_baseApiUrl}excluirvoto/buscar-processo-excluir/{usuario}/{situacaoEsc}/{processoEsc}";           
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.NoContent)
                return new List<ExcluirModel>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var data = await response.Content.ReadFromJsonAsync<List<ExcluirModel>>();
                return data ?? new List<ExcluirModel>();
            }
            
            // Aqui você pode logar ou mostrar uma mensagem
            ViewBag.MensagemErro = $"Erro ao buscar os lotes: {response.StatusCode}";
            return new List<ExcluirModel>();                       
            
        }


        [HttpPost]
        public async Task<IActionResult> ConfirmarExcluirVoto([FromForm] ExcluirDetalheModel excluirModel)
        {

            excluirModel.Disjul_Usuario = userMatrix;

            string apiUrl = $"{_baseApiUrl}excluirvoto/excluir-julgamento";
            var response = await _httpClient.PostAsJsonAsync(apiUrl, excluirModel);

            // Verifica erros tratados
            var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;
         
            if (response.StatusCode == HttpStatusCode.OK)
                ViewBag.Protocolo = await LocalizarProcessosExcluirVoto(excluirModel.MovPro_Situacao, "Todos");

            return PartialView("_ListaProcesso");

        }


       

    }
}
