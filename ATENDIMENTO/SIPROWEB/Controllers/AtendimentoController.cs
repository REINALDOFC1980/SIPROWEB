using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SIPROSHARED.Models;
using SIPROSHARED.Filtro;

using System.Text.Json.Serialization;
using System.Text.Json;
using Newtonsoft.Json;
using System.Text;
using SIPROSHARED.API;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using Microsoft.AspNetCore.Http;
using SIRPOEXCEPTIONS.ExceptionBase;
using System.Web.Services.Description;
using System.ServiceModel.Channels;
using System.Collections.Generic;


namespace SIPROWEB.Controllers
{
 
    [AutorizacaoTokenAttribute("ADM", "ATENDIMENTO_1")]
    public class AtendimentoController : Controller
    {

        private readonly HttpClient _httpClient;
        private readonly string? _baseApiUrl;
        private readonly string? _baseSipApiUrl;
        private readonly string? _relatorioApiUrl;
        private string? userMatrix;


        public AtendimentoController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseApiUrl = configuration.GetValue<string>("BaseApiUrl");
            _baseSipApiUrl = configuration.GetValue<string>("BaseSipApiUrl");
            _relatorioApiUrl = configuration.GetValue<string>("RelatorioApiUrl");  
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

        [HttpGet]
        public async Task<IActionResult> BuscarAgendamento(string cpf)
        {
            try
            {
                #region ##_Pessoa_##
                // Buscar pessoa
                PessoaModel pessoaDetran = null;

                string apiUrl = $"{_baseApiUrl}pessoa/getpessoa/{cpf}";
                var response = await _httpClient.GetAsync(apiUrl);

                //tratamento de erro
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                        return Json(new { error = true });

                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                        return await HandleErrorResponse(response);
                }

                if (response.StatusCode == HttpStatusCode.NoContent)
                {         
                    apiUrl = $"{_baseApiUrl}pessoa/getpessoadetram/{cpf}";
                    var detranResponse = await _httpClient.GetAsync(apiUrl);

                    if (detranResponse.StatusCode == HttpStatusCode.NoContent)
                        return Json(new { retorno = "SemDados", pessoaDetran });

                    // Checando se a segunda requisição foi bem-sucedida
                    if (detranResponse.IsSuccessStatusCode)
                    {
                        pessoaDetran = await detranResponse.Content.ReadFromJsonAsync<PessoaModel>();
                        return Json(new { retorno = "SemDados", pessoaDetran });
                    }
                                  
                }

                #endregion

                #region ## Agendamento ##
                // Buscar agendamento
                AgendaModel agendamento = null;

                apiUrl = $"{_baseApiUrl}atendimento/getagendamento/{cpf}";
                response = await _httpClient.GetAsync(apiUrl);

                //tratamento de erro!
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                        return Json(new { error = true });

                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                        return await HandleErrorResponse(response);                   
                }

                if (response.StatusCode == HttpStatusCode.NoContent)
                return Json(new { retorno = "SemAgendamento" });

                agendamento = await response.Content.ReadFromJsonAsync<AgendaModel>();

                var servico = agendamento.Ass_Nome;
                var ait = agendamento.Age_AIT;
                int codServico = agendamento.Age_Cod_Assunto;            

                if (agendamento != null && agendamento.Age_Cod_Origem == 48)
                {
                    //buscando os dados do proprietário e validando o AIT agendado
                    apiUrl = $"{_baseSipApiUrl}ait/v1/{agendamento.Age_AIT}";
                    response = await _httpClient.GetAsync(apiUrl);

                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.InternalServerError)
                            return Json(new { error = true });

                        apiUrl = $"{_baseApiUrl}atendimento/alteragendamento";
                        await _httpClient.PostAsJsonAsync(apiUrl, new { ait, codServico, situacao = "Ait Inexist."});
                        return Json(new { retorno = "AIT Desconhecido", cpf, servico, ait });
                    }

                    var multa = await response.Content.ReadFromJsonAsync<ResultGetAitModel>();
                    if (multa != null && multa.defesanai == null)
                    {
                        apiUrl = $"{_baseApiUrl}atendimento/alteragendamento";
                        await _httpClient.PostAsJsonAsync(apiUrl, new { ait, codServico, situacao = "Ait Incomp.." });
                        return Json(new { retorno = "AIT Desconhecido", cpf, servico, ait });
                    }


                    var cpfDigitado = cpf.TrimStart('0').TrimEnd('0');
                    var cpfProprietario = multa.cnh_cnpj_proprietario?.TrimStart('0').TrimEnd('0');
                  
                    return Json(new { retorno = cpfProprietario == cpfDigitado ? "Proprietario" : "Desconhecido", cpf, servico, ait });
                }
                else
                return Json(new { retorno = "SemAgendamento" });
                #endregion

            }
            catch (ErrorOnValidationException ex)
            {
                return Json(new { error = true });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Atendimento(string cpf, string conduto, string nome)
        {
  
            try
            {
                ProtocoloModel protocolo = new();
                ResultGetAitModel multa = new();
                AnalisarAbertura analisarAbertura = new();
                List<DocumentosModel> documentosModel = new();

        
                var apiUrl = $"{_baseApiUrl}atendimento/getagendamento/{cpf}";
                var response = await _httpClient.GetAsync(apiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                        return RedirectToAction("InternalServerError", "Home");                   
                }

                if (response.IsSuccessStatusCode)
                {
                    var Agendamento = await response.Content.ReadFromJsonAsync<AgendaModel>();
        
                    protocolo.PRT_ATENDENTE = userMatrix;
                    protocolo.PRT_ORIGEM = Agendamento.Age_Cod_Origem;
                    protocolo.PRT_NOME_ORIGEM = Agendamento.Ori_Descricao;
                    protocolo.PRT_DT_ABERTURA = Agendamento.Age_Abertura;                    
                    protocolo.PRT_ASSUNTO = Agendamento.Age_Cod_Assunto;
                    protocolo.PRT_NOME_ASSUNTO = Agendamento.Ass_Nome;
                    protocolo.PRT_TIPO_SOLICITANTE = "Portador";

                    //seja estrangeiro
                    if (conduto == "estrangeiro")
                    {
                        protocolo.PRT_CNH_ESTRANGEIRA = Agendamento.Age_Doc_Solicitante;
                        protocolo.PRT_CNH_ESTRANGEIRA_NOME = nome;
                        protocolo.PRT_CNH_ESTRANGEIRA_PAIS = "";
                        protocolo.PRT_TIPO_SOLICITANTE = "Estrangeiro";
                        protocolo.PRT_NOME_SOLICITANTE = nome;
                    }

                    //Buscando dados do AIT
                    apiUrl = $"{_baseSipApiUrl}ait/v1/{Agendamento.Age_AIT}";
                    response = await _httpClient.GetAsync(apiUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        TempData["ErroMessage"] = response.StatusCode + " - ait/v1..";                        
                        return RedirectToAction("InternalServerError", "Home"); 
                    }  


                    else
                    {
                        //Analisando se o AIT está com todos os dados pertinentes para abertura do processo.
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                        };

                        //buscando cpf do proprietário e do solicitante para saber se é proprietário
                        multa = await response.Content.ReadFromJsonAsync<ResultGetAitModel>();

                        //verificando se o CPF digitado é do Proprietário!
                        string? cpf_proprietario = multa.cnh_cnpj_proprietario.TrimStart('0').TrimEnd('0');
                        string? cpfDigitado = cpf.TrimStart('0').TrimEnd('0');

                        if (cpf_proprietario == cpfDigitado)
                        {
                            analisarAbertura.Proprietatario = multa.cnh_cnpj_proprietario;
                            protocolo.PRT_TIPO_SOLICITANTE = "Proprietário";
                        }

                        //passando os dados da multa para o protocolo 
                        protocolo.PRT_AIT = multa.rec_ait_numero;
                        protocolo.PRT_AIT_SITUACAO = multa.situacao;
                        protocolo.PRT_PLACA = multa.Rec_Veiculo_Placa;
                        protocolo.PRT_DT_COMETIMENTO = multa.infracaodata;
                        protocolo.PRT_CPFCNJ_PROPRIETARIO = multa.cnh_cnpj_proprietario;
                        protocolo.PRT_NOMEPROPRIETARIO = multa.proprietario;
                        protocolo.PRT_CIDADE_PROPRIETARIO = multa.rec_veiculo_municipio;

                        DateTime defesaNaiDate = DateTime.Parse(multa.defesanai);
                        protocolo.PRT_DT_PRAZO = defesaNaiDate.ToString("dd/MM/yyyy");          

                    }

                    //buscando os documentos necessários                  

                    apiUrl = $"{_baseApiUrl}atendimento/getdocanexo/{Agendamento.Age_Cod_Assunto}";
                    response = await _httpClient.GetAsync(apiUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.InternalServerError)
                            return RedirectToAction("InternalServerError", "Home");
                    }

                    if (response.StatusCode == HttpStatusCode.OK)
                        documentosModel = await response.Content.ReadFromJsonAsync<List<DocumentosModel>>();

                   

                    //buscar dados do solicitante 
                    apiUrl = $"{_baseApiUrl}pessoa/getpessoa/{cpf}";
                    response = await _httpClient.GetAsync(apiUrl);

                    //tratamento de erro
                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.InternalServerError)
                            return RedirectToAction("InternalServerError", "Home"); 
                    }
                    else if(response.StatusCode == HttpStatusCode.OK)
                    {
                        var solicitante = await response.Content.ReadFromJsonAsync<PessoaModel>();

                        protocolo.PRT_NOME_SOLICITANTE = solicitante.pes_Nome;
                        protocolo.PRT_CPF_SOLICITANTE = solicitante.pes_CPF;

                        if (conduto == "sim")
                        {
                            protocolo.PRT_CPF_CONDUTOR = solicitante.pes_CPF;
                            ViewBag.Condutor = "Sim"; //substituir por: protocolo.PRT_TIPO_SOLICITANTE
                            protocolo.PRT_TIPO_SOLICITANTE = "Condutor";
                        }
                    }

                    var ait = protocolo.PRT_AIT;
                    var codServico = protocolo.PRT_ASSUNTO;

                    //######  ANALISE ###### || PASSAR PARA ABERTURA! AGENDAMENTO E PRESENCIAL, EXIBIR NO POP UP    

                    //##Verificando duplicidade
                    var apiUrl2 = $"{_baseApiUrl}atendimento/alteragendamento";
                      
                    apiUrl = $"{_baseApiUrl}atendimento/getduplicidade/{ait}/{codServico}";
                    response = await _httpClient.GetAsync(apiUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.InternalServerError)
                            return RedirectToAction("InternalServerError", "Home"); 
                    }
                    else
                    {
                        analisarAbertura.Duplicidade = await response.Content.ReadFromJsonAsync<int>();
                        
                        if(analisarAbertura.Duplicidade != 0)
                        await _httpClient.PostAsJsonAsync(apiUrl2, new { ait, codServico, situacao = "Duplicidade" });
                    }

                    //##Verificando se o condutor ja foi apresentado   
                    analisarAbertura.CondutorApresentado = multa.condutorInfratorNome;
                    if (!string.IsNullOrEmpty(analisarAbertura.CondutorApresentado) && new[] { 1, 39, 59 }.Contains(codServico))
                    {
                        analisarAbertura.CondutorApresentado = "SIM";
                        await _httpClient.PostAsJsonAsync(apiUrl2, new { ait, codServico, situacao = "Cond. Existente" });
                    }
                    else
                        analisarAbertura.CondutorApresentado = "NÃO";

                    //##Situacao do AIT com o tipo do serviço   
                    if (!string.IsNullOrEmpty(multa.lotenip))
                        analisarAbertura.SituacaoAIT = "NIP";
                    else
                        analisarAbertura.SituacaoAIT = "NAI";

                    if (analisarAbertura.SituacaoAIT == "NIP" && new[] { 1, 2, 39, 59 }.Contains(codServico))
                        await _httpClient.PostAsJsonAsync(apiUrl2, new { ait, codServico, situacao = "FaseNIP" });
                    else if (analisarAbertura.SituacaoAIT == "NAI" && new[] { 8, 9, 62 }.Contains(codServico))
                        await _httpClient.PostAsJsonAsync(apiUrl2, new { ait, codServico, situacao = "FaseNAI" });


                    //##Intepestividade 
                    DateTime prazoSIP = DateTime.Parse(multa.defesanai);
                    protocolo.PRT_DT_PRAZO = prazoSIP.ToString("dd/MM/yyyy");
                    var agora = DateTime.Now.Date;
                    if (agora <= prazoSIP && new[] { 1, 2, 39, 59, 8, 9, 62 }.Contains(codServico))
                        analisarAbertura.Intepestividade = "Não";
                    else
                        analisarAbertura.Intepestividade = "Sim";

                    ViewBag.Analise = analisarAbertura;
                    ViewBag.Agendamento = Agendamento;
                    ViewBag.Documento = documentosModel;
                }

                return View(protocolo);

            }
            catch (ApiException ex)
            {
                return Json(new { errorAPI = ex.ErrorMessage });

            }
            catch (Exception ex)
            {
                return Json(new { error = "Erro Interno" });
            }

        }

        [HttpGet]
        public async Task<IActionResult> AberturaAtendimento(string cpf)
        {
            AgendaModel agendaModel = new AgendaModel();

            string apiUrl = $"{_baseApiUrl}pessoa/getpessoa/{cpf}";
            var response = await _httpClient.GetAsync(apiUrl);

            //tratamento de erro!
            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                    TempData["ErroMessage"] = response.StatusCode + " (Erro no servidor ../pessoa/getpessoa)";

                else if (response.StatusCode == HttpStatusCode.BadRequest)
                    return await HandleErrorResponse(response);

                else if (response.StatusCode == HttpStatusCode.NotFound)
                    TempData["ErroMessage"] = response.StatusCode + " (Pagina não encontrada ../pessoa/getpessoa)";

                return RedirectToAction("InternalServerError", "Home"); 
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var pessoa = await response.Content.ReadFromJsonAsync<PessoaModel>();
                agendaModel.Age_Doc_Solicitante = pessoa.pes_CPF;
                agendaModel.Age_Nome_Solicitante = pessoa.pes_Nome;
            }

            return View(agendaModel);
          
        }

        [HttpGet]
        public IActionResult AberturaEstrangeiro()
        {
            return View();

        }

        [HttpGet]
        public async Task<JsonResult> BuscarCondutor(string cpf)
        {
            PessoaModel? pessoaModel = null;  

            var retornodetran = 0;

            string apiUrl = $"{_baseApiUrl}pessoa/getpessoa/{cpf}";
            var response = await _httpClient.GetAsync(apiUrl);


            //tratanto erros!
            if (response.StatusCode == HttpStatusCode.InternalServerError)
            {
                TempData["ErroMessage"] = "Erro interno (..pessoa/getpessoa) ";
                return Json(new { error = "InternalServerError" });
            }
            else if (response.StatusCode == HttpStatusCode.BadRequest)
                return await HandleErrorResponse(response);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                pessoaModel = await response.Content.ReadFromJsonAsync<PessoaModel>();
                
                //Pegando dados do Detran Para atualizar os campos no formulário
                apiUrl = $"{_baseApiUrl}pessoa/getpessoadetram/{cpf}";
                response = await _httpClient.GetAsync(apiUrl);            

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    retornodetran = 1;                  
                  
                    var pessoaDetranModel = await _httpClient.GetFromJsonAsync<PessoaModel>(apiUrl);
                    
                    pessoaModel.pes_NumRegistroCNH = pessoaDetranModel.pes_NumRegistroCNH;
                    pessoaModel.pes_DT_Validade = pessoaDetranModel.pes_DT_Validade;
                    pessoaModel.pes_UFCNH = pessoaDetranModel.pes_UFCNH;
                }              
            }
            else if (response.StatusCode == HttpStatusCode.NoContent)
            {
                apiUrl = $"{_baseApiUrl}pessoa/getpessoadetram/{cpf}";
                response = await _httpClient.GetAsync(apiUrl);
                
                if (response.StatusCode == HttpStatusCode.OK)
                {   retornodetran = 1;
                    pessoaModel = await _httpClient.GetFromJsonAsync<PessoaModel>(apiUrl);
                }
            }
             
          

            return Json(new { pessoaModel, retornodetran });

        }      

        [HttpPost]
        public async Task<JsonResult> CadastroSemAgendamento(AgendaModel agendaModel)
        {
           
            AnalisarAbertura analisarAbertura = new AnalisarAbertura();
            
            var cpf = agendaModel.Age_Doc_Solicitante;
            var ait = agendaModel.Age_AIT;
            var codservico = agendaModel.Age_Cod_Assunto;
            var nomeservico = agendaModel.Ass_Nome;                  
            
            var apiUrl2 = $"{_baseApiUrl}atendimento/alteragendamento";            
            
            var apiUrl = $"{_baseSipApiUrl}ait/v1/{ait}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
            {            
                //## Verificando se o AIT está faltando dados importantes 
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var multa = await response.Content.ReadFromJsonAsync<ResultGetAitModel>(options);
                if (multa != null)
                {
                    if (multa.defesanai == null)
                        return Json(new { retorno = "error", errorAPI = "Não encontramos registro desse <strong>AIT</strong> em nossa base de dados ou os dados estão incompletos, o que pode impactar a conclusão do processo.'" });
                }

                if (agendaModel.Age_Placa != multa.Rec_Veiculo_Placa)
                {
                    return Json(new { retorno = "error", errorAPI = "A PLACA informada não tem vÍnculo com o AIT digitado." });
                }
            
                string? cpf_proprietario = multa.cnh_cnpj_proprietario.TrimStart('0').TrimEnd('0');
                string? cpfDigitado = null;                    
                if (cpf != null)                    
                     cpfDigitado = cpf.TrimStart('0').TrimEnd('0');   
            
            
                //##Verificando se o condutor ja foi apresentado 
                analisarAbertura.CondutorApresentado = multa.condutorInfratorNome;
                if (!string.IsNullOrEmpty(analisarAbertura.CondutorApresentado) &&  new[] { 1, 39, 59 }.Contains(codservico))
                {
                    await _httpClient.PostAsJsonAsync(apiUrl2, new { ait, codservico, situacao = "Cond. Existente" });
                    return Json(new { retorno = "error", errorAPI = "Não é permitido realizar abertura para o serviço selecionado.  <br /> <strong>CONDUTOR JÁ APRESENTADO.</strong>" });
                }
            
                // Lógica condicional para definir SituacaoAIT
                if (!string.IsNullOrEmpty(multa.lotenip))
                    analisarAbertura.SituacaoAIT = "NIP";
                else
                    analisarAbertura.SituacaoAIT = "NAI";
            
                //Situacao do AIT com o tipo do serviço
                if (analisarAbertura.SituacaoAIT == "NIP" && new[] { 1, 2, 39, 59 }.Contains(codservico))
                {
                    await _httpClient.PostAsJsonAsync(apiUrl2, new { ait, codservico, situacao = "FaseNIP" });
                    return Json(new { retorno = "error", errorAPI = "Não é permitido realizar abertura para o serviço selecionado.<br/><strong> AIT EM FASE NIP.</strong>" });
                    
                }
                else if (analisarAbertura.SituacaoAIT == "NAI" && new[] { 8, 9, 62 }.Contains(codservico))
                {
                    await _httpClient.PostAsJsonAsync(apiUrl2, new { ait, codservico, situacao = "FaseNAI" });
                    return Json(new { retorno = "error", errorAPI = "Não é permitido realizar abertura para o serviço selecionado.<br/><strong> AIT EM FASE NAI.</strong>" });
                }
            
                //Verificando duplicidade
                apiUrl = $"{_baseApiUrl}atendimento/getduplicidade/{ait}/{codservico}";
                response = await _httpClient.GetAsync(apiUrl);

                if (response.StatusCode == HttpStatusCode.OK)
                { 
                    analisarAbertura.Duplicidade = await response.Content.ReadFromJsonAsync<int>();
            
                    if (analisarAbertura.Duplicidade == 1)
                    {
                        await _httpClient.PostAsJsonAsync(apiUrl2, new { ait, codservico, situacao = "Duplicidade" });                              
                        return Json(new { retorno = "error", errorAPI = "Esse Serviço solicitado já foi cadastrado para o AIT informado. <br />Verifique se foi digitado corretamente." });
                    }
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                    return Json(new { retorno = "error", errorAPI = "Erro interno!" });

            //cadastrando abertura na tabela agendamento
            apiUrl = $"{_baseApiUrl}atendimento/aberturapresencial";
                
                response = await _httpClient.PostAsJsonAsync(apiUrl, agendaModel);
               
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Json(new { retorno = "error", errorAPI = content });
                }                    
                
                ViewBag.Analise = analisarAbertura;
            
                if (cpf_proprietario == cpfDigitado)
                    return Json(new { retorno = "Proprietario", cpf });
                else
                    return Json(new { retorno = "Desconhecido", cpf });                 
            }
            else                              
            {
                return Json(new { retorno = "error", errorAPI = response.StatusCode + " - ait/v1.." });         
            }           
            
        }

        [HttpGet]
        public async Task<PartialViewResult> AnexarArquivos()
        {
            //buscando os documentos necessários
            var apiUrl = $"{_baseApiUrl}atendimento/getanexo";
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

        [HttpPost]
        public async Task<IActionResult> GerarProtocolo(ProtocoloModel protocoloModel)
        {


            if (userMatrix == null)
                return Unauthorized();

            if (protocoloModel == null)
                return RedirectToAction("InternalServerError", "Home"); 


            protocoloModel.PRT_USUARIOARQUIVO = userMatrix;

            try
            {
                var apiUrl = $"{_baseApiUrl}atendimento/addprotocolo";
                var response = await _httpClient.PostAsJsonAsync(apiUrl, protocoloModel);


                //tratamento de erro
                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                        return Json(new { error = "InternalServerError", mensagem = "Erro interno no servidor" });

                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                        return await HandleErrorResponse(response);

                }

                if (response.IsSuccessStatusCode) 
                {
                    var result = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();           


                    if (result != null && result.ContainsKey("protocolo"))
                    {
                        string protocolo = result["protocolo"];
                        protocolo = Uri.UnescapeDataString(protocolo);
                        protocoloModel.PRT_NUMERO = protocolo;                           

                        return Json(new { error = false, protocolo });
                    }
                    else
                    {
                        return Json(new { error = true, message = "Erro ao gerar o protocolo" });
                    }
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return Json(new { error = true, message = content });
                }
            }
            catch (Exception ex)
            {
                // Logar a exceção (não mostrado aqui por brevidade)
                return Json(new { error = true, message = "Erro interno do servidor" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Finalizar_Atendimento(string protocolo)
        {
            if (string.IsNullOrWhiteSpace(protocolo))
            {
                return RedirectToAction("InternalServerError", "Home"); 
            }

            try
            {
                ProtocoloModel protocoloModel = new ProtocoloModel();
                string apiUrl = $"{_baseApiUrl}atendimento/getprotocolo?protocolo={protocolo}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    protocoloModel = await response.Content.ReadFromJsonAsync<ProtocoloModel>();
                    return View(protocoloModel);
                }
                else
                    return RedirectToAction("InternalServerError", "Home"); 
            }
            catch (Exception ex)
            {
                // Log a exceção detalhada
                return RedirectToAction("InternalServerError", "Home"); 
            }
        }

        [HttpGet]
        public IActionResult LocalizarProtocolo()
        {
            return View();
        }
        
        [HttpGet]
        public async Task<PartialViewResult> BuscarProtocolo(string vlobusca)
        {
            ViewBag.Protocolo = new List<dynamic>();
            string apiUrl = $"{_baseApiUrl}atendimento/getprotocoloall?vloBusca={vlobusca}";
            var response = await _httpClient.GetAsync(apiUrl);


           if (response.StatusCode == HttpStatusCode.InternalServerError)
           {
              ViewBag.Protocolo = new List<dynamic>();
              return PartialView("_ErrorPartialView");
           }
           if (response.StatusCode == HttpStatusCode.OK)
                ViewBag.Protocolo = await response.Content.ReadFromJsonAsync<List<ProtocoloModel>>();
              
            
            return PartialView("_ListaProtocolo");            
        }

        [HttpGet]
        public async Task<PartialViewResult> DetalheProtocolo(string protocolo)
        {
            try
            {
               
                ProtocoloModel protocoloModel = new ProtocoloModel();

                string apiUrl = $"{_baseApiUrl}atendimento/getprotocolo?protocolo={protocolo}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                        return PartialView("_ErrorPartialView");

                }
            
            
                var apiProtocoloModel = await response.Content.ReadFromJsonAsync<ProtocoloModel>();
                if (apiProtocoloModel != null)
                {
                    protocoloModel = apiProtocoloModel;

                    //Dados da Infração
                    apiUrl = $"{_baseSipApiUrl}ait/v1/{protocoloModel.PRT_AIT}";
                    response = await _httpClient.GetAsync(apiUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.InternalServerError)
                            return PartialView("_ErrorPartialView");
                    }
                    else
                    {   var multa = await response.Content.ReadFromJsonAsync<ResultGetAitModel>();
                        protocoloModel.PRT_PLACA = multa.Rec_Veiculo_Placa;
                        protocoloModel.PRT_DT_COMETIMENTO = multa.infracaodata;
              
                        DateTime defesaNaiDate = DateTime.Parse(multa.defesanai);
                        protocoloModel.PRT_DT_PRAZO = defesaNaiDate.ToString("dd/MM/yyyy");
                    }           
                }
                else
                    protocoloModel = new ProtocoloModel();
                
                return PartialView("_DetalhamentoProtocolo", protocoloModel);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpGet]
        public async Task<PartialViewResult> MovimentacaoProtocolo(string protocolo)
        {
                ViewBag.Movimentacao = new List<MovimentacaoModel>();
               
                string apiUrl = $"{_baseApiUrl}atendimento/getmovimentacao?protocolo={protocolo}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                        return PartialView("_ErrorPartialView");
                }
           
                if (response.StatusCode == HttpStatusCode.OK)
                    ViewBag.Movimentacao = await response.Content.ReadFromJsonAsync<List<MovimentacaoModel>>();

            return PartialView("_Movimentacao");
        }

        [HttpGet]
        public async Task<PartialViewResult> Proprietario_Condutor(string protocolo)
        {
                ProtocoloModel protocoloModel = new ProtocoloModel();

                string apiUrl = $"{_baseApiUrl}atendimento/getprotocolo?protocolo={protocolo}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                        return PartialView("_ErrorPartialView");

                }
                else if (response.StatusCode == HttpStatusCode.OK)
                    protocoloModel = await response.Content.ReadFromJsonAsync<ProtocoloModel>();                  
               

                return PartialView("_Proprietario_Condutor", protocoloModel);
           

        }

        [HttpGet]
        public async Task<PartialViewResult> DadosBancario(string protocolo)
        {
        
                ProtocoloModel protocoloModel = new ProtocoloModel();

                string apiUrl = $"{_baseApiUrl}atendimento/getprotocolo?protocolo={protocolo}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                        return PartialView("_ErrorPartialView");

                }
                if(response.StatusCode == HttpStatusCode.OK)    
                {
                    protocoloModel = await response.Content.ReadFromJsonAsync<ProtocoloModel>();
                    
                }

                return PartialView("_DadosBancario", protocoloModel);
           
        }
    }
}
