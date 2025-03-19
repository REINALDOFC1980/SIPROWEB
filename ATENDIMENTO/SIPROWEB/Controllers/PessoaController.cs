using Microsoft.AspNetCore.Mvc;
using SIPROSHARED.Models;
using SIPROSHARED.Filtro;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;

namespace SIPROWEB.Controllers
{
    [AutorizacaoTokenAttribute("ADM", "ATENDIMENTO_1")]
    public class PessoaController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string? _baseApiUrl;
    
      
        public PessoaController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseApiUrl = configuration.GetValue<string>("BaseApiUrl");        
        }


        private async Task<JsonResult> HandleErrorResponse(HttpResponseMessage response)
        {
            var errorResponse = await response.Content.ReadAsStringAsync();
            var errorData = JsonConvert.DeserializeObject<ErrorResponseModel>(errorResponse);
            var errorMessage = errorData?.Errors?.FirstOrDefault() ?? "Erro ao processar sua solicitação.";

            return Json(new { error = "BadRequest",  retorno = errorMessage });
        }


        //[AutorizacaoTokenAttribute("ADM")]
        public IActionResult Pessoa()
        {
            return View();
        }

        [HttpGet]
        public async Task<JsonResult> GetPessoa(string cpf)
        {
            string apiUrl = $"{_baseApiUrl}pessoa/getpessoa/{cpf}";
            var response = await _httpClient.GetAsync(apiUrl);

            PessoaModel? pessoaModel = null;

            var retornodetran = 0;

            if (response.StatusCode == HttpStatusCode.OK)
                pessoaModel = await response.Content.ReadFromJsonAsync<PessoaModel>();


            else if (response.StatusCode == HttpStatusCode.NoContent)
            {
                //Pegando dados do Detran Para atualizar os campos no formulário
                apiUrl = $"{_baseApiUrl}pessoa/getpessoadetram/{cpf}";
                response = await _httpClient.GetAsync(apiUrl);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    retornodetran = 1;
                    pessoaModel = new PessoaModel();

                    var pessoaDetranModel = await _httpClient.GetFromJsonAsync<PessoaModel>(apiUrl);
                    pessoaModel.pes_Nome = pessoaDetranModel.pes_Nome;
                    pessoaModel.pes_NumRegistroCNH = pessoaDetranModel.pes_NumRegistroCNH;
                    pessoaModel.pes_DT_Validade = pessoaDetranModel.pes_DT_Validade;
                    pessoaModel.pes_UFCNH = pessoaDetranModel.pes_UFCNH;
                }
                else if (response.StatusCode == HttpStatusCode.NoContent)
                    return Json(new { message = "Nenhuma pessoa encontrada.", pessoaModel });
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                    return Json(new { message = "Erro na busca.", pessoaModel });
            }           

            return Json(new { pessoaModel });
        }

        [HttpPost]
        public async Task<IActionResult> CadastrarPessoa(PessoaModel pessoa)
        {
            try
            {

                if (pessoa.pes_ID == 0)
                {
                    var apiUrl = $"{_baseApiUrl}pessoa/addpessoa";
                    var response = await _httpClient.PostAsJsonAsync(apiUrl, pessoa);

                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                        return Json(new { error = "InternalServerError"});

                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                        return await HandleErrorResponse(response);
                }
                else
                {
                    var apiUrl = $"{_baseApiUrl}pessoa/alterpessoa";
                    var response = await _httpClient.PostAsJsonAsync(apiUrl, pessoa);

                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                        return Json(new { error = "InternalServerError" });

                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                        return await HandleErrorResponse(response);


                }

                return Json(new { erro = false, retorno = "Operação realizada com sucesso!" });
            }
            catch (Exception)
            {

                throw;
            }

        }

    }
}
