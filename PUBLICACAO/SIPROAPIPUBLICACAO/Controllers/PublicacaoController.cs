using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIPROSHARED.DbContext;
using SIPROSHAREDPUBLICACAO.Model;
using SIPROSHAREDPUBLICACAO.Service.IRepository;

namespace SIPROAPIPUBLICACAO.Controllers
{
    [Route("sipro/Publicacao")]
    [ApiController]
    public class PublicacaoController : ControllerBase
    {

        private readonly ILogger<PublicacaoController> _logger;
        private readonly IPublicacaoService _publicacao;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DapperContext _context;

        public PublicacaoController(IPublicacaoService distribuicao,
                                      IHttpClientFactory httpClientFactory,
                                      DapperContext context)
        {
            _publicacao = distribuicao;
            _httpClientFactory = httpClientFactory;
            _context = context;


        }



        [HttpGet]
        [Route("quantidade-processo/{usuario}")]
        public async Task<IActionResult> QuantidadeProcesso(string usuario)
        {
            PublicacaoModel publicar_model = new PublicacaoModel();

            var processo = await _publicacao.QuantidadeProcesso(usuario);

            publicar_model.PRT_PUBLICACAO_QDT = processo;

            return Ok(publicar_model);
        }


        [HttpPost]
        [Route("gerar-lote/{usuario}")] 
        public async Task<IActionResult> GerarLote(string usuario)
        {
            if (string.IsNullOrEmpty(usuario))
                return BadRequest("Usuário não informado.");

            await _publicacao.GerarLote(usuario);

            return Ok();
        }


        [HttpGet]
        [Route("buscar-lote/{usuario}")]
        public async Task<IActionResult> BuscarLotes(string usuario)
        {

            var processo = await _publicacao.BuscarLotes(usuario);

            if (processo == null)
                return NoContent();

            return Ok(processo);

        }

    }
}
