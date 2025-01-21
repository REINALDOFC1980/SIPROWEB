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
                    return RedirectToAction("Error", "Home");
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var result = await response.Content.ReadFromJsonAsync<List<AssuntoQtd>>();
                ViewBag.Assunto = result;
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


        //parei aqui ver quem está usando BuscarProcesso!

        public async Task<IActionResult> BuscarProcesso(int movpro_id)
        {
            var apiUrl = $"{_baseApiUrl}distribuicao/GetProcesso/{movpro_id}"; 
            var response = await _httpClient.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
                return PartialView("_ErrorPartialView");

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var processo = await response.Content.ReadFromJsonAsync<ListaProcessoUsuario>();
                return Json(processo);
            }
            
            return Json(new { error = true, message = "Processo não encontrado" });
        }

        public async Task<List<ProtocoloDistribuicaoModel>> BuscarUsuarioSetor(string usuario)
        {
            var apiUrl = $"{_baseApiUrl}distribuicao/getUsuarioSetor/{usuario}";
            var response = await _httpClient.GetAsync(apiUrl);

            if (response.StatusCode == HttpStatusCode.OK)
                return await response.Content.ReadFromJsonAsync<List<ProtocoloDistribuicaoModel>>();
                //return processosUsuario ?? new List<ProtocoloDistribuicaoModel>();

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
        public async Task<PartialViewResult> ProcessoPorAssuntoView(string userMatrix)
        {
            try
            {
                var apiUrl = $"{_baseApiUrl}distribuicao/GetProcessosDistribuidoUsuario/{userMatrix}";
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    List<ProtocoloDistribuicaoModel> result = await response.Content.ReadFromJsonAsync<List<ProtocoloDistribuicaoModel>>();
                    ViewBag.Usuario = result;
                }

                return PartialView("_ProcessoDistribuidos");
            }
            catch (Exception ex)
            {
                throw;
            }

        }
        
        [HttpPost]
        public async Task<PartialViewResult> addDistribuicaoProcessoEspecifico([FromBody] ProtocoloDistribuicaoModel distribuicaoModel)
        {
            var ProcessorDictionary = new Dictionary<string, List<ListaProcessoUsuario>>();
            ViewBag.ListaProcessosSetor = new List<ListaProcessoUsuario>();
            ViewBag.ListaProcessos = new List<ProtocoloDistribuicaoModel>();
            ViewBag.Usuario = new List<ProtocoloDistribuicaoModel>();
            ViewBag.Assunto = new List<AssuntoQtd>();

            try
            {

                distribuicaoModel.DIS_ORIGEM_USUARIO = userMatrix;

                //Add processo ao usuario
                var apiUrl = $"{_baseApiUrl}distribuicao/adddistribuicaoprocessoespecifico";
                var response = await _httpClient.PostAsJsonAsync(apiUrl, distribuicaoModel);

                if (!response.IsSuccessStatusCode)
                    return PartialView("_ErrorPartialView");


                // atualizando tela de usuário
                apiUrl = $"{_baseApiUrl}distribuicao/Getprocessosassunto/{userMatrix}";
                response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                    return PartialView("_ErrorPartialView");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<AssuntoQtd>>();
                    ViewBag.Assunto = result;
                    await ProcessoPorAssuntoView(userMatrix);
                }


                //Buscando os processo do usuario
                apiUrl = $"{_baseApiUrl}distribuicao/GetProcessosDistribuidoUsuario/{userMatrix}";
                response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                    return PartialView("_ErrorPartialView");

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
             
                return PartialView("_Assunto");
            }
            catch (Exception ex)
            {
                // Log da exceção
                return PartialView("_ErrorPartialView");
                throw;
            }
        }

        [HttpPost]
        public async Task<PartialViewResult> addDistribuicaoProcesso([FromBody] ProtocoloDistribuicaoModel distribuicaoModel)
        {
            var ProcessorDictionary = new Dictionary<string, List<ListaProcessoUsuario>>();
            ViewBag.ListaProcessosSetor = new List<ListaProcessoUsuario>();
            ViewBag.ListaProcessos = new List<ProtocoloDistribuicaoModel>();
            ViewBag.Usuario = new List<ProtocoloDistribuicaoModel>();
            ViewBag.Assunto = new List<AssuntoQtd>();

            try
            {

                distribuicaoModel.DIS_ORIGEM_USUARIO = userMatrix;

                //ADD 
                var apiUrl = $"{_baseApiUrl}distribuicao/adddistribuicaoprocesso";
                var response = await _httpClient.PostAsJsonAsync(apiUrl, distribuicaoModel);

                if (!response.IsSuccessStatusCode)
                   return PartialView("_ErrorPartialView");


                // ATUALIZANDO A QTD NA TELA
                apiUrl = $"{_baseApiUrl}distribuicao/Getprocessosassunto/{userMatrix}";
                response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                    return PartialView("_ErrorPartialView");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<AssuntoQtd>>();
                    ViewBag.Assunto = result;
                    await ProcessoPorAssuntoView(userMatrix);
                }            


                //BUSCANDO OS PROCESSOS VINCULADOS
                apiUrl = $"{_baseApiUrl}distribuicao/GetProcessosDistribuidoUsuario/{userMatrix}";
                response = await _httpClient.GetAsync(apiUrl);


                if (response.StatusCode == HttpStatusCode.OK)
                {
                    List<ProtocoloDistribuicaoModel> result = await response.Content.ReadFromJsonAsync<List<ProtocoloDistribuicaoModel>>();
                    ViewBag.Usuario = result;

                    foreach (var item in result)
                    {
                        var itens = await BuscarProcessoUsuario(item.DIS_DESTINO_USUARIO);
                        ProcessorDictionary[item.DIS_DESTINO_USUARIO] = itens;
                    }

                    ViewBag.ListaProcessos = ProcessorDictionary; //
                    ViewBag.ListaProcessosSetor = await BuscarProcessoSetor(userMatrix);
                }

                return PartialView("_Assunto");
            }
            catch (Exception ex)
            {
                return PartialView("_ErrorPartialView");
                throw;
            }
        }

        [HttpPost]
        public async Task<PartialViewResult> RetirarProcesso([FromBody] ProtocoloDistribuicaoModel distribuicaoModel)
        {
            var ProcessorDictionary = new Dictionary<string, List<ListaProcessoUsuario>>();
            ViewBag.ListaProcessosSetor = new List<ListaProcessoUsuario>();
            ViewBag.ListaProcessos = new List<ProtocoloDistribuicaoModel>();
            ViewBag.Usuario = new List<ProtocoloDistribuicaoModel>();
            ViewBag.Assunto = new List<AssuntoQtd>();

            try
            {
               // string? userMatrix = "CAIOCSO";  // Pega o usuário
                distribuicaoModel.DIS_ORIGEM_USUARIO = userMatrix;   

                //ADD 
                var apiUrl = $"{_baseApiUrl}distribuicao/retirarprocesso";
                var response = await _httpClient.PostAsJsonAsync(apiUrl, distribuicaoModel);

                if (!response.IsSuccessStatusCode)
                    return PartialView("_ErrorPartialView");            


                // ATUALIZANDO A QTD NA TELA
                apiUrl = $"{_baseApiUrl}distribuicao/Getprocessosassunto/{userMatrix}";
                response = await _httpClient.GetAsync(apiUrl);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadFromJsonAsync<List<AssuntoQtd>>();
                    ViewBag.Assunto = result;
                    await ProcessoPorAssuntoView(userMatrix);
                }


                //BUSCANDO OS PROCESSOS VINCULADOS
                apiUrl = $"{_baseApiUrl}distribuicao/GetProcessosDistribuidoUsuario/{userMatrix}";
                response = await _httpClient.GetAsync(apiUrl);


                if (response.StatusCode == HttpStatusCode.OK)
                {
                    List<ProtocoloDistribuicaoModel> result = await response.Content.ReadFromJsonAsync<List<ProtocoloDistribuicaoModel>>();
                    ViewBag.Usuario = result;

                    //alimentando a grids de cada usaurio
                    foreach (var item in result)
                    {
                        // Chama o método para buscar os itens e armazena no dicionário
                        var itens = await BuscarProcessoUsuario(item.DIS_DESTINO_USUARIO);
                        ProcessorDictionary[item.DIS_DESTINO_USUARIO] = itens;
                    }

                    ViewBag.ListaProcessos = ProcessorDictionary; //ATUALIZANDO OS PROCESSO VINCULADO AO USUARIO
                    ViewBag.ListaProcessosSetor = await BuscarProcessoSetor(userMatrix);//ATUALIZANDO OS PROCESSO VINCULADO AO SETOR
                }


                return PartialView("_Assunto");
            }
            catch (Exception ex)
            {
                // Log da exceção
                return PartialView("_ErrorPartialView");
                throw;
            }
        }

        [HttpPost]
        public async Task<PartialViewResult> RetirarProcessoEspecifico(int DIS_ID)
        {
            var ProcessorDictionary = new Dictionary<string, List<ListaProcessoUsuario>>();
            ProtocoloDistribuicaoModel distribuicaoModel = new ProtocoloDistribuicaoModel();
            ViewBag.ListaProcessosSetor = new List<ListaProcessoUsuario>();
            ViewBag.ListaProcessos = new List<ProtocoloDistribuicaoModel>();
            ViewBag.Usuario = new List<ProtocoloDistribuicaoModel>();
            ViewBag.Assunto = new List<AssuntoQtd>();

            distribuicaoModel.DIS_ID = DIS_ID;
         
            try 
            {
                //ADD 
                var apiUrl = $"{_baseApiUrl}distribuicao/retirarprocessoespecifico";
                var response = await _httpClient.PostAsJsonAsync(apiUrl, distribuicaoModel);

                if (!response.IsSuccessStatusCode)
                    return PartialView("_ErrorPartialView");


                // ATUALIZANDO A QTD NA TELA
                apiUrl = $"{_baseApiUrl}distribuicao/Getprocessosassunto/{userMatrix}";
                var getResponse = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                    return PartialView("_ErrorPartialView");

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await getResponse.Content.ReadFromJsonAsync<List<AssuntoQtd>>();
                    ViewBag.Assunto = result;
                    await ProcessoPorAssuntoView(userMatrix);
                }

                //BUSCANDO OS PROCESSOS VINCULADOS
                apiUrl = $"{_baseApiUrl}distribuicao/GetProcessosDistribuidoUsuario/{userMatrix}";
                response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                    return PartialView("_ErrorPartialView");


                if (response.StatusCode == HttpStatusCode.OK)
                {
                    List<ProtocoloDistribuicaoModel> result = await response.Content.ReadFromJsonAsync<List<ProtocoloDistribuicaoModel>>();
                    ViewBag.Usuario = result;

                    //alimentando a grids de cada usaurio
                    foreach (var item in result)
                    {
                        // Chama o método para buscar os itens e armazena no dicionário
                        var itens = await BuscarProcessoUsuario(item.DIS_DESTINO_USUARIO);
                        ProcessorDictionary[item.DIS_DESTINO_USUARIO] = itens;
                    }

                    ViewBag.ListaProcessos = ProcessorDictionary;
                    ViewBag.ListaProcessosSetor = await BuscarProcessoSetor(userMatrix);
                }


                return PartialView("_Assunto");
            }
            catch (Exception ex)
            {
                // Log da exceção
                return PartialView("_ErrorPartialView");
                throw;
            }
        }

    }
}



