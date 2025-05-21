using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SIPROSHARED.Filtro;
using SIPROSHARED.Models;
using SIPROSHAREDDISTRIBUICAO.Models;
using System.Net;
using System.Web.Services.Description;


namespace SIPROWEBDISTRIBUICAO.Controllers
{
    [AutorizacaoTokenAttribute("ADM", "DISTRIBUICAO_1")]
    public class DistribuicaoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string? _baseApiUrl;
        private string? userMatrix;

        public DistribuicaoController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseApiUrl = configuration.GetValue<string>("DistribuicaoApiUrl");
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            userMatrix = HttpContext.Items["UserMatrix"] as string;
            base.OnActionExecuting(context);
        }

        public async Task<IActionResult> Distribuicao()
        {
            var ProcessorDictionary = new Dictionary<string, List<ListaProcessoUsuario>>();
            ViewBag.Assunto = new List<AssuntoQtd>();

            //Buscando processo por assunto
            var apiUrl = $"{_baseApiUrl}distribuicao/Getprocessosassunto/{userMatrix}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                    return RedirectToAction("InternalServerError", "Home");

                return RedirectToAction("BadRequest", "Home");
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadFromJsonAsync<List<AssuntoQtd>>();
                ViewBag.Assunto = result ?? new List<AssuntoQtd>();
            }

            //buscando processo ja distribuidos
            ViewBag.Usuario = new List<ProtocoloDistribuicaoModel>();
            ViewBag.ListaProcessos = new List<ListaProcessoUsuario>();
            ViewBag.ListaProcessosSetor = new List<ProtocoloDistribuicaoModel>();
            ViewBag.Usuario = new List<ProtocoloDistribuicaoModel>();

            apiUrl = $"{_baseApiUrl}distribuicao/GetProcessosDistribuidoUsuario/{userMatrix}";
            response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                    return RedirectToAction("Error", "Home");
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                List<ProtocoloDistribuicaoModel> result = await response.Content.ReadFromJsonAsync<List<ProtocoloDistribuicaoModel>>();
                ViewBag.Usuario = result;

                foreach (var item in result)
                {
                    var itens = await BuscarProcessoUsuario(item.DIS_DESTINO_USUARIO);
                    ProcessorDictionary[item.DIS_DESTINO_USUARIO] = itens;
                }

                ViewBag.ListaProcessos = ProcessorDictionary;
            }
            //pegando os processo por setor
            ViewBag.ListaProcessosSetor = await BuscarProcessoSetor(userMatrix);

            return View();
        }

        public async Task<List<ListaProcessoUsuario>> BuscarProcessoSetor(string usuario)
        {
            var apiUrl = $"{_baseApiUrl}distribuicao/GetProcessoSetor/{usuario}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<List<ListaProcessoUsuario>>();

            return new List<ListaProcessoUsuario>();

        }

        public async Task<IActionResult> BuscarProcesso(int movpro_id)
        {
            var apiUrl = $"{_baseApiUrl}distribuicao/GetProcesso/{movpro_id}";
            var response = await _httpClient.GetAsync(apiUrl);

            // Verifica erros tratados
            var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;


            if (response.StatusCode == HttpStatusCode.OK)
            {
                var processo = await response.Content.ReadFromJsonAsync<ListaProcessoUsuario>();
                return Json(new { error = false, processo });
            }

            return Json(new { error = true, message = "Processo não encontrado" });
        }

        public async Task<List<ProtocoloDistribuicaoModel>> BuscarUsuarioSetor(string usuario)
        {
            var apiUrl = $"{_baseApiUrl}distribuicao/getUsuarioSetor/{usuario}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<List<ProtocoloDistribuicaoModel>>();


            return new List<ProtocoloDistribuicaoModel>();
        }

        public async Task<List<ListaProcessoUsuario>> BuscarProcessoUsuario(string usuario)
        {
            var apiUrl = $"{_baseApiUrl}distribuicao/GetProcessosUsuario/{usuario}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<List<ListaProcessoUsuario>>();

            return new List<ListaProcessoUsuario>();
        }

        [HttpGet]
        public async Task<IActionResult> ProcessoPorAssuntoView(string userMatrix)
        {

            var apiUrl = $"{_baseApiUrl}distribuicao/GetProcessosDistribuidoUsuario/{userMatrix}";
            var response = await _httpClient.GetAsync(apiUrl);

            // Verifica erros tratados
            var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                List<ProtocoloDistribuicaoModel> result = await response.Content.ReadFromJsonAsync<List<ProtocoloDistribuicaoModel>>();
                ViewBag.Usuario = result;
            }

            return PartialView("_ProcessoDistribuidos");


        }



        [HttpPost]
        public async Task<IActionResult> addDistribuicaoProcessoEspecifico([FromBody] ProtocoloDistribuicaoModel distribuicaoModel)
        {


            distribuicaoModel.DIS_ORIGEM_USUARIO = userMatrix;

            //Add processo ao usuario
            var apiUrl = $"{_baseApiUrl}distribuicao/adddistribuicaoprocessoespecifico";
            var response = await _httpClient.PostAsJsonAsync(apiUrl, distribuicaoModel);

            var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;

            await CarregarDistribuicao(userMatrix);


            return PartialView("_Assunto");

        }

        [HttpPost]
        public async Task<IActionResult> RetirarProcessoEspecifico([FromBody] int DIS_ID)
        {
            var distribuicaoModel = new ProtocoloDistribuicaoModel
            {
                DIS_ID = DIS_ID,
                DIS_ORIGEM_USUARIO = userMatrix
            };

            var apiUrl = $"{_baseApiUrl}distribuicao/retirarprocessoespecifico";
            var response = await _httpClient.PostAsJsonAsync(apiUrl, distribuicaoModel);

            var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;

            await CarregarDistribuicao(userMatrix);

            return PartialView("_Assunto");
        }

        [HttpPost]
        public async Task<IActionResult> addDistribuicaoProcesso([FromBody] List<ProtocoloDistribuicaoModel> distribuicoes)
        {
            foreach (var distribuicaoModel in distribuicoes)
            {
                distribuicaoModel.DIS_ORIGEM_USUARIO = userMatrix;

                var apiUrl = $"{_baseApiUrl}distribuicao/adddistribuicaoprocesso";
                var response = await _httpClient.PostAsJsonAsync(apiUrl, distribuicaoModel);

                var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
                if (resultadoErro != null)
                    return resultadoErro;
            }
            await CarregarDistribuicao(userMatrix);

            return PartialView("_Assunto");
        }

        [HttpPost]
        public async Task<IActionResult> RetirarProcesso([FromBody] List<ProtocoloDistribuicaoModel> distribuicoes)
        {
            foreach (var distribuicaoModel in distribuicoes)
            {
                distribuicaoModel.DIS_ORIGEM_USUARIO = userMatrix;

                var apiUrl = $"{_baseApiUrl}distribuicao/retirarprocesso";
                var response = await _httpClient.PostAsJsonAsync(apiUrl, distribuicaoModel);


                var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
                if (resultadoErro != null)
                    return resultadoErro;
            }

            _ = await CarregarDistribuicao(userMatrix);

            return PartialView("_Assunto");

        }


        private async Task<IActionResult?> CarregarDistribuicao(string userMatrix)
        {
            var ProcessorDictionary = new Dictionary<string, List<ListaProcessoUsuario>>();

            // atualizando tela de usuário
            var apiUrl = $"{_baseApiUrl}distribuicao/Getprocessosassunto/{userMatrix}";
            var response = await _httpClient.GetAsync(apiUrl);

            var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadFromJsonAsync<List<AssuntoQtd>>();
                ViewBag.Assunto = result;
                await ProcessoPorAssuntoView(userMatrix);
            }

            /*------------------------------------------------------*/

            //Buscando os processo do usuario
            apiUrl = $"{_baseApiUrl}distribuicao/GetProcessosDistribuidoUsuario/{userMatrix}";
            response = await _httpClient.GetAsync(apiUrl);

            resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                List<ProtocoloDistribuicaoModel> result = await response.Content.ReadFromJsonAsync<List<ProtocoloDistribuicaoModel>>();
                ViewBag.Usuario = result;


                foreach (var item in result)
                {
                    var itens = await BuscarProcessoUsuario(item.DIS_DESTINO_USUARIO);
                    ProcessorDictionary[item.DIS_DESTINO_USUARIO] = itens;
                }

                ViewBag.ListaProcessos = ProcessorDictionary;
                ViewBag.ListaProcessosSetor = await BuscarProcessoSetor(userMatrix);
            }
            return null; // nenhum erro, segue o fluxo normal
        }



    }
}



