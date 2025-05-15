using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SIPROSHARED.Filtro;
using SIPROSHARED.Models;
using SIPROSHAREDINSTRUCAO.Models;
using System.Net;
using System.Web.Services.Description;


namespace SIPROSHAREDINSTRUCAO.Controllers
{
    [AutorizacaoTokenAttribute("ADM", "INSTRUCAO_1")]
    public class InstrucaoDistribuicaoController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly string? _baseApiUrl;
        private string? userMatrix;

        public InstrucaoDistribuicaoController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseApiUrl = configuration.GetValue<string>("InstrucaoApiUrl");
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            userMatrix = HttpContext.Items["UserMatrix"] as string;
            base.OnActionExecuting(context);
        }

        public async Task<IActionResult> InstrucaoDistribuicao()
        {
            var ProcessorDictionary = new Dictionary<string, List<ListaProcessoUsuario>>();
            ViewBag.Assunto = new List<AssuntoQtd>();

            // Buscando processo por assunto
            var apiUrl = $"{_baseApiUrl}distribuicaoinstrucao/Getprocessosassunto/{userMatrix}";
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

            // Buscando processo já distribuídos
            ViewBag.Usuario = new List<ProtocoloDistribuicaoModel>();
            ViewBag.ListaProcessos = new List<ListaProcessoUsuario>();
            ViewBag.ListaProcessosSetor = new List<ProtocoloDistribuicaoModel>();

            apiUrl = $"{_baseApiUrl}distribuicaoinstrucao/GetProcessosDistribuidoUsuario/{userMatrix}";
            response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                    return RedirectToAction("InternalServerError", "Home");

                return RedirectToAction("BadRequest", "Home");
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadFromJsonAsync<List<ProtocoloDistribuicaoModel>>();
                ViewBag.Usuario = result ?? new List<ProtocoloDistribuicaoModel>();

                foreach (var item in ViewBag.Usuario)
                {
                    var itens = await BuscarProcessoUsuario(item.DIS_DESTINO_USUARIO);
                    ProcessorDictionary[item.DIS_DESTINO_USUARIO] = itens;
                }

                ViewBag.ListaProcessos = ProcessorDictionary;
            }

            // Pegando os processos por setor
            ViewBag.ListaProcessosSetor = await BuscarProcessoSetor(userMatrix);

            return View();
        }

        public async Task<List<ProtocoloDistribuicaoModel>> BuscarUsuarioSetor(string usuario)
        {
            var apiUrl = $"{_baseApiUrl}distribuicaoinstrucao/getUsuarioSetor/{usuario}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<List<ProtocoloDistribuicaoModel>>();           

            return new List<ProtocoloDistribuicaoModel>();
        }

        public async Task<List<ListaProcessoUsuario>> BuscarProcessoUsuario(string usuario)
        {
            var apiUrl = $"{_baseApiUrl}distribuicaoinstrucao/GetProcessosUsuario/{usuario}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<List<ListaProcessoUsuario>>();

            return new List<ListaProcessoUsuario>();
        }

        public async Task<List<ListaProcessoUsuario>> BuscarProcessoSetor(string usuario)
        {
            var apiUrl = $"{_baseApiUrl}distribuicaoinstrucao/GetProcessoSetor/{usuario}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<List<ListaProcessoUsuario>>();

            return new List<ListaProcessoUsuario>();

        }

        public async Task<IActionResult> BuscarProcesso(int movpro_id)
        {
            var apiUrl = $"{_baseApiUrl}distribuicaoinstrucao/GetProcesso/{movpro_id}"; 
            var response = await _httpClient.GetAsync(apiUrl);

            // Verifica erros tratados
            var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var processo = await response.Content.ReadFromJsonAsync<ListaProcessoUsuario>();
                return Json(processo);
            }
            
            return Json(new { error = true, message = "Processo não encontrado" });
        }     

        public async Task<IActionResult> ProcessoPorAssuntoView(string userMatrix)
        {
            
            var apiUrl = $"{_baseApiUrl}distribuicaoinstrucao/GetProcessosDistribuidoUsuario/{userMatrix}";
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
        public async Task<IActionResult> DistribuirProcessos([FromBody] List<ProtocoloDistribuicaoModel> distribuicoes)
        {
            foreach (var distribuicaoModel in distribuicoes)
            {
                distribuicaoModel.DIS_ORIGEM_USUARIO = userMatrix;

                var apiUrl = $"{_baseApiUrl}distribuicaoinstrucao/adddistribuicaoprocesso";
                var response = await _httpClient.PostAsJsonAsync(apiUrl, distribuicaoModel);

                var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
                if (resultadoErro != null)
                    return resultadoErro;
            }

            await CarregarDistribuicao(userMatrix); // Certifique-se que esse método é async ou remova o await se for void
            return PartialView("_Assunto");
        }


        [HttpPost]
        public async Task<IActionResult> RetirarProcessos([FromBody] List<ProtocoloDistribuicaoModel> distribuicoes)
        {
            foreach (var distribuicaoModel in distribuicoes)
            {
                distribuicaoModel.DIS_ORIGEM_USUARIO = userMatrix;

                var apiUrl = $"{_baseApiUrl}distribuicaoinstrucao/retirarprocesso";
                var response = await _httpClient.PostAsJsonAsync(apiUrl, distribuicaoModel);

                var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
                if (resultadoErro != null)
                    return resultadoErro;
            }

            await CarregarDistribuicao(userMatrix); // Ou apenas CarregarDistribuicao se não for async
            return PartialView("_Assunto");
        }


        [HttpPost]
        public async Task<IActionResult> addDistribuicaoProcessoEspecifico([FromBody] ProtocoloDistribuicaoModel distribuicaoModel)
        {

            distribuicaoModel.DIS_ORIGEM_USUARIO = userMatrix;

            //Add processo ao usuario
            var apiUrl = $"{_baseApiUrl}distribuicaoinstrucao/adddistribuicaoprocessoespecifico";
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

            var apiUrl = $"{_baseApiUrl}distribuicaoinstrucao/retirarprocessoespecifico";
            var response = await _httpClient.PostAsJsonAsync(apiUrl, distribuicaoModel);

            var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;

            await CarregarDistribuicao(userMatrix);

            return PartialView("_Assunto");
        }


        private async Task<IActionResult?> CarregarDistribuicao(string userMatrix)
        {
            var ProcessorDictionary = new Dictionary<string, List<ListaProcessoUsuario>>();

            // Buscar assuntos
            var apiUrl = $"{_baseApiUrl}distribuicaoinstrucao/Getprocessosassunto/{userMatrix}";
            var response = await _httpClient.GetAsync(apiUrl);

            var resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var assuntos = await response.Content.ReadFromJsonAsync<List<AssuntoQtd>>();
                ViewBag.Assunto = assuntos;
                await ProcessoPorAssuntoView(userMatrix);
            }

            // Buscar processos do usuário
            apiUrl = $"{_baseApiUrl}distribuicaoinstrucao/GetProcessosDistribuidoUsuario/{userMatrix}";
            response = await _httpClient.GetAsync(apiUrl);

            resultadoErro = await ApiErrorHandler.TratarErrosHttpResponse(response, Url);
            if (resultadoErro != null)
                return resultadoErro;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var processos = await response.Content.ReadFromJsonAsync<List<ProtocoloDistribuicaoModel>>();
                ViewBag.Usuario = processos;

                foreach (var item in processos)
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



