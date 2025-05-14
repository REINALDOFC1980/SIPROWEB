using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIPROSHARED.DbContext;
using SIPROSHAREDHOMOLOGACAO.Models;
using SIPROSHAREDHOMOLOGACAO.Service.IRepository;
using SIPROSHAREDHOMOLOGACAO.Service.Repository;
using SIRPOEXCEPTIONS.ExceptionBase;

namespace SIPROAPIHOMOLOGACAO.Controllers
{
    [Route("sipro/homologacao")]
    [ApiController]
    public class HomologacaoController : ControllerBase
    {
        private readonly ILogger<HomologacaoController> _logger;
        private readonly IHomologacaoService _homologacaoService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DapperContext _context;


        public HomologacaoController(IHomologacaoService homologacaoService,
                                   IHttpClientFactory httpClientFactory,
                                   DapperContext context,
                                   ILogger<HomologacaoController> logger)
        {
            _homologacaoService = homologacaoService;
            _httpClientFactory = httpClientFactory;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        [Route("localizar-homologacao/{setor}/{resultado}")]
        public async Task<IActionResult> LocalizarHomolgacao(int setor, string resultado)
        {
            var processo = await _homologacaoService.LocalizarHomolgacao(setor, resultado);

            if (processo.Count == 0)
                return NoContent();

            return Ok(processo);
        }


        [HttpGet]
        [Route("buscar-homologacao/{prt_numero}")]
        public async Task<IActionResult> GetInstrucao(string prt_numero)
        {
            var result = await _homologacaoService.BuscarHomologacao(prt_numero);

            if (result == null)
                return NoContent();

            return Ok(result);
        }


        [HttpGet]
        [Route("buscar-setor")]
        public async Task<IActionResult> BuscarSetor()
        {
            var setor = await _homologacaoService.BuscarSetor();

            if (setor == null)
                return NoContent();

            return Ok(setor);
        }


        [HttpGet]
        [Route("buscar-votacao/{processo}")]
        public async Task<IActionResult> BuscarVotacao(string processo)
        {
            var result = await _homologacaoService.BuscarVotacao(processo);

            if (result == null)
                return NoContent();

            return Ok(result);

        }



        [HttpGet]
        [Route("buscar-parecer/{processo}")]
        public async Task<IActionResult> BuscarParece(string processo)
        {
            try
            {
                var result = await _homologacaoService.BuscarParecer(processo);

                if (result == null)
                    return NoContent();

                return Ok(result);

            }
            catch (Exception)
            {

                throw;
            } 
        }



        [HttpGet]
        [Route("buscar-anexo/{ait}")]
        public async Task<IActionResult> BuscarAnexo( string ait)
        {

            var anexos = await _homologacaoService.BuscarAnexo(ait);

            if (anexos == null)
                return NoContent();

            return Ok(anexos);

        }


        [HttpGet]
        [Route("buscar-anexo-banco/{prt_numero}")]
        public async Task<IActionResult> BuscarAnexosBanco(string prt_numero)
        {

            var anexos = await _homologacaoService.BuscarAnexosBanco(prt_numero);

            if (anexos == null)
            {
                return NoContent();
            }
            return Ok(anexos);

        }



        [HttpPost]
        [Route("realizar-homologacao")]
        public async Task<IActionResult> RealizarHomologacao(JulgamentoModel julgamentoModel)
        {

            using (var connection = _context.CreateConnection())
            {
                // Abrir a conexão
                connection.Open();

                // Iniciar a transação
                using (var transaction = connection.BeginTransaction())
                {
                    
                        await _homologacaoService.RealizarHomologacao(julgamentoModel,  connection, transaction);


                        transaction.Commit();

                        return Ok();

                   
                }
            }



        }

        [HttpPost]
        [Route("homologacao-todos")]
        public async Task<IActionResult> HomologarTodos(HomologacaoModel homologacaoModel)
        {

            using (var connection = _context.CreateConnection())
            {
                // Abrir a conexão
                connection.Open();

                // Iniciar a transação
                using (var transaction = connection.BeginTransaction())
                {
                    await _homologacaoService.HomologarTodos(homologacaoModel, connection, transaction);

                    transaction.Commit();

                    return Ok();


                }
            }



        }

        [HttpPost]
        [Route("retornar-julgamento")]
        public async Task<IActionResult> RetornarJulgamento(RetificacaoModel retificacaoModel)
        {
            using (var connection = _context.CreateConnection())
            {
                // Abrir a conexão
                connection.Open();

                // Iniciar a transação
                using (var transaction = connection.BeginTransaction())
                {
                    
                        await _homologacaoService.RetornarJulgamento(retificacaoModel, connection, transaction);


                        transaction.Commit();

                        return Ok();

                    
                }
            }
        }

    }
}
