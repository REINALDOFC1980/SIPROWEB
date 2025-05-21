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

        //[AutorizacaoTokenAttribute("ADM")]
        public IActionResult Pessoa()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPessoa(string cpf)
        {
            PessoaModel? pessoaModel = null;

            string apiUrl = $"{_baseApiUrl}pessoa/getpessoa/{cpf}";
            var response = await _httpClient.GetAsync(apiUrl);

            // Verifica erros tratados
            var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;

            if (response.StatusCode == HttpStatusCode.OK)
                pessoaModel = await response.Content.ReadFromJsonAsync<PessoaModel>();


            //Caso não ache buscar no Detran
            else if (response.StatusCode == HttpStatusCode.NoContent)
            {
                apiUrl = $"{_baseApiUrl}pessoa/getpessoadetram/{cpf}";
                response = await _httpClient.GetAsync(apiUrl);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    pessoaModel = new PessoaModel();

                    var pessoaDetranModel = await _httpClient.GetFromJsonAsync<PessoaModel>(apiUrl);
                    pessoaModel.pes_Nome = pessoaDetranModel.pes_Nome;
                    pessoaModel.pes_NumRegistroCNH = pessoaDetranModel.pes_NumRegistroCNH;
                    pessoaModel.pes_DT_Validade = pessoaDetranModel.pes_DT_Validade;
                    pessoaModel.pes_UFCNH = pessoaDetranModel.pes_UFCNH;
                }
                else if (response.StatusCode == HttpStatusCode.NoContent)
                    return Json(new { message = "Nenhuma pessoa encontrada.", pessoaModel });
               
            }           

            return Json(new { pessoaModel });
        }

        [HttpPost]
        public async Task<IActionResult> CadastrarPessoa(PessoaModel pessoa)
        {
            
            if (pessoa.pes_ID == 0)
            {
                var apiUrl = $"{_baseApiUrl}pessoa/addpessoa";
                var response = await _httpClient.PostAsJsonAsync(apiUrl, pessoa);

                // Verifica erros tratados
                var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
                if (resultadoErro != null)
                    return resultadoErro;

            }
            else
            {
                var apiUrl = $"{_baseApiUrl}pessoa/alterpessoa";
                var response = await _httpClient.PostAsJsonAsync(apiUrl, pessoa);

                // Verifica erros tratados
                var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
                if (resultadoErro != null)
                    return resultadoErro;

            }

            return Json(new { erro = false, retorno = "Operação realizada com sucesso!" });
            

        }

    }
}
