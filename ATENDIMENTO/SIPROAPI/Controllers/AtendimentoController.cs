using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SIPROSHARED.DbContext;
using SIPROSHARED.Models;
using SIPROSHARED.Service.IRepository;
using SIRPOEXCEPTIONS.ExceptionBase;
using System.Net;
using System.Text.RegularExpressions;



namespace SIPROAPI.Controllers
{
    [Route("sipro/atendimento")]
    [ApiController]
    public class AtendimentoController : ControllerBase
    {
        private readonly ILogger<AtendimentoController> _logger;
        private readonly IAtendimentoService _atendimentoService;
        private readonly IPessoaService _pessoaService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DapperContext _context;
        private readonly string? _baseApiUrl;

        public AtendimentoController(IAtendimentoService atendimentoService,
                                     IPessoaService pessoaService,
                                     IHttpClientFactory httpClientFactory, 
                                     DapperContext context, 
                                     IConfiguration configuration,
                                     ILogger<AtendimentoController> logger)
        {
            _atendimentoService = atendimentoService;
            _pessoaService = pessoaService;
            _httpClientFactory = httpClientFactory;
            _context = context;
            _baseApiUrl = configuration.GetValue<string>("SipApiUrl");
            _logger = logger;


        }



        [HttpGet]
        [Route("getagendamento/{buscas}")]
        public async Task<IActionResult> GetAgendamento(string buscas)
        {           
            var agendamento = await _atendimentoService.GetAgendamento(buscas);

            if (agendamento == null)
            {
                return NoContent();
            }

            return Ok(agendamento);
           
        }
          
        [HttpGet]
        [Route("getanexo")]
        public async Task<IActionResult> GetAnexos()
        {
           
            var anexos = await _atendimentoService.BuscarAnexosTemp(@"C:\TempAnexoProtocolo");

            if (anexos == null)
            {
                return NoContent();
            }
            return Ok(anexos);
                       
        }

        [HttpGet]
        [Route("getprotocolo")]
        public async Task<IActionResult> GetProtocolo([FromQuery(Name = "protocolo")] string protocolo)
        {          

            var resultado = await _atendimentoService.BuscarProtocolo(Uri.UnescapeDataString(protocolo));

            if (resultado == null)
            {
                return NoContent();
            }

            return Ok(resultado);
           
            
        }

        [HttpGet]
        [Route("getprotocoloall")]
        public async Task<IActionResult> GetProtocoloAll([FromQuery(Name = "vloBusca")] string vloBusca)
        {            
            var resultado = await _atendimentoService.BuscarProtocoloAll(Uri.UnescapeDataString(vloBusca));

            if (resultado.Count == 0)
            {
                return NoContent();
            }

            return Ok(resultado);
            
        }

        [HttpGet]
        [Route("getmovimentacao")]
        public async Task<IActionResult> Getmovimentacao([FromQuery(Name = "protocolo")] string vloBusca)
        {
            try
            {
                var resultado = await _atendimentoService.BuscarMovimentacao(Uri.UnescapeDataString(vloBusca));

                if (resultado == null)
                {
                      return NoContent();
                }
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                // Registrar a exceção (supondo que um logger esteja configurado)
                //_logger.LogError(ex, "Ocorreu um erro ao buscar os dados do solicitante.");

                // Retornar uma mensagem de erro genérica
                return StatusCode(500, "Ocorreu um erro interno no servidor.");
            }
        }

        [HttpPost]
        [Route("addprotocolo")]
        public async Task<IActionResult> GerarProtocolo(ProtocoloModel protocoloModel)
        {
            using (var connection = _context.CreateConnection())
            {
                // Abrir a conexão
                connection.Open();

                // Iniciar a transação
                using (var transaction = connection.BeginTransaction())
                {
              
                    // Chamar o serviço para realizar a abertura do protocolo
                    string PRT_NUMERO = await _atendimentoService.RealizarAbertura(protocoloModel, connection, transaction);
                     
                    // Atribuir o número do protocolo ao modelo
                    protocoloModel.PRT_NUMERO = PRT_NUMERO;


                    ProtocoloDocImgModel protocoloDocImg = new ProtocoloDocImgModel();
                    protocoloDocImg.PRTDOC_PRT_NUMERO = protocoloModel.PRT_NUMERO;
                    protocoloDocImg.PRTDOC_PRT_AIT = protocoloModel.PRT_AIT;
                    protocoloDocImg.PRTDOC_PRT_SETOR = protocoloModel.PRT_ORIGEM;                   

                    //inserindo imagem no banco
                    string folderPath = @"C:\TempAnexoProtocolo";                  
                    await _atendimentoService.IntoAnexoFolder(folderPath, protocoloDocImg, connection, transaction);


                    //Integração API SIP
                    var response = await EnviarRequisicaoAsync(protocoloModel);

                    if (!response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        transaction.Rollback();
                        await _atendimentoService.UpdateAgendamento(protocoloModel.PRT_AIT, protocoloModel.PRT_ASSUNTO, "Validação API");
                        var status = response.StatusCode;

                        if (status == HttpStatusCode.BadRequest) // 400
                        {
                            var errorResponses = JsonConvert.DeserializeObject<List<ErrorResponse>>(content);
                            var errorMessage = errorResponses?.FirstOrDefault()?.Message;
                            throw new ErrorOnValidationException(new List<string> { errorMessage });
                         
                        }
                        else
                        {
                            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(content);
                            var errorMessage = errorResponse?.Message;
                            throw new ErrorOnValidationException(new List<string> { errorMessage });
                        }

                    } 
                    else
                    {
                        // Confirmar a transação se tudo ocorrer bem
                        transaction.Commit();
                        return Ok(new { protocolo = protocoloModel.PRT_NUMERO });
                    }
                }
            }
        }
                    
        [HttpPost]
        [Route("aberturapresencial")]
        public async Task<IActionResult> AddAberturaPresencial(AgendaModel agendaModel)
        {
            await _atendimentoService.CadastrarAberturaAsync(agendaModel);
            return Ok(new { message = "Abertura cadastrada com sucesso." });            
        }

        [HttpPost]
        [Route("alteragendamento")]
        public async Task<IActionResult> AlterAgendamento([FromBody] dynamic parametros)
        {
            try
            {
                string ait = parametros.GetProperty("ait").GetString();
                int codservico = parametros.GetProperty("codServico").GetInt32();
                string situacao = parametros.GetProperty("situacao").GetString();

                await _atendimentoService.UpdateAgendamento(ait, codservico, situacao);


                return Ok();
            }
            catch (Exception ex)
            {
                // Registrar a exceção (supondo que um logger esteja configurado)
                //_logger.LogError(ex, "Ocorreu um erro ao buscar os dados do solicitante.");
                return StatusCode(500, "Ocorreu um erro interno no servidor.");
            }
        }

        [HttpGet]
        [Route("getdocanexo/{cod_servico}")]
        public async Task<IActionResult> GetDocumentosAnexo(int cod_servico)
        {
            try
            {
                var documento = await _atendimentoService.BuscarDocumentos(cod_servico);

                if (documento == null)
                {
                    return NoContent();
                }

                return Ok(documento);
            }
            catch (Exception ex)
            {
                // Registrar a exceção (supondo que um logger esteja configurado)
                _logger.LogError(ex, "Ocorreu um erro ao buscar os dados do solicitante.");

                return StatusCode(500, "Ocorreu um erro interno no servidor.");
            }
        }

        [HttpGet]
        [Route("getduplicidade/{ait}/{codservico}")]
        public async Task<IActionResult> GetDuplicidadeProcesso(string ait, int codservico)
        {
            try
            {
                // Buscar dados do solicitante pra saber se tem cadastro!
                var duplicidade = await _atendimentoService.DuplicidadeServico(ait, codservico);

                return Ok(duplicidade);
            }
            catch (Exception ex)
            {
                // Registrar a exceção (supondo que um logger esteja configurado)
                //_logger.L ogError(ex, "Ocorreu um erro ao buscar os dados do solicitante.");

                // Retornar uma mensagem de erro genérica
                return StatusCode(500, "Ocorreu um erro interno no servidor.");
            }
        }


        private async Task<HttpResponseMessage> EnviarRequisicaoAsync(ProtocoloModel protocoloModel)
        {
            //pegar a data de abertura deacordo com a origem!!!!

            var httpClient = _httpClientFactory.CreateClient();

            var condutor = await _pessoaService.GetDadosPessoa(protocoloModel.PRT_CPF_CONDUTOR);


            if (protocoloModel.PRT_ASSUNTO == 1)//Condutor
            {
                if (!string.IsNullOrEmpty(protocoloModel.PRT_CNH_ESTRANGEIRA))
                {
                    var payload = new
                    {
                        recAitNumero = protocoloModel.PRT_AIT,
                        recTrocainfNomecond = protocoloModel.PRT_CNH_ESTRANGEIRA_NOME,
                        recTrocainfRegistro = protocoloModel.PRT_CNH_ESTRANGEIRA,
                        recTrocainfUfcnh = "BA",
                        recModelocnh = 3,
                        recTrocainfEndereco = "Av. Vale dos Barris",
                        recTrocainfNumero = "501",
                        recTrocainfComplemento = "TRANSALVADOR",
                        recTrocainfBairro = "TRANSALVADOR",
                        recTrocainfMunicipio = "3849",
                        recTrocainfCep = "40070055",
                        recTrocainfDataapre = DateTime.Now.ToString("yyyy-MM-dd"),
                        recTrocainfUsuario = protocoloModel.PRT_ATENDENTE,
                        numeroprocesso = protocoloModel.PRT_NUMERO,
                    };
                    var apiUrl = $"{_baseApiUrl}condutor/v1";
                    return await httpClient.PostAsJsonAsync(apiUrl, payload);
                }
                else
                {
                    var payload = new
                    {
                        recAitNumero = protocoloModel.PRT_AIT,
                        recTrocainfNomecond = condutor.pes_Nome,
                        recTrocainfRegistro = condutor.pes_NumRegistroCNH,
                        recTrocainfUfcnh = condutor.pes_UFCNH.ToUpper(),
                        recModelocnh = condutor.pes_Tipo == "Física" ? "2" : "3",
                        recTrocainfEndereco = condutor.pes_EndLogradouro,
                        recTrocainfNumero = condutor.pes_EndNumero,
                        recTrocainfComplemento = condutor.pes_EndComplemento,
                        recTrocainfBairro = condutor.pes_EndBairro,
                        recTrocainfMunicipio = "",
                        recTrocainfCep = Regex.Replace(condutor.pes_EndCEP, "[^0-9]", ""),
                        recTrocainfDataapre = DateTime.Now.ToString("yyyy-MM-dd"),
                        recTrocainfUsuario = protocoloModel.PRT_ATENDENTE,
                        numeroprocesso = protocoloModel.PRT_NUMERO,
                    };
                    var apiUrl = $"{_baseApiUrl}condutor/v1";
                    return await httpClient.PostAsJsonAsync(apiUrl, payload);
                }
            }
            else if (protocoloModel.PRT_ASSUNTO == 2)//Defesa
            {
                var payload = new
                {
                    recAitNumero = protocoloModel.PRT_AIT,
                    recDpDataabertura = DateTime.Now.ToString("yyyy-MM-dd"),
                    recDpUsuariocadastro = protocoloModel.PRT_ATENDENTE,
                    recDpProcesso = protocoloModel.PRT_NUMERO,
                };
                var apiUrl = $"{_baseApiUrl}defesa/v1";
                return await httpClient.PostAsJsonAsync(apiUrl, payload);
            }
            //else if (protocoloModel.PRT_ASSUNTO == 3)//ressarcimento
            //{
            //    var payload = new
            //    {
            //        recAitNumero = protocoloModel.PRT_AIT,
            //        recRsProcesso = protocoloModel.PRT_NUMERO,                
            //        recRsDataabertura = DateTime.Now.ToString("yyyy-MM-dd"),              
            //        recRsUsuariocadastroabertura = protocoloModel.PRT_ATENDENTE,
            //    };
            //    var apiUrl = $"{_baseApiUrl}ressarcimento/v1";
            //    return await httpClient.PostAsJsonAsync(apiUrl, payload);
            //}
            else if (protocoloModel.PRT_ASSUNTO == 8)//Jari
            {
                var payload = new
                {
                    recAitNumero = protocoloModel.PRT_AIT,
                    recJariProcesso = protocoloModel.PRT_NUMERO,
                    recJariDataabertura = DateTime.Now.ToString("yyyy-MM-dd"),
                    recJariUsuariocadastro = protocoloModel.PRT_ATENDENTE,
                    aplicarEfeitoSuspensivo = true,//Veri se miro ja faz
                };
                var apiUrl = $"{_baseApiUrl}jari/v1";
                return await httpClient.PostAsJsonAsync(apiUrl, payload);
            }
            else if (protocoloModel.PRT_ASSUNTO == 9)//Cetran
            {
                var payload = new
                {
                    recAitNumero = protocoloModel.PRT_AIT,
                    recCetranProcesso = protocoloModel.PRT_NUMERO,
                    recCetranDataabertura = DateTime.Now.ToString("yyyy-MM-dd"),
                    recCetranUsuariocadastro = protocoloModel.PRT_ATENDENTE,
                    aplicarEfeitoSuspensivo = true,//Veri se miro ja faz
                };
                var apiUrl = $"{_baseApiUrl}cetran/v1";
                return await httpClient.PostAsJsonAsync(apiUrl, payload);
            }
            else if (protocoloModel.PRT_ASSUNTO == 39) //Condutor_Defesa
            {
                var payload = new
                {
                    recAitNumero = protocoloModel.PRT_AIT,
                    recTrocainfNomecond = condutor.pes_Nome,
                    recTrocainfRegistro = condutor.pes_NumRegistroCNH,
                    recTrocainfUfcnh = condutor.pes_UFCNH,
                    recModelocnh = condutor.pes_Tipo == "Física" ? "2" : "3",
                    recTrocainfEndereco = condutor.pes_EndLogradouro,
                    recTrocainfNumero = condutor.pes_EndNumero,
                    recTrocainfComplemento = condutor.pes_EndComplemento,
                    recTrocainfBairro = condutor.pes_EndBairro,
                    recTrocainfMunicipio = "",
                    recTrocainfCep = Regex.Replace(condutor.pes_EndCEP, "[^0-9]", ""),
                    recTrocainfDataapre = DateTime.Now.ToString("yyyy-MM-dd"),
                    recTrocainfUsuario = protocoloModel.PRT_ATENDENTE,
                    numeroprocesso = protocoloModel.PRT_NUMERO,
                };
                var apiUrl = $"{_baseApiUrl}condutoredefesa/v1";
                return await httpClient.PostAsJsonAsync(apiUrl, payload);
            }
            else
                return null;



        }
    }
}
