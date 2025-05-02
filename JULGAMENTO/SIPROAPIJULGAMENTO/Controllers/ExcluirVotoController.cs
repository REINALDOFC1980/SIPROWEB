using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIPROSHARED.DbContext;
using SIPROSHAREDJULGAMENTO.Models;
using SIPROSHAREDJULGAMENTO.Service.IRepository;

namespace SIPROAPIJULGAMENTO.Controllers
{
    [Route("sipro/excluirvoto")]
    [ApiController]
    public class ExcluirVotoController : ControllerBase
    {

        private readonly ILogger<ExcluirVotoController> _logger;
        private readonly IExcluirVotoService _excluirVotoService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DapperContext _context;


        public ExcluirVotoController(IExcluirVotoService excluirVotoService,
                                   IHttpClientFactory httpClientFactory,
                                   DapperContext context,
                                   ILogger<ExcluirVotoController> logger)
        {
            _excluirVotoService = excluirVotoService;
            _httpClientFactory = httpClientFactory;
            _context = context;
            _logger = logger;
        }


        [HttpGet]
        [Route("buscar-processo-excluir/{usuario}/{situacao}/{processo}")]
        public async Task<IActionResult> LocalizarProcessosExcluirVoto(string usuario, string situacao, string processo)
        {

            var resultado = await _excluirVotoService.LocalizarProcessosExcluirVoto(usuario, situacao, processo);

            if (resultado == null)
            {
                return NoContent();
            }

            return Ok(resultado);

        }


        [HttpGet]
        [Route("buscar-parecer/{processo}")]
        public async Task<IActionResult> BuscarParece(string processo)
        {
            try
            {
                var result = await _excluirVotoService.BuscarParecer(processo);

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
        [Route("buscar-votacao/{processo}")]
        public async Task<IActionResult> BuscarVotacao(string processo)
        {
            var result = await _excluirVotoService.BuscarVotacao(processo);

            if (result == null)
                return NoContent();

            return Ok(result);

        }

        [HttpPost]
        [Route("excluir-julgamento")]
        public async Task<IActionResult> RetornarJulgamento(ExcluirDetalheModel excluirDetalheModel)
        {
            using (var connection = _context.CreateConnection())
            {
                // Abrir a conexão
                connection.Open();

                // Iniciar a transação
                using (var transaction = connection.BeginTransaction())
                {

                    await _excluirVotoService.ExcluirVoto(excluirDetalheModel, connection, transaction);


                    transaction.Commit();

                    return Ok();


                }
            }
        }

    }
}
