using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SIPROSHARED.Filtro;
using SIPROSHARED.Models;
using SIPROSHAREDHOMOLOGACAO.Models;
using System.Net;
using System.Text.RegularExpressions;

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

        public static string RemoveHtmlTags(string input)
        {
            return Regex.Replace(input, "<.*?>", "").Replace("&nbsp;", "").Trim();
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

        //[HttpGet]
        //public async Task<List<string>> ObterImagens(string protocolo)
        //{
        //    var pasta = protocolo.Replace("/", "");
        //    var apiUrl = $"{_baseApiUrl}homologacao/listar-imagens?protocolo={Uri.EscapeDataString(pasta)}";
        //    var response = await _httpClient.GetAsync(apiUrl);

        //    if (response.StatusCode == HttpStatusCode.OK)
        //        return await response.Content.ReadFromJsonAsync<List<string>>();
        //    else
        //        return new List<string>(); // Retorna uma lista vazia em caso de erro
        //}

        [HttpGet]
        public async Task<List<JulgamentoModel>> BuscarVotacao(string Protocolo)
        {
            try
            {
                string apiUrl = $"{_baseApiUrl}homologacao/buscar-votacao/{Protocolo}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.StatusCode == HttpStatusCode.OK)
                    return await response.Content.ReadFromJsonAsync<List<JulgamentoModel>>();
                else
                    return new List<JulgamentoModel>();

            }
            catch (Exception)
            {

                throw;
            }
            
        }


        [HttpGet]
        public async Task<PartialViewResult> BuscarProtocolo(int setor, string resultado)
        {
            try
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
                string apiUrl = $"{_baseApiUrl}homologacao/buscar-parecer/{protocolo}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.StatusCode == HttpStatusCode.OK)
                { 
                    ViewBag.Parecer = await response.Content.ReadFromJsonAsync<JulgamentoModel>();
                    ViewBag.Votacao = await BuscarVotacao(prt_numero.Replace("/", ""));                   

                }
                else
                    ViewBag.Parecer = new JulgamentoModel();

                ViewBag.Parecer.Disjug_Parecer_Relatorio = RemoveHtmlTags(ViewBag.Parecer.Disjug_Parecer_Relatorio);
              
                return PartialView("_ParecerRelator");

            }
            catch (Exception)
            {

                throw;
            }
           
        }


        [HttpGet]
        public async Task<PartialViewResult> HomologacaoDetalhe(int movpro_id)
        {
            string apiUrl = $"{_baseApiUrl}homologacao/get-homologacao/{movpro_id}";
            var response = await _httpClient.GetAsync(apiUrl);
            



            if (response.StatusCode == HttpStatusCode.OK)
            {
                var Protocolo = await response.Content.ReadFromJsonAsync<HomologacaoModel>();

      
                return PartialView("_DetalhamentoProtocolo", Protocolo);
            }

            return PartialView("_DetalhamentoProtocolo", null);
        }

        [HttpGet]
        public async Task<PartialViewResult> DetalheAIT(string ait)
        {
            try
            {
                string apiUrl = $"{_baseSipApiUrl}ait/v1/{ait}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var apiAitModel = await response.Content.ReadFromJsonAsync<ResultGetAitModel>();

                    DateTime defesaNaiDate = DateTime.Parse(apiAitModel.defesanai);
                    apiAitModel.defesanai = defesaNaiDate.ToString("dd/MM/yyyy");


                    return PartialView("_DetalhamentoAIT", apiAitModel);
                }
                else
                {
                    return PartialView("_DetalhamentoAIT", null);
                }

            }
            catch (Exception ex)
            {
                throw;
            }

        }


        [HttpGet]
        public async Task<PartialViewResult> BuscarAnexoBanco(string prt_numero)
        {
            //buscando os documentos necessários
            var protocolo = prt_numero.Replace("/", "");
            var apiUrl = $"{_baseApiUrl}homologacao/buscar-anexo-banco/{protocolo}";
          
            var response = await _httpClient.GetAsync(apiUrl);
            ViewBag.Anexos = new List<AnexoModel>();

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                    return PartialView("_ErrorPartialView");
            }

            if (response.StatusCode == HttpStatusCode.OK)//
            {
                var anexos = await response.Content.ReadFromJsonAsync<List<AnexoModel>>();
                ViewBag.Anexos = anexos;
            }

            return PartialView("_Anexos");
        }

    }
}
