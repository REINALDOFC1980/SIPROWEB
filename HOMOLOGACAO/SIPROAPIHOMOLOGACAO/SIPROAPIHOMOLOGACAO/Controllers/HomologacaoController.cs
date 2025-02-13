using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIPROSHARED.DbContext;
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
        public async Task<IActionResult> LocalizarInstrucao(int setor, string resultado)
        {
            var processo = await _homologacaoService.LocalizarHomolgacao(setor, resultado);

            if (processo.Count == 0)
                return NoContent();

            return Ok(processo);
        }


        [HttpGet]
        [Route("get-homologacao/{movpro_id}")]
        public async Task<IActionResult> GetInstrucao(int movpro_id)
        {
            var processo = await _homologacaoService.GetHomologacao(movpro_id);

            if (processo == null)
                return NoContent();

            return Ok(processo);
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

    }
}
