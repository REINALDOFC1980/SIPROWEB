using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using SIPROSHARED.Filtro;
using SIPROSHARED.Models;
using SIPROSHAREDPUBLICACAO.Model;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

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

        private async Task<JsonResult> HandleErrorResponse(HttpResponseMessage response)
        {
            var errorResponse = await response.Content.ReadAsStringAsync();
            var errorData = JsonConvert.DeserializeObject<ErrorResponseModel>(errorResponse);
            var errorMessage = errorData?.Errors?.FirstOrDefault() ?? "Erro ao processar sua solicitação.";
            TempData["ErroMessage"] = errorMessage;
            return Json(new { error = "BadRequest", message = errorMessage });
        }

        public async Task<PublicacaoModel> BuscarQtdPublicar(string usuario)
        {
            try
            {
                PublicacaoModel publicacaoModel = new PublicacaoModel();

                string apiUrl = $"{_baseApiUrl}publicacao/quantidade-processo/{userMatrix}";
                var response = await _httpClient.GetAsync(apiUrl); // Aguarda a resposta


                if (response.StatusCode == HttpStatusCode.OK)
                    publicacaoModel = await response.Content.ReadFromJsonAsync<PublicacaoModel>();

                return publicacaoModel;
            }
            catch (Exception ex)
            {

                throw;
            }

            
        }

        [HttpGet]
        public async Task<IActionResult> Publicacao() // Torna o método assíncrono
        {
            try
            {

                PublicacaoModel publicacaoModel = new PublicacaoModel();

                publicacaoModel = await Buscar_Qtd_Processo(userMatrix);

                ViewBag.LoteGerado = await Buscar_Lotes(userMatrix);

                return View(publicacaoModel);
            }
            catch (Exception ex)
            {

                throw;
            }

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

            publicacaoModel = await Buscar_Qtd_Processo(userMatrix);

            ViewBag.LoteGerado = await Buscar_Lotes(userMatrix);

            return PartialView("_Qtd_Publicar", publicacaoModel);
        }

        [HttpPost]
        public async Task<IActionResult> AtualizarPublicacao(PublicacaoModel publicacaoModel)
        {
            publicacaoModel.prt_usu_publicacao = userMatrix;

            var apiUrl = $"{_baseApiUrl}publicacao/atualizar-publicacao";
            var response = await _httpClient.PostAsJsonAsync(apiUrl, publicacaoModel);

            //tratamento de erro
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                    return Json(new { error = true });

                else if (response.StatusCode == HttpStatusCode.BadRequest)
                    return await HandleErrorResponse(response);

                else if (response.StatusCode == HttpStatusCode.NoContent)
                    return await HandleErrorResponse(response);

            }

             ViewBag.LoteGerado = Buscar_Lotes(userMatrix);
            return PartialView("_Qtd_Publicar");
        }

        [HttpGet]
        public async Task<List<PublicacaoModel>> Buscar_Lotes(string usuario)
        {

            //Buscando os lotes gerados!
            ViewBag.LoteGerado = new List<PublicacaoModel>();
            var apiUrl = $"{_baseApiUrl}publicacao/buscar-lotes/{userMatrix}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                List<PublicacaoModel> result = await response.Content.ReadFromJsonAsync<List<PublicacaoModel>>();
                return result;
            }

            return new List<PublicacaoModel>();

        }

        public async Task<IActionResult> Buscar_Lote(string lote)
        {
            try
            {
                PublicacaoModel publicacaoModel = new PublicacaoModel();

                var valor = lote?.Replace("/", "") ?? "";

                var apiUrl = $"{_baseApiUrl}publicacao/buscar-lote/{valor}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    publicacaoModel = await response.Content.ReadFromJsonAsync<PublicacaoModel>();

                    publicacaoModel.prt_publicacao_dom = userMatrix;


                    return Json(new { error = false, publicacaoModel });
                }

                return Json(new { error = true, message = "Lote não encontrado" });
            }
            catch (Exception ex)
            {

                throw;
            }

            
        }

        [HttpGet]
        public async Task<PublicacaoModel> Buscar_Qtd_Processo(string usuario)
        {

            //Buscando os novos valores
           var apiUrl = $"{_baseApiUrl}publicacao/quantidade-processo/{userMatrix}";
           var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
               return await response.Content.ReadFromJsonAsync<PublicacaoModel>();

            return new PublicacaoModel();

        }



        [HttpPost]
        public async Task<IActionResult> ExcluirLote(string lote)
        {
            var valor = lote?.Replace("/", "") ?? "";

            PublicacaoModel publicacaoModel = new PublicacaoModel();

            var apiUrl = $"{_baseApiUrl}publicacao/excluir-lote/{valor}";
               var response = await _httpClient.PutAsJsonAsync(apiUrl, valor); 

            if (!response.IsSuccessStatusCode)
                return Json(new { error = true, message = "Erro ao excluir o Lote." });

            publicacaoModel = await Buscar_Qtd_Processo(userMatrix);

            return PartialView("_Qtd_Publicar", publicacaoModel);
        }

    }
}
