using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SIPROSHARED.Filtro;
using SIPROSHAREDPUBLICACAO.Model;
using System.Net;

namespace SIPROWEBPUBLICACAO.Controllers
{
    [AutorizacaoTokenAttribute("ADM", "JULGAMENTO_1")]
    public class PublicacaoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string? _baseApiUrl;
        private readonly string? _baseSipApiUrl;
        private string? userMatrix;

        public PublicacaoController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseApiUrl = configuration.GetValue<string>("PublicacaoApiUrl");
            _baseSipApiUrl = configuration.GetValue<string>("BaseSipApiUrl");
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            userMatrix = HttpContext.Items["UserMatrix"] as string;
            base.OnActionExecuting(context);
        }


        public async Task<PublicacaoModel> BuscarQtdPublicar(string usuario)
        {
            PublicacaoModel publicacaoModel = new PublicacaoModel();

            string apiUrl = $"{_baseApiUrl}publicacao/quantidade-processo/{userMatrix}";
            var response = await _httpClient.GetAsync(apiUrl); // Aguarda a resposta


            if (response.StatusCode == HttpStatusCode.OK)
                publicacaoModel = await response.Content.ReadFromJsonAsync<PublicacaoModel>();

            return publicacaoModel;
        }




        [HttpGet]
        public async Task<IActionResult> Publicacao() // Torna o método assíncrono
        {
            PublicacaoModel publicacaoModel = new PublicacaoModel();


            //Buscando QTD de processos
            string apiUrl = $"{_baseApiUrl}publicacao/quantidade-processo/{userMatrix}";
            var response = await _httpClient.GetAsync(apiUrl); // Aguarda a resposta
            if (response.StatusCode == HttpStatusCode.OK)
                publicacaoModel = await response.Content.ReadFromJsonAsync<PublicacaoModel>();


            //Buscando os lotes gerados!
            ViewBag.LoteGerado = new List<PublicacaoModel>();
            apiUrl = $"{_baseApiUrl}publicacao/buscar-lote/{userMatrix}";
            response = await _httpClient.GetAsync(apiUrl);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                List<PublicacaoModel> result = await response.Content.ReadFromJsonAsync<List<PublicacaoModel>>();
                ViewBag.LoteGerado = result;
            }

             return View(publicacaoModel);
        }

        [HttpPost]
        public async Task<PartialViewResult> GerarLote()
        {

            PublicacaoModel publicacaoModel = new PublicacaoModel();

            //Gerando Lote
            string apiUrl = $"{_baseApiUrl}publicacao/gerar-lote/{userMatrix}";
            var response = await _httpClient.PostAsync(apiUrl, null); 

            if (!response.IsSuccessStatusCode)
                return PartialView("_ErrorPartialView");

            
            //Buscando os novos valores
             apiUrl = $"{_baseApiUrl}publicacao/quantidade-processo/{userMatrix}";
             response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
                return PartialView("_ErrorPartialView");

            if (response.StatusCode == HttpStatusCode.OK)
                publicacaoModel = await response.Content.ReadFromJsonAsync<PublicacaoModel>();



            //Buscando os lotes gerados!
            ViewBag.LoteGerado = new List<PublicacaoModel>();
            apiUrl = $"{_baseApiUrl}publicacao/buscar-lote/{userMatrix}";
            response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
                return PartialView("_ErrorPartialView");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                List<PublicacaoModel> result = await response.Content.ReadFromJsonAsync<List<PublicacaoModel>>();
                ViewBag.LoteGerado = result;
            }

            return PartialView("_Qtd_Publicar", publicacaoModel);
        }

    }
}
