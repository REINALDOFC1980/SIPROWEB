﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using SIPROSHARED.Filtro;
using SIPROSHARED.Models;
using SIPROSHAREDHOMOLOGACAO.Models;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Json;
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


        private async Task<JsonResult> HandleErrorResponse(HttpResponseMessage response)
        {
            var errorResponse = await response.Content.ReadAsStringAsync();
            var errorData = JsonConvert.DeserializeObject<ErrorResponseModel>(errorResponse);
            var errorMessage = errorData?.Errors?.FirstOrDefault() ?? "Erro ao processar sua solicitação.";
            TempData["ErroMessage"] = errorMessage;
            return Json(new { error = "BadRequest", message = errorMessage });
        }


        public static string RemoveHtmlTags(string input)
        {
            return Regex.Replace(input, "<.*?>", "").Replace("&nbsp;", "").Trim();
        }

        public async Task<IActionResult> Homologacao()
        {
            ViewBag.Setor = await BuscarSetor();
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> ListaHomologar(int setor, string resultado)
        {

            ViewBag.Protocolo = new List<HomologacaoModel>();

            string apiUrl = $"{_baseApiUrl}homologacao/localizar-homologacao/{setor}/{resultado}";
            var response = await _httpClient.GetAsync(apiUrl);

            // Tratamento centralizado de erros
            var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;

            // Fluxo normal
            if (response.StatusCode == HttpStatusCode.OK)
            {
                ViewBag.Protocolo = await response.Content.ReadFromJsonAsync<List<HomologacaoModel>>();
            }
            
            return PartialView("_ListaHomologacao");
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
        public async Task<List<HomologacaoModel>> BuscarListaProtocolo(int setor, string resultado)
        {
            //buscando os documentos necessários
        
            string apiUrl = $"{_baseApiUrl}homologacao/localizar-homologacao/{setor}/{resultado}";
            var response = await _httpClient.GetAsync(apiUrl);


            if (response.StatusCode == HttpStatusCode.OK)//
                return await response.Content.ReadFromJsonAsync<List<HomologacaoModel>>();

            return new List<HomologacaoModel>();

        }

        [HttpGet]
        public async Task<HomologacaoModel> BuscarPrtHomologar(string protocolo)
        {
            
                string apiUrl = $"{_baseApiUrl}homologacao/buscar-homologacao/{protocolo}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.StatusCode == HttpStatusCode.OK)
                    return await response.Content.ReadFromJsonAsync<HomologacaoModel>();
                else
                    return new HomologacaoModel();
            

        }      

        [HttpGet]
        public async Task<PartialViewResult> BuscarParecer(string prt_numero)
        {
            try
            {
                var protocolo = prt_numero.Replace("/", "");
                string apiUrl = $"{_baseApiUrl}homologacao/buscar-parecer/{protocolo}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                        return PartialView("_ErrorPartialView");
                }
                else
                if (response.StatusCode == HttpStatusCode.OK)
                { 
                    ViewBag.Parecer = await response.Content.ReadFromJsonAsync<JulgamentoModel>();
                    ViewBag.Parecer.Disjug_Parecer_Relatorio = RemoveHtmlTags(ViewBag.Parecer.Disjug_Parecer_Relatorio);
                    
                    ViewBag.Votacao = await BuscarVotacao(prt_numero.Replace("/", ""));
                }
                else
                    ViewBag.Parecer = new JulgamentoModel();

              
                return PartialView("_ParecerRelator");

            }
            catch (Exception)
            {

                throw;
            }
           
        }

        [HttpGet]
        public async Task<PartialViewResult> HomologacaoDetalhe(string prt_numero)
        {
            var prtnumero = prt_numero.Replace("/", "");
            var Protocolo = await BuscarPrtHomologar(prtnumero);

            return PartialView("_DetalhamentoProtocolo", Protocolo);

           
        }

        [HttpGet]
        public async Task<PartialViewResult> DetalheAIT(string ait)
        {
               string apiUrl = $"{_baseSipApiUrl}ait/v1/{ait}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                        return PartialView("_ErrorPartialView");
                }
                else if (response.StatusCode == HttpStatusCode.OK)
                {
                    var apiAitModel = await response.Content.ReadFromJsonAsync<ResultGetAitModel>();

                    DateTime defesaNaiDate = DateTime.Parse(apiAitModel.defesanai);
                    apiAitModel.defesanai = defesaNaiDate.ToString("dd/MM/yyyy");

                    return PartialView("_DetalhamentoAIT", apiAitModel);
                }
                return PartialView("_DetalhamentoAIT", null);           

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
                if (response.StatusCode == HttpStatusCode.BadRequest)
                    return PartialView("_ErrorPartialView");
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                    return PartialView("_ErrorPartialView");
            }

            if (response.StatusCode == HttpStatusCode.OK)//
            {
                var anexos = await response.Content.ReadFromJsonAsync<List<AnexoModel>>();
                ViewBag.Anexos = anexos;
            }

            return PartialView("_Anexos");
        }

        [HttpPost]
        public async Task<IActionResult> RealizarHomologacao(JulgamentoModel julgamentoModel)
        {
            var prtnumero = julgamentoModel.MovPro_Prt_Numero.Replace("/", "");
           
            HomologacaoModel homologacaoModel = await BuscarPrtHomologar(prtnumero); ;

            if (homologacaoModel != null)
            {
                julgamentoModel.MovPro_id = homologacaoModel.MOVPRO_ID;
                julgamentoModel.Disjul_SetSub_Id = homologacaoModel.SETSUB_ID;
                julgamentoModel.Disjug_Homologador = userMatrix;
            }  
             
             //Realizando a homologação
            var  apiUrl = $"{_baseApiUrl}homologacao/realizar-homologacao";
            var  response = await _httpClient.PostAsJsonAsync(apiUrl, julgamentoModel);

            // Verifica erros tratados
            var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;

            if (response.StatusCode == HttpStatusCode.OK)
                ViewBag.Protocolo = await BuscarListaProtocolo(homologacaoModel.SETSUB_ID, "Todos");
        
            return PartialView("_ListaHomologacao");


        }

        [HttpPost]
        public async Task<IActionResult> HomologarTodos(int setor, string resultado)
        {
            HomologacaoModel homologacaoModel = new HomologacaoModel();

            homologacaoModel.PRT_HOMOLOGADOR = userMatrix;
            homologacaoModel.SETSUB_ID = setor;
            homologacaoModel.PRT_RESULTADO = resultado;

            string apiUrl = $"{_baseApiUrl}homologacao/homologacao-todos";
            var response = await _httpClient.PostAsJsonAsync(apiUrl, homologacaoModel);

            // Verifica erros tratados
            var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;

            if (response.StatusCode == HttpStatusCode.OK)
                ViewBag.Protocolo = await BuscarListaProtocolo(homologacaoModel.SETSUB_ID, "Todos");

            return PartialView("_ListaHomologacao");

        }

        [HttpPost]
        public async Task<IActionResult> DevolverVoto(RetificacaoModel retificacaoModel)
        {
            var prtnumero = retificacaoModel.MOVPRO_PRT_NUMERO.Replace("/", "");
            HomologacaoModel homologacaoModel = await BuscarPrtHomologar(prtnumero); 

            if (homologacaoModel != null)
            {
                retificacaoModel.MOVPRO_ID = homologacaoModel.MOVPRO_ID;               
                retificacaoModel.MOVPRO_USUARIO_ORIGEM = userMatrix;
            }

            string apiUrl = $"{_baseApiUrl}homologacao/retornar-julgamento";
            var response = await _httpClient.PostAsJsonAsync(apiUrl, retificacaoModel);
           
            // Verifica erros tratados
            var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;

            if (response.StatusCode == HttpStatusCode.OK)
                ViewBag.Protocolo = await BuscarListaProtocolo(homologacaoModel.SETSUB_ID, "Todos");

            return PartialView("_ListaHomologacao");

        }

    }
}
