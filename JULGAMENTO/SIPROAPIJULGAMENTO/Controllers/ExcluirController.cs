using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIPROSHARED.DbContext;
using SIPROSHAREDJULGAMENTO.Service.IRepository;

namespace SIPROAPIJULGAMENTO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExcluirController : ControllerBase
    {

        private readonly ILogger<ExcluirController> _logger;
        private readonly IExcluirVotoService _excluirVotoService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DapperContext _context;


        public ExcluirController(IExcluirVotoService excluirVotoService,
                                   IHttpClientFactory httpClientFactory,
                                   DapperContext context,
                                   ILogger<ExcluirController> logger)
        {
            _excluirVotoService = excluirVotoService;
            _httpClientFactory = httpClientFactory;
            _context = context;
            _logger = logger;
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
    }
}
