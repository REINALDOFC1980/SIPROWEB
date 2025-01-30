using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SIPROSHARED.Filtro;
using SIPROSHARED.Models;
using SIPROSHAREDHOMOLOGACAO.Models;
using System.Net;

namespace SIPROWEBHOMOLOGACAO.Controllers
{
    [AutorizacaoTokenAttribute("ADM")]
    public class HomologacaoController : Controller
    {

        private readonly HttpClient _httpClient;
        private readonly string? _baseApiUrl;
        private readonly string? _baseSipApiUrl;
        private string? userMatrix;


        public HomologacaoController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseApiUrl = configuration.GetValue<string>("HomologacaoApiUrl");
            _baseSipApiUrl = configuration.GetValue<string>("BaseSipApiUrl");
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            userMatrix = HttpContext.Items["UserMatrix"] as string;
            base.OnActionExecuting(context);
        }

        public async Task<IActionResult> HomologacaoAsync()
        {
            ViewBag.Setor = await BuscarSetor();
            return View();
        }


        [HttpGet]
        public async Task<List<SetorModel>> BuscarSetor()
        {
            string apiUrl = $"{_baseApiUrl}homologacao/buscar-setor";
               var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<List<SetorModel>>();
            else
                return new List<SetorModel>();
        }



        [HttpGet]
        public async Task<PartialViewResult> BuscarProtocolo(int setor, string resultado)
        {
            ViewBag.Protocolo = new List<dynamic>();
            string apiUrl = $"{_baseApiUrl}homologacao/localizar-homologacao/{setor}/{resultado}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                ViewBag.Protocolo = new List<dynamic>();
                return PartialView("_ErrorPartialView");
            }
            if (response.StatusCode == HttpStatusCode.OK)
                ViewBag.Protocolo = await response.Content.ReadFromJsonAsync<List<HomologacaoModel>>();

            return PartialView("_ListaHomologacao");
        }


        [HttpGet]
        public async Task<PartialViewResult> InstrucaoDetalhe(int movpro_id)
        {
            string apiUrl = $"{_baseApiUrl}homologacao/get-homologacao/{movpro_id}";
            var response = await _httpClient.GetAsync(apiUrl);

            //tratamento de erro 500
            //if (response.StatusCode == HttpStatusCode.InternalServerError)
            //    return RedirectToAction("InternalServerError", "Home");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var Protocolo = await response.Content.ReadFromJsonAsync<HomologacaoModel>();

                //ViewBag.Notificacao = await BuscarNotificacao(Protocolo.PRT_AIT);
                //ViewBag.Setor = await BuscarSetor();
                //ViewBag.instrucao = await MovimentacaoInstrucao(dis_id);
                //ViewBag.Anexo = await ObterImagens(Protocolo.PRT_NUMERO);

                return PartialView("_DetalhamentoProtocolo", Protocolo);
            }

            return PartialView("_DetalhamentoProtocolo", null);
        }


    }
}
