using Castle.Core.Internal;
using Microsoft.AspNetCore.Mvc;
using SIPROSHARED.Models;
using SIPROSHARED.Filtro;
using System.Net.Http;
using System.Text.Json;

namespace SIPROWEB.Controllers
{
    [AutorizacaoTokenAttribute( "ADM","ATENDIMENTO_1")]
    public class AITController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseSipApiUrl;


        public AITController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
          
            _baseSipApiUrl = configuration.GetValue<string>("BaseSipApiUrl");

        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult LocalizarAIT()
        {
            return View();
        }

        [HttpGet]
        public async Task<PartialViewResult> BuscarAIT(string vloBusca)
        {
            string apiUrl = $"{_baseSipApiUrl}ait/v1/{vloBusca}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var jsonString = await response.Content.ReadAsStringAsync();

                try
                {       
                    var aitData = JsonSerializer.Deserialize<ResultGetAitModel>(jsonString);

                    // Convert the single object to a list
                    var aitDataList = new List<ResultGetAitModel> { aitData };

                    return PartialView("_ListaAIT", aitDataList);
                }
                catch (JsonException ex)
                {
                    ModelState.AddModelError(string.Empty, "Erro ao processar os dados recebidos.");
                    return PartialView("_ListaAIT", new List<ResultGetAitModel>());
                }
            }

            ModelState.AddModelError(string.Empty, "Erro ao buscar dados.");
            return PartialView("_ListaAIT", new List<ResultGetAitModel>());
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
                }else
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
        public async Task<PartialViewResult> DetalheCondutor(string ait)
        {
            try
            {
                ProtocoloModel protocoloModel = new ProtocoloModel();

               
               
                var apiUrl = $"{_baseSipApiUrl}condutor/v1/{ait}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var apiAitModel = await response.Content.ReadFromJsonAsync<Proc_Condutor_Model>();

                    protocoloModel.PRT_NUMERO = apiAitModel.numeroprocesso;
                    protocoloModel.PRT_AIT = apiAitModel.Rec_AIT_Numero;

                    DateTime defesaNaiDate = DateTime.Parse(apiAitModel.rec_TrocaInf_Dataapre);
                    protocoloModel.PRT_DT_ABERTURA = defesaNaiDate.ToString("dd/MM/yyyy");
                    protocoloModel.PRT_CPF_CONDUTOR = apiAitModel.rec_TrocaInf_Numero;
                    protocoloModel.PRT_ATENDENTE = apiAitModel.rec_TrocaInf_Usuario;
                    protocoloModel.PRT_NOME_CONDUTOR = apiAitModel.rec_Trocainf_Nomecond;
                    protocoloModel.PRT_NUMREGISTRO_CNH = apiAitModel.rec_Trocainf_Registro;
                    protocoloModel.PRT_RETORNO_DETRAN = apiAitModel.rec_Retornodetran;
                    protocoloModel.PRT_ENDERECO_CONDUTOR = apiAitModel.rec_TrocaInf_Endereco + ", " +
                                                           apiAitModel.rec_Trocainf_Bairro + ", " +
                                                           apiAitModel.rec_TrocaInf_Municipio + ", " +
                                                           apiAitModel.rec_TrocaInf_UF;

                    return PartialView("_Condutor", protocoloModel);
                }
                else
                {
                    //Pegando o condutore apresentado no ato da infração:
                     apiUrl = $"{_baseSipApiUrl}ait/v1/{ait}";
                     response = await _httpClient.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var apiAitModel = await response.Content.ReadFromJsonAsync<ResultGetAitModel>();
                      
                        if (apiAitModel?.condutorInfratorNome == null)
                            return PartialView("_Condutor", null);
                        else
                        {
                            protocoloModel.PRT_CONDUTOR_APRESENTADO = apiAitModel.condutorInfratorNome;
                            return PartialView("_Condutor", protocoloModel);
                        }                
                    }
                }                
                
                return PartialView("_Condutor", null);
                

            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpGet]
        public async Task<PartialViewResult> DetalheDefesa(string ait)
        {
            try
            {
                ProtocoloModel protocoloModel = new ProtocoloModel();

                string apiUrl = $"{_baseSipApiUrl}defesa/v1/{ait}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var apiAitModel = await response.Content.ReadFromJsonAsync<Proc_Defesa_Model>();

                    protocoloModel.PRT_NUMERO = apiAitModel.Rec_DP_Processo;
                    protocoloModel.PRT_AIT = apiAitModel.Rec_AIT_Numero;
                    DateTime defesaNaiDate = DateTime.Parse(apiAitModel.Rec_DP_DataAbertura);
                    protocoloModel.PRT_DT_ABERTURA = defesaNaiDate.ToString("dd/MM/yyyy");
                    protocoloModel.PRT_ATENDENTE = apiAitModel.Rec_DP_UsuarioCadastro;
                    
                    
                    //protocoloModel.PRT_RESULTADO = apiAitModel.Rec_DP_Resultado;
                    switch (apiAitModel.Rec_DP_Resultado)
                    {
                        case "X":
                            protocoloModel.PRT_RESULTADO = "Aguardando Resultado";
                            break;
                        case "I":
                            protocoloModel.PRT_RESULTADO = "Indeferido";
                            break;
                        case "D":
                            protocoloModel.PRT_RESULTADO = "Deferido";
                            break;
                        default:
                            protocoloModel.PRT_RESULTADO = "Resultado Desconhecido"; // Opcional, para tratar valores inesperados
                            break;
                    }

                
                   protocoloModel.PRT_DT_JULGAMENTO = DateTime.TryParse(apiAitModel?.Rec_DP_DataResultado, out var dtjulgamento)
                   ? dtjulgamento.ToString("dd/MM/yyyy")
                   : string.Empty;
                   

                     protocoloModel.PRT_DT_PUBLICACAO = DateTime.TryParse(apiAitModel?.Rec_DP_Publicacao_Cancel, out var dtpublicacao)
                   ? dtpublicacao.ToString("dd/MM/yyyy")
                   : string.Empty;

                    return PartialView("_Defesa", protocoloModel);
                }
                else
                {
                    return PartialView("_Defesa", null);
                }

            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpGet]
        public async Task<PartialViewResult> DetalheJARI(string ait)
        {
            try
            {
                ProtocoloModel protocoloModel = new ProtocoloModel();

                string apiUrl = $"{_baseSipApiUrl}jari/v1/{ait}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var apiAitModel = await response.Content.ReadFromJsonAsync<Proc_Jari_Model>();

                    protocoloModel.PRT_NUMERO = apiAitModel.Rec_JARI_Processo;
                    protocoloModel.PRT_AIT = apiAitModel.Rec_AIT_Numero;
                    DateTime defesaNaiDate = DateTime.Parse(apiAitModel.Rec_JARI_DataAbertura);
                    protocoloModel.PRT_DT_ABERTURA = defesaNaiDate.ToString("dd/MM/yyyy");
                    protocoloModel.PRT_ATENDENTE = apiAitModel.Rec_JARI_UsuarioCadastro;


                    //protocoloModel.PRT_RESULTADO = apiAitModel.Rec_DP_Resultado;
                    switch (apiAitModel.Rec_JARI_Resultado)
                    {
                        case "X":
                            protocoloModel.PRT_RESULTADO = "Aguardando Resultado";
                            break;
                        case "I":
                            protocoloModel.PRT_RESULTADO = "Indeferido";
                            break;
                        case "D":
                            protocoloModel.PRT_RESULTADO = "Deferido";
                            break;
                        default:
                            protocoloModel.PRT_RESULTADO = "Resultado Desconhecido"; // Opcional, para tratar valores inesperados
                            break;
                    }


                    protocoloModel.PRT_DT_JULGAMENTO = DateTime.TryParse(apiAitModel?.Rec_JARI_DataResultado, out var dtjulgamento)
                    ? dtjulgamento.ToString("dd/MM/yyyy")
                    : string.Empty;


                    protocoloModel.PRT_DT_PUBLICACAO = DateTime.TryParse(apiAitModel?.Rec_JARI_Publicacao_Cancel, out var dtpublicacao)
                    ? dtpublicacao.ToString("dd/MM/yyyy")
                    : string.Empty;



                    return PartialView("_JARI", protocoloModel);
                }
                else
                {
                    return PartialView("_JARI", null);
                }

            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpGet]
        public async Task<PartialViewResult> DetalheCETRAN(string ait)
        {
            try
            {
                ProtocoloModel protocoloModel = new ProtocoloModel();

                string apiUrl = $"{_baseSipApiUrl}cetran/v1/{ait}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var apiAitModel = await response.Content.ReadFromJsonAsync<Proc_CETRAN_Model>();

                    protocoloModel.PRT_NUMERO = apiAitModel.Rec_CETRAN_Processo;
                    protocoloModel.PRT_AIT = apiAitModel.Rec_AIT_Numero;
                    DateTime defesaNaiDate = DateTime.Parse(apiAitModel.Rec_CETRAN_DataAbertura);
                    protocoloModel.PRT_DT_ABERTURA = defesaNaiDate.ToString("dd/MM/yyyy");
                    protocoloModel.PRT_ATENDENTE = apiAitModel.Rec_CETRAN_UsuarioCadastro;


                    //protocoloModel.PRT_RESULTADO = apiAitModel.Rec_DP_Resultado;
                    switch (apiAitModel.Rec_CETRAN_Resultado)
                    {
                        case "X":
                            protocoloModel.PRT_RESULTADO = "Aguardando Resultado";
                            break;
                        case "I":
                            protocoloModel.PRT_RESULTADO = "Indeferido";
                            break;
                        case "D":
                            protocoloModel.PRT_RESULTADO = "Deferido";
                            break;
                        default:
                            protocoloModel.PRT_RESULTADO = "Resultado Desconhecido"; // Opcional, para tratar valores inesperados
                            break;
                    }

                    protocoloModel.PRT_DT_JULGAMENTO = DateTime.TryParse(apiAitModel?.Rec_CETRAN_DataResultado, out var dtjulgamento)
                    ? dtjulgamento.ToString("dd/MM/yyyy")
                    : string.Empty;


                    protocoloModel.PRT_DT_PUBLICACAO = DateTime.TryParse(apiAitModel?.Rec_CETRAN_Publicacao_Cancel, out var dtpublicacao)
                    ? dtpublicacao.ToString("dd/MM/yyyy")
                    : string.Empty;

                    return PartialView("_CETRAN", protocoloModel);
                }
                else
                {
                    return PartialView("_CETRAN", null);
                }

            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpGet]
        public async Task<PartialViewResult> DetalheRessarcimento(string ait)
        {
            try
            {
                ProtocoloModel protocoloModel = new ProtocoloModel();

                string apiUrl = $"{_baseSipApiUrl}ressarcimento/v1/{ait}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var apiAitModel = await response.Content.ReadFromJsonAsync<Proc_Ressarcimento_Model>();

                    protocoloModel.PRT_NUMERO = apiAitModel.Rec_RS_Processo;
                    protocoloModel.PRT_AIT = apiAitModel.Rec_AIT_Numero;
                    DateTime defesaNaiDate = DateTime.Parse(apiAitModel.Rec_RS_DataAbertura);
                    protocoloModel.PRT_DT_ABERTURA = defesaNaiDate.ToString("dd/MM/yyyy");
                    protocoloModel.PRT_ATENDENTE = apiAitModel.Rec_RS_UsuarioCadastroAbertura;


                    //protocoloModel.PRT_RESULTADO = apiAitModel.Rec_DP_Resultado;
                    switch (apiAitModel.Rec_RS_Resultado)
                    {
                        case "X":
                            protocoloModel.PRT_RESULTADO = "Aguardando Resultado";
                            break;
                        case "I":
                            protocoloModel.PRT_RESULTADO = "Indeferido";
                            break;
                        case "D":
                            protocoloModel.PRT_RESULTADO = "Deferido";
                            break;
                        default:
                            protocoloModel.PRT_RESULTADO = "Resultado Desconhecido"; // Opcional, para tratar valores inesperados
                            break;
                    }

                    protocoloModel.PRT_DT_JULGAMENTO = DateTime.TryParse(apiAitModel?.Rec_RS_DataResultado, out var dtpublicacao)
                    ? dtpublicacao.ToString("dd/MM/yyyy")
                    : string.Empty;

                    protocoloModel.PRT_OBSERVACAO = apiAitModel.Rec_RS_Observacao;

                    return PartialView("_Ressarcimento", protocoloModel);
                }
                else
                {
                    return PartialView("_Ressarcimento", null);
                }

            }
            catch (Exception ex)
            {
                throw;
            }

        }

    }
}
