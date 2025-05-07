using Microsoft.AspNetCore.Mvc;
using SIPROSHARED.DbContext;
using SIPROSHARED.Models;
using SIPROSHAREDJULGAMENTO.Models;
using SIPROSHAREDJULGAMENTO.Service.IRepository;
using System.Diagnostics;
using System.Text.Json;

namespace SIPROAPIJULGAMENTO.Controllers
{
    [Route("sipro/julgamento")]
    [ApiController]
    public class JulgamentoController : ControllerBase
    {
        private readonly ILogger<JulgamentoController> _logger;
        private readonly IJulgamentoService _julgamento;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DapperContext _context;


        public JulgamentoController(IJulgamentoService julgamento,
                                    IHttpClientFactory httpClientFactory,
                                    DapperContext context,
                                    ILogger<JulgamentoController> logger)
        {
            _julgamento = julgamento;
            _httpClientFactory = httpClientFactory;
            _context = context;
            _logger = logger;

        }

        [HttpGet]
        [Route("localizar-processo/{usuario}/{vlobusca}")]
        public async Task<IActionResult> LocalizarProcesso(string usuario, string vlobusca)
        {

            var processo = await _julgamento.LocalizarProcesso(usuario, vlobusca);

            if (processo == null)
                return NoContent();

            return Ok(processo);

        }


        [HttpGet]
        [Route("localizar-processoall/{usuario}/{vlobusca}")]
        public async Task<IActionResult> LocalizarProcessosAll(string usuario, string vlobusca)
        {       
            var processo = await _julgamento.LocalizarProcessos(usuario, vlobusca);

            if (processo == null)
                return NoContent();

            return Ok(processo);            
        }


        [HttpGet]
        [Route("localizar-processos-assinar/{usuario}/{vlobusca}")]
        public async Task<IActionResult> LocalizarProcessoAssinar(string usuario, string vlobusca)
        {

            var processo = await _julgamento.LocalizarProcessosAssinar(usuario, vlobusca);

            if (processo == null)
                return NoContent();

            return Ok(processo);
            
        }


        [HttpGet]
        [Route("localizar-retificacao/{usuario}/{vlobusca}")]
        public async Task<IActionResult> LocalizarRetificacao(string usuario, string vlobusca)
        {
            var processo = await _julgamento.LocalizarRetificacao(usuario, vlobusca);

            if (processo == null)
                return NoContent();

            return Ok(processo);

        }


        [HttpGet]
        [Route("buscar-setor")]
        public async Task<IActionResult> BuscarSetor()
        {          
            var setor = await _julgamento.BuscarSetor();

            if (setor == null)
                return NoContent();

            return Ok(setor);            
        }


        [HttpGet]
        [Route("buscar-MotivoVoto")]
        public async Task<IActionResult> BuscarMotivoVoto()
        {
            var setor = await _julgamento.BuscarMotivoVoto();

            if (setor == null)
            return NoContent();

            return Ok(setor);            
        }


        [HttpGet]
        [Route("buscar-membros/{usuario}")]
        public async Task<IActionResult> BuscarMembro(string usuario)
        {
            
            var processo = await _julgamento.BuscarMembros(usuario);

            if (processo == null)
                return NoContent();

            return Ok(processo);
            
        }


        [HttpGet]
        [Route("buscar-votacao/{vlobusca}")]
        public async Task<IActionResult> BuscarVotacao(int vlobusca)
        {

            var processo = await _julgamento.BuscarVotacao(vlobusca);

            if (processo == null)
                return NoContent();

            return Ok(processo);
            
        }


        [HttpPost]
        [Route("encaminhar-processo-instrucao")]
        public async Task<IActionResult> EncamimharProcessoInstrucao(InstrucaoProcessoModel instrucao)
        {
            await _julgamento.EncamimharProcessoInstrucao(instrucao);
            return Ok();  
        }

        
        [HttpGet]
        [Route("verificar-voto/{disjul_relator}/{disjug_dis_id}")]
        public async Task<IActionResult> VericarVoto (string disjul_relator, int disjug_dis_id)
        {

            var resultado = await _julgamento.VerificarVoto(disjul_relator, disjug_dis_id);
           
            if (resultado == null)
            return NoContent();

            return Ok(resultado);
            
        }


        [HttpPost]
        [Route("inserir-voto-relator")]
        public async Task<IActionResult> InserirVotoRelator(JulgamentoProcessoModel julgamento)
        {
           
            await _julgamento.InserirVotoRelator(julgamento);
            return Ok();
            
        }


        [HttpPost]
        [Route("inserir-voto-membro")]
        public async Task<IActionResult> InserirVotoMembro(JulgamentoProcessoModel julgamento)
        {           
            await _julgamento.InserirVotoMembro(julgamento);
            return Ok();           
        }

               
        [HttpGet]
        [Route("buscar-parecer_relator/{vloBusca}")]
        public async Task<IActionResult> BuscarParecerRelator(int vloBusca)
        {           
            var processo = await _julgamento.BuscarParecerRelator(vloBusca);

            if (processo == null)
                return NoContent();

            return Ok(processo);            
        }




        [HttpPost]
        [Route("inserir-anexo")]
        public async Task<IActionResult> InserirAnexo([FromForm] List<IFormFile> arquivos, [FromForm] string protocoloJson)
        {
         
            var protocolo = JsonSerializer.Deserialize<ProtocoloModel>(protocoloJson); 
            await _julgamento.IntoAnexo(arquivos, protocolo);

            return Ok();
           
        }


        [HttpGet]
        [Route("buscar-anexo/{usuario}/{ait}")]
        public async Task<IActionResult> BuscarAnexo(string usuario, string ait)
        {
            
            var anexos = await _julgamento.BuscarAnexo(usuario, ait);

            if (anexos == null)
                return NoContent();

            return Ok(anexos);   

        }



        [HttpGet]
        [Route("buscar-anexo-banco/{prt_numero}")]
        public async Task<IActionResult> BuscarAnexosBanco(string prt_numero)
        {

            var anexos = await _julgamento.BuscarAnexosBanco(prt_numero);

            if (anexos == null)
            {
                return NoContent();
            }
            return Ok(anexos);

        }


        [HttpPost]
        [Route("excluir-anexo/{prtdoc_id}")]
        public async Task<IActionResult> ExcluirAnexo(int prtdoc_id)
        {
          
            await _julgamento.ExcluirAnexo(prtdoc_id);   
            return Ok();
            
        }


        [HttpGet]
        [Route("buscar-instrucao/{vlobusca}")]
        public async Task<IActionResult> BuscarInstrucao(string vlobusca)
        {
           
             var processo = await _julgamento.BuscarHistoricoInstrucao(vlobusca);

             if (processo == null)
             {
                return NoContent();
            }

             return Ok(processo);
            
        }



       

    }
}
