using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SIPROSHARED.Filtro;
using SIPROSHARED.Models;
using SIPROSHAREDINSTRUCAO.Models;
using System.IO;
using System.Net;
using System.Net.Http.Headers;

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
                ViewBag.Anexo = await ObterImagens(Protocolo.PRT_NUMERO);              

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

        
        //anexos
        [HttpPost]
        public async Task<IActionResult> UploadAnexo(List<IFormFile> arquivos, ProtocoloModel protocolo)
        {
            //if (arquivos == null || arquivos.Count == 0 || protocolo == null || string.IsNullOrEmpty(protocolo.PRT_NUMERO))
            //{
            //    return BadRequest("Arquivos ou protocolo inválido.");
            //}



            var apiUrl = $"{_baseApiUrl}instrucao/upload";

            using (var formContent = new MultipartFormDataContent())
            {
                // Adicionar os arquivos ao formulário
                foreach (var arquivo in arquivos)
                {
                    if (arquivo.Length > 0)
                    {
                        var fileContent = new StreamContent(arquivo.OpenReadStream());
                        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(arquivo.ContentType);
                        formContent.Add(fileContent, "arquivos", arquivo.FileName);
                    }
                }

                var pasta = protocolo.PRT_NUMERO.Replace("/", "");

                // Adiciona o valor sanitizado ao formulário
                formContent.Add(new StringContent(pasta), "protocolo");


                // Enviar a requisição POST para a API
                var response = await _httpClient.PostAsync(apiUrl, formContent);

                if (response.StatusCode == HttpStatusCode.InternalServerError)
                    return RedirectToAction("InternalServerError", "Home");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    ViewBag.Anexo = await ObterImagens(pasta);

                    return PartialView("_AnexoInstrucao");
                   
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return RedirectToAction("InternalServerError", "Home");
                }
            }
        }

        [HttpGet]
        public async Task<List<string>> ObterImagens(string protocolo)
        {
            var pasta = protocolo.Replace("/", "");
            var apiUrl = $"{_baseApiUrl}instrucao/listar-imagens?protocolo={Uri.EscapeDataString(pasta)}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<List<string>>();
            else
                return new List<string>(); // Retorna uma lista vazia em caso de erro
        }

        public async Task<IActionResult> ExcluirAnexo(string? prtNumero, string? arquivo)
        {
            var pasta = prtNumero.Replace("/", "");
      
            var apiUrl = $"{_baseApiUrl}instrucao/excluir-imagem/{Uri.EscapeDataString(pasta)}/{Uri.EscapeDataString(arquivo)}";
           
            var response = await _httpClient.DeleteAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.InternalServerError)
                return RedirectToAction("InternalServerError", "Home");

            if (response.StatusCode == HttpStatusCode.OK)                           
            {
                ViewBag.Anexo = await ObterImagens(pasta);
                return PartialView("_AnexoInstrucao");
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                return RedirectToAction("InternalServerError", "Home");
            }          
            
        } 
    }



}

