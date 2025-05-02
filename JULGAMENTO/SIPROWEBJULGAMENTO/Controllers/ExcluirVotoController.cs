
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


        private async Task<JsonResult> HandleErrorResponse(HttpResponseMessage response)
        {
            var errorResponse = await response.Content.ReadAsStringAsync();
            var errorData = JsonConvert.DeserializeObject<ErrorResponseModel>(errorResponse);
            var errorMessage = errorData?.Errors?.FirstOrDefault() ?? "Erro ao processar sua solicitação.";
            TempData["ErroMessage"] = errorMessage;
            return Json(new { error = "BadRequest", message = errorMessage });
        }

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
        public async Task<List<ExcluirDetalheModel>> BuscarVotacao(string Protocolo)
        {
            try
            {
                string apiUrl = $"{_baseApiUrl}excluirvoto/buscar-votacao/{Protocolo}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.StatusCode == HttpStatusCode.OK)
                    return await response.Content.ReadFromJsonAsync<List<ExcluirDetalheModel>>();
                else
                    return new List<ExcluirDetalheModel>();

            }
            catch (Exception)
            {

                throw;
            }

        }

        [HttpGet]
        public async Task<PartialViewResult> BuscarParecer(string prt_numero)
        {
            try
            {
                var protocolo = prt_numero.Replace("/", "");
                string apiUrl = $"{_baseApiUrl}excluirvoto/buscar-parecer/{protocolo}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                        return PartialView("_ErrorPartialView");
                }
                else
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    ViewBag.Parecer = await response.Content.ReadFromJsonAsync<ExcluirDetalheModel>();
                    ViewBag.Parecer.Disjug_Parecer_Relatorio = RemoveHtmlTags(ViewBag.Parecer.Disjug_Parecer_Relatorio);

                    ViewBag.Votacao = await BuscarVotacao(prt_numero.Replace("/", ""));
                }
                else
                    ViewBag.Parecer = new ExcluirDetalheModel();


                return PartialView("_ParecerRelator");

            }
            catch (Exception)
            {

                throw;
            }

        }


        public static string RemoveHtmlTags(string input)
        {
            return Regex.Replace(input, "<.*?>", "").Replace("&nbsp;", "").Trim();
        }

        public async Task<List<ExcluirModel>> LocalizarProcessosExcluirVoto(string situacao, string processo)
        {

            string processo_ = (processo ?? "").Replace("/", "");
            string usuario = userMatrix;
            string situacaoEsc = string.IsNullOrWhiteSpace(situacao) ? "TODOS" : situacao;
            string processoEsc = string.IsNullOrWhiteSpace(processo_) ? "TODOS" : processo_;

            string apiUrl = $"{_baseApiUrl}excluirvoto/buscar-processo-excluir/{usuario}/{situacaoEsc}/{processoEsc}";
           
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


        [HttpPost]
        public async Task<IActionResult> ConfirmarExcluirVoto(ExcluirDetalheModel excluirModel,  string Vlobusca)
        {
    
            excluirModel.Disjul_Usuario = userMatrix;
            string apiUrl = $"{_baseApiUrl}excluirvoto/excluir-julgamento";
            var response = await _httpClient.PostAsJsonAsync(apiUrl, excluirModel);

            if (response.StatusCode == HttpStatusCode.BadRequest)
                return await HandleErrorResponse(response);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
                return PartialView("_ErrorPartialView");


            if (response.StatusCode == HttpStatusCode.OK)
                ViewBag.Protocolo = await LocalizarProcessosExcluirVoto(excluirModel.MovPro_Situacao, "Todos");

            return PartialView("_ListaProcesso");

        }


        [HttpGet]
        public async Task<ExcluirModel> BuscarPrtHomologar(string protocolo)
        {
            try
            {
                string apiUrl = $"{_baseApiUrl}homologacao/buscar-homologacao/{protocolo}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.StatusCode == HttpStatusCode.OK)
                    return await response.Content.ReadFromJsonAsync<ExcluirModel>();
                else
                    return new ExcluirModel();

            }
            catch (Exception)
            {

                throw;
            }

        }



    }
}
