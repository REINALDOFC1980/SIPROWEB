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

            publicar_model.prt_publicacao_qtd = processo;

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
        [Route("buscar-lotes/{usuario}")]
        public async Task<IActionResult> BuscarLotes(string usuario)
        {
            var result = await _publicacao.BuscarLotes(usuario);

            if (result == null)
                return NoContent();

            return Ok(result);
        }


        [HttpGet]
        [Route("buscar-lote/{lote}")]
        public async Task<IActionResult> Buscar_Lote(string lote)
        {
            var result = await _publicacao.Buscar_Lote(lote);

            if (result == null)
                return NoContent();

            return Ok(result);
        }


        [HttpPost]
        [Route("atualizar-publicacao")]
        public async Task<IActionResult> AtualizarPublicacao(PublicacaoModel publicacaoModel)
        {
            await _publicacao.AtualizarPublicacao(publicacaoModel);

            return Ok();
        }


        [HttpPut]
        [Route("excluir-lote/{lote}")]
        public async Task<IActionResult> ExcluirLote(string lote)
        {

            await _publicacao.ExcluirLote(lote);

            return Ok();

        }



        [HttpGet]
        [Route("gerar-dom/{lote}")]
        public async Task<IActionResult> GerarDOM(string lote)
        {
            var result = await _publicacao.GerarDOM(lote);

            if (result == null)
                return NoContent();

            return Ok(result);
        }


    }
}
