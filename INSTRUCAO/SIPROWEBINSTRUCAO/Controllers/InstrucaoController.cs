using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SIPROSHARED.Filtro;
using SIPROSHARED.Models;
using SIPROSHAREDINSTRUCAO.Models;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SIPROWEBINSTRUCAO.Controllers
{
    [AutorizacaoTokenAttribute("ADM", "INSTRUCAO_1")]
    public class InstrucaoController : Controller
    {
      
        private readonly HttpClient _httpClient;
        private readonly string? _baseApiUrl;
        private readonly string? _baseSipApiUrl;
        private string? userMatrix;


        public InstrucaoController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseApiUrl = configuration.GetValue<string>("InstrucaoApiUrl");
            _baseSipApiUrl = configuration.GetValue<string>("BaseSipApiUrl");
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            userMatrix = HttpContext.Items["UserMatrix"] as string;
            base.OnActionExecuting(context);
        }


        public async Task<IActionResult> Instrucao()
        {
            ViewBag.Protocolo = new List<InstrucaoProcessosModel>();
            string apiUrl = $"{_baseApiUrl}instrucao/localizar-instrucao/{userMatrix}/{"Todos"}";
            var response = await _httpClient.GetAsync(apiUrl);

            //tratamento de erro 500
            if (response.StatusCode == HttpStatusCode.InternalServerError)
                return RedirectToAction("InternalServerError", "Home");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var protocolos = await response.Content.ReadFromJsonAsync<List<InstrucaoProcessosModel>>();
                ViewBag.Protocolo = protocolos ?? new List<InstrucaoProcessosModel>();
            }
           

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> InstrucaoDetalhe(int dis_id)
        {
            string apiUrl = $"{_baseApiUrl}instrucao/get-instrucao/{dis_id}";
            var response = await _httpClient.GetAsync(apiUrl);

            //tratamento de erro 500
            if (response.StatusCode == HttpStatusCode.InternalServerError)
                return RedirectToAction("InternalServerError", "Home");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var Protocolo = await response.Content.ReadFromJsonAsync<InstrucaoProcessosModel>();

                ViewBag.Notificacao = await BuscarNotificacao(Protocolo.PRT_AIT);
                ViewBag.Setor = await BuscarSetor();
                ViewBag.instrucao = await MovimentacaoInstrucao(dis_id);
                ViewBag.Anexos = await BuscarAnexoBanco(Protocolo.PRT_NUMERO);              

                return View(Protocolo);
            }

            return View(new InstrucaoProcessosModel());
        }

        [HttpGet]
        public async Task<ResultGetAitModel> BuscarNotificacao(string ait)
        {
            string apiUrl = $"{_baseSipApiUrl}ait/v1/{ait}";
            var response = await _httpClient.GetAsync(apiUrl);


            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<ResultGetAitModel>();
            else
                return new ResultGetAitModel();
        }

        [HttpGet]
        public async Task<List<SetorModel>> BuscarSetor()
        {
            string apiUrl = $"{_baseApiUrl}instrucao/buscar-setor";
            var response = await _httpClient.GetAsync(apiUrl);
            
            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<List<SetorModel>>();
             else
            return new List<SetorModel>();         
        }

        [HttpGet]
        public async Task<InstrucaoModel> MovimentacaoInstrucao(int dis_id)
        {
            string apiUrl = $"{_baseApiUrl}instrucao/movimentacao-instrucao/{dis_id}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<InstrucaoModel>();
            else
                return new InstrucaoModel();
        }

        [HttpPost]
        public async Task<IActionResult> EncaminharInstrucao(InstrucaoModel instrucao)
        {

            instrucao.INSPRO_Usuario_origem = userMatrix;

            var apiUrl = $"{_baseApiUrl}instrucao/encaminhar-instrucao";
            var response = await _httpClient.PostAsJsonAsync(apiUrl, instrucao);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
                return RedirectToAction("InternalServerError", "Home");

            if (response.StatusCode == HttpStatusCode.OK)
                return Json(new { erro = false, retorno = "Operação realizada com sucesso!" });
            else 
                return Json(new { erro = true, retorno = "Erro ao inserir!" });


        }

       
        public async Task<IActionResult> AnexarDocumentos(List<IFormFile> arquivos, ProtocoloModel protocolo)
        {
            try
            {
                ViewBag.Anexo = new List<Anexo_Model>();

                var apiUrl = $"{_baseApiUrl}instrucao/inserir-anexo";

                protocolo.PRT_ATENDENTE = userMatrix;

                // Cria o conteúdo do formulário multipart
                var content = new MultipartFormDataContent();

                // Adiciona os arquivos ao conteúdo
                foreach (var arquivo in arquivos)
                {
                    var fileContent = new StreamContent(arquivo.OpenReadStream());
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(arquivo.ContentType);
                    content.Add(fileContent, "arquivos", arquivo.FileName);
                }

                var protocoloJson = JsonSerializer.Serialize(protocolo);
                content.Add(new StringContent(protocoloJson, Encoding.UTF8, "application/json"), "protocoloJson");

                var response = await _httpClient.PostAsync(apiUrl, content);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    await response.Content.ReadAsStringAsync();

                    ViewBag.Anexos = await BuscarAnexoBanco(protocolo.PRT_NUMERO);
                    return PartialView("_AnexoInstrucao");
                }

                //Exibe o erro com detalhes da resposta!!!!! 
                var errorDetails = await response.Content.ReadAsStringAsync();
                return PartialView("_ErrorPartialView");
            }
            catch (Exception ex)
            {

                throw;
            }


        }

        [HttpGet]
        public async Task<List<AnexoModel>> BuscarAnexoBanco(string prt_numero)
        {
            //buscando os documentos necessários
            var protocolo = prt_numero.Replace("/", "");
            var apiUrl = $"{_baseApiUrl}instrucao/buscar-anexo-banco/{protocolo}/{userMatrix}";

            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)//
                return await response.Content.ReadFromJsonAsync<List<AnexoModel>>();

            return new List<AnexoModel>();

        }

        public async Task<IActionResult> ExcluirAnexo(int prodoc_id, string? prt_numero)
        {
            ViewBag.Anexo = new List<Anexo_Model>();

            var usuariomatrix = userMatrix;
            var apiUrl = $"{_baseApiUrl}instrucao/excluir-anexo/{prodoc_id}";
            var response = await _httpClient.PostAsJsonAsync(apiUrl, prodoc_id);

            if (!response.IsSuccessStatusCode)
                return PartialView("_ErrorPartialView");

            ViewBag.Anexos = await BuscarAnexoBanco(prt_numero);
            return PartialView("_AnexoInstrucao");
        }
    }



}

