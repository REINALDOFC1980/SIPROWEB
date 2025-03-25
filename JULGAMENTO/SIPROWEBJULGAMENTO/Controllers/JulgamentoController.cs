using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using SIPROSHARED.Filtro;
using SIPROSHARED.Models;
using SIPROSHAREDJULGAMENTO.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;


namespace SIPROWEBJULGAMENTO.Controllers
{
    [AutorizacaoTokenAttribute("ADM", "JULGAMENTO_1")]
    public class JulgamentoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string? _baseApiUrl;
        private readonly string? _baseSipApiUrl;
        private string? userMatrix;
        //public readonly string? userMatrix = "GRACIAMCS"; // ROSANEMSB - ADRIANAAB -ADILSONPDS; !! verificar o Voto 


        public JulgamentoController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseApiUrl = configuration.GetValue<string>("JulgamentoApiUrl");
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

        public async Task<IActionResult> Julgamento()
        {
            try
            {
                ViewBag.Protocolo = new List<ProtocoloModel>();

                string apiUrl = $"{_baseApiUrl}julgamento/localizar-processoall/{userMatrix}/{"Todos"}";
                var response = await _httpClient.GetAsync(apiUrl);

                //tratamento de erro 500
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                    return RedirectToAction("InternalServerError", "Home");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var protocolos = await response.Content.ReadFromJsonAsync<List<ProtocoloModel>>();
                    ViewBag.Protocolo = protocolos;
                }

                return View();
            }
            catch (Exception)
            {

                throw;
            }

           
        }

        public async Task<IActionResult> Assinatura()
        {
            ViewBag.Protocolo = new List<ProtocoloModel>();
            string apiUrl = $"{_baseApiUrl}julgamento/localizar-processos-assinar/{userMatrix}/{"Todos"}";
            var response = await _httpClient.GetAsync(apiUrl);

            //tratamento de erro 500
            if (response.StatusCode == HttpStatusCode.InternalServerError)
                return RedirectToAction("InternalServerError", "Home");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var protocolos = await response.Content.ReadFromJsonAsync<List<ProtocoloModel>>();
                ViewBag.Protocolo = protocolos ?? new List<ProtocoloModel>();
            }
           
            return View();
        }


        public async Task<IActionResult> Retificacao()
        {
            try
            {
                ViewBag.Protocolo = new List<ProtocoloModel>();

                string apiUrl = $"{_baseApiUrl}julgamento/localizar-retificacao/{userMatrix}/{"Todos"}";
                var response = await _httpClient.GetAsync(apiUrl);

                //tratamento de erro 500
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                    return RedirectToAction("InternalServerError", "Home");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var protocolos = await response.Content.ReadFromJsonAsync<List<ProtocoloModel>>();
                    ViewBag.Protocolo = protocolos;
                }

                return View();
            }
            catch (Exception)
            {

                throw;
            }


        }

        [HttpGet]
        public async Task<IActionResult> BuscarProtocolo(string vlobusca)
        {
            ViewBag.Protocolo = new List<ProtocoloJulgamento_Model>();

            string apiUrl = $"{_baseApiUrl}julgamento/localizar-processoall/{userMatrix}/{vlobusca}";
            var response = await _httpClient.GetAsync(apiUrl);

            //tratamento de erro 500
            if (response.StatusCode == HttpStatusCode.InternalServerError)
                return PartialView("_ErrorPartialView");

             if (response.StatusCode == HttpStatusCode.OK)
                ViewBag.Protocolo = await response.Content.ReadFromJsonAsync<List<ProtocoloJulgamento_Model>>();

            return PartialView("_ListaProtocolo");
        }

        [HttpGet]
        public async Task<IActionResult> AssinaturaDetalhe(string vlobusca)
        {

            string apiUrl = $"{_baseApiUrl}julgamento/localizar-processo/{userMatrix}/{vlobusca}";
            var response = await _httpClient.GetAsync(apiUrl);

            //tratamento de erro 500
            if (response.StatusCode == HttpStatusCode.InternalServerError)
                return RedirectToAction("InternalServerError", "Home");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var Protocolo = await response.Content.ReadFromJsonAsync<ProtocoloJulgamento_Model>();

                ViewBag.Notificacao = await BuscarNotificacao(vlobusca);
                ViewBag.Condutor = await BuscarCondutor(vlobusca);
                ViewBag.Setor = await BuscarSetor();
                ViewBag.Membro = await BuscarMembro();
                ViewBag.MotivoVoto = await BuscarMotivoVoto();
                ViewBag.ParecerRelator = await BuscarParecerRelator(Protocolo.DIS_ID);
                ViewBag.Votacao = await BuscarVotacao(Protocolo.DIS_ID);
                ViewBag.instrucao = await BuscarInstrucao(Protocolo.PRT_NUMERO);
                ViewBag.Anexos = await BuscarAnexoBanco(Protocolo.PRT_NUMERO);

                return View(Protocolo);
            }

            return View(new ProtocoloJulgamento_Model());
        }

        [HttpGet]
        public async Task<IActionResult> JulgamentoDetalhe(string vlobusca)
        {

            string apiUrl = $"{_baseApiUrl}julgamento/localizar-processo/{userMatrix}/{vlobusca}";            
            var response = await _httpClient.GetAsync(apiUrl);

            //tratamento de erro 500
            if (response.StatusCode == HttpStatusCode.InternalServerError)
                return RedirectToAction("InternalServerError", "Home");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var Protocolo = await response.Content.ReadFromJsonAsync<ProtocoloJulgamento_Model>();                

                ViewBag.Notificacao = await BuscarNotificacao(vlobusca);
                ViewBag.Condutor = await BuscarCondutor(vlobusca);
                ViewBag.Setor = await BuscarSetor();
                ViewBag.Membro = await BuscarMembro();
                ViewBag.MotivoVoto = await BuscarMotivoVoto();
                ViewBag.ParecerRelator = await BuscarParecerRelator(Protocolo.DIS_ID);
                ViewBag.Votacao = await BuscarVotacao(Protocolo.DIS_ID);
                ViewBag.instrucao = await BuscarInstrucao(Protocolo.PRT_NUMERO);
                ViewBag.Anexos = await BuscarAnexoBanco( Protocolo.PRT_NUMERO);

                return View(Protocolo);
            }

            return View(new ProtocoloJulgamento_Model());
        }

        [HttpGet]
        public async Task<ResultGetAitModel> BuscarNotificacao(string ait)
        {
           
                string apiUrl = $"{_baseSipApiUrl}ait/v1/{ait}";
                var response = await _httpClient.GetAsync(apiUrl);


                 if (response.StatusCode == HttpStatusCode.OK)
                     return await response.Content.ReadFromJsonAsync<ResultGetAitModel>();


                 return new ResultGetAitModel();
                           
        }

        [HttpGet]
        public async Task<Proc_Condutor_Model> BuscarCondutor(string ait)
        {

            Proc_Condutor_Model proc_Condutor = new Proc_Condutor_Model();

            string apiUrl = $"{_baseSipApiUrl}condutor/v1/{ait}";
            var response = await _httpClient.GetAsync(apiUrl);

             if (response.StatusCode == HttpStatusCode.OK)
             {
                proc_Condutor = await response.Content.ReadFromJsonAsync<Proc_Condutor_Model>();              


                switch (proc_Condutor?.Rec_Modelocnh)
                {
                    case 2:
                        proc_Condutor.Rec_Modelocnh_Desc = "Nacional";
                    break;
                    case 3:
                        proc_Condutor.Rec_Modelocnh_Desc = "Estrangeiro";
                    break;
                }

            }
            else
            { //Pegando o condutore apresentado no ato da infração:
                apiUrl = $"{_baseSipApiUrl}ait/v1/{ait}";
                response = await _httpClient.GetAsync(apiUrl);

                 if (response.StatusCode == HttpStatusCode.OK)
                {
                    var apiAitModel = await response.Content.ReadFromJsonAsync<ResultGetAitModel>();

                    proc_Condutor.rec_Trocainf_Nomecond = apiAitModel.condutorInfratorNome;
                    proc_Condutor.rec_TrocaInf_Infracao = apiAitModel.infracaodescricao;
                    proc_Condutor.rec_TrocaInf_Local = apiAitModel.localinfracao;
                }
            }


            return proc_Condutor ?? new Proc_Condutor_Model();
        }

        [HttpGet]
        public async Task<List<SetorModel>> BuscarSetor()
        {

            string apiUrl = $"{_baseApiUrl}julgamento/buscar-setor";
            var response = await _httpClient.GetAsync(apiUrl);

             if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<List<SetorModel>>();
             else
               return new List<SetorModel>();
           
        }

        [HttpGet]
        public async Task<List<MembroModel>> BuscarMembro()
        {

            string apiUrl = $"{_baseApiUrl}julgamento/buscar-membros/{userMatrix}";
            var response = await _httpClient.GetAsync(apiUrl);

             if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<List<MembroModel>>();
            else
                return new List<MembroModel>();

        }

        [HttpGet]
        public async Task<List<MotivoVotoModel>> BuscarMotivoVoto()
        {

            string apiUrl = $"{_baseApiUrl}julgamento/buscar-MotivoVoto";
            var response = await _httpClient.GetAsync(apiUrl);

             if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<List<MotivoVotoModel>>();
            else
                return new List<MotivoVotoModel>();
        }

        [HttpGet]
        public async Task<List<JulgamentoProcessoModel>> BuscarVotacao(int vlobusca)
        {

            string apiUrl = $"{_baseApiUrl}julgamento/buscar-votacao/{vlobusca}";
            var response = await _httpClient.GetAsync(apiUrl);

             if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<List<JulgamentoProcessoModel>>();
            else
                return new List<JulgamentoProcessoModel>();
        }

        [HttpGet]
        public async Task<JulgamentoProcessoModel> BuscarParecerRelator(int Disjug_Dis_Id)
        {

            string apiUrl = $"{_baseApiUrl}julgamento/buscar-parecer_relator/{Disjug_Dis_Id}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<JulgamentoProcessoModel>();
            else
                return new JulgamentoProcessoModel();
        }

        public async Task<JulgamentoProcessoModel?> VerificarVoto(int disjug_jug_id)
        {
            var apiUrl = $"{_baseApiUrl}julgamento/verificar-voto/{userMatrix}/{disjug_jug_id}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<JulgamentoProcessoModel>();            
            else  
                return null;
        }     

        [HttpPost]
        public async Task<IActionResult> EncaminharProcessoInstrucao(InstrucaoProcessoModel instrucaoProcesso)
        {
               
            var apiUrl = $"{_baseApiUrl}julgamento/encaminhar-processo-instrucao";
            var response = await _httpClient.PostAsJsonAsync(apiUrl, instrucaoProcesso);


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
            
                return Json(new { erro = false, retorno = "Operação realizada com sucesso!" });
           

        }

        [HttpPost]
        public async Task<IActionResult> InserirVotoRelator(JulgamentoProcessoModel julgamentoProcesso)
        {                   
            var resultado = await VerificarVoto(julgamentoProcesso.Disjug_Dis_Id);
          
            //caso não retorne valor é feita a primeira inserção
            if(resultado == null){

                julgamentoProcesso.Disjug_Relator = userMatrix;

                var apiUrl = $"{_baseApiUrl}julgamento/inserir-voto-relator";
                var response = await _httpClient.PostAsJsonAsync(apiUrl, julgamentoProcesso);

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

                return Json(new { erro = false, retorno = "Operação realizada com sucesso!" , Url = @Url.Action("Julgamento", "Julgamento") });
            }
            //caso ja tenha dados verifica se usuario que esta logado ja realizou o voto
            if(resultado.Disjug_Resultado_Data != null )
            {
                return Json(new { erro = true, retorno = "Seu voto já foi confirmado!" });
            }   
            else
            {
                julgamentoProcesso.Disjug_Relator = userMatrix;
               
                var apiUrl = $"{_baseApiUrl}julgamento/inserir-voto-membro";
                var response = await _httpClient.PostAsJsonAsync(apiUrl, julgamentoProcesso);

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


                if (!response.IsSuccessStatusCode)
                    return Json(new { erro = true, retorno = "Erro ao inserir!"});
               
                return Json(new { erro = false, retorno = "Operação realizada com sucesso!" , Url = @Url.Action("Assinatura", "Julgamento") });

            }                   
                           
        }

        [HttpPost]
        public async Task<List<ResultGetAitModel>> HistoricoInfracao(string cpf_proprietario)
        {

            string apiUrl = $"{_baseSipApiUrl}ait/v1/cpfcnpj/{cpf_proprietario}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<List<ResultGetAitModel>>();
             else
                return new List<ResultGetAitModel>();
        }

        public async Task<IActionResult> AnexarDocumentos(List<IFormFile> arquivos, ProtocoloModel protocolo)
        {
            try
            {
                ViewBag.Anexo = new List<Anexo_Model>();

                var apiUrl = $"{_baseApiUrl}julgamento/inserir-anexo";

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
                    return PartialView("_AnexoJulgamento");
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
            var apiUrl = $"{_baseApiUrl}julgamento/buscar-anexo-banco/{protocolo}";

            var response = await _httpClient.GetAsync(apiUrl);
                    
            if (response.StatusCode == HttpStatusCode.OK)//
                return await response.Content.ReadFromJsonAsync<List<AnexoModel>>();

            return new List<AnexoModel>();

        }

        public async Task<IActionResult> ExcluirAnexo(int prodoc_id, string? prt_numero)
        {
            ViewBag.Anexo = new List<Anexo_Model>();

            var usuariomatrix = userMatrix;
            var apiUrl = $"{_baseApiUrl}julgamento/excluir-anexo/{prodoc_id}";
            var response = await _httpClient.PostAsJsonAsync(apiUrl, prodoc_id);

            if (!response.IsSuccessStatusCode)
                return PartialView("_ErrorPartialView");

            ViewBag.Anexos = await BuscarAnexoBanco(prt_numero);
            return PartialView("_AnexoJulgamento");
        }

        public async Task<List<InstrucaoProcessoModel>> BuscarInstrucao(string? vlobusca)
        {

            vlobusca = vlobusca.Replace("/", "");

            string apiUrl = $"{_baseApiUrl}julgamento/buscar-instrucao/{vlobusca}";
            var response = await _httpClient.GetAsync(apiUrl);

             if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<List<InstrucaoProcessoModel>>();
            else
               return new List<InstrucaoProcessoModel>();
        }
    }


}
