using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIPROSHARED.DbContext;
using SIPROSHARED.Models;
using SIPROSHAREDINSTRUCAO.Models;
using SIPROSHAREDINSTRUCAO.Service.IRepository;
using SIRPOEXCEPTIONS.ExceptionBase;
using System.Net.Http;
using System.Text.Json;

namespace SIPROAPIINSTRUCAO.Controllers
{
    [Route("sipro/instrucao")]
    [ApiController]
    public class InstrucaoController : ControllerBase
    {

        private readonly ILogger<InstrucaoController> _logger;
        private readonly IInstrucaoService _instrucao;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly DapperContext _context;


        public InstrucaoController(IInstrucaoService julgamento,
                                   IHttpClientFactory httpClientFactory,
                                   DapperContext context,
                                   ILogger<InstrucaoController> logger)
        {
            _instrucao = julgamento;
            _httpClientFactory = httpClientFactory;
            _context = context;
            _logger = logger;
        }


        [HttpGet]
        [Route("localizar-instrucao/{usuario}/{vlobusca}")]
        public async Task<IActionResult> LocalizarInstrucao(string usuario, string vlobusca)
        {

            var processo = await _instrucao.LocalizarInstrucao(usuario, vlobusca);

            if (processo.Count == 0)
                return NoContent();

            return Ok(processo);
           
        }


        [HttpGet]
        [Route("get-instrucao/{dis_id}")]
        public async Task<IActionResult> GetInstrucao(int dis_id)
        {
           
            var processo = await _instrucao.GetInstrucao(dis_id);

            if (processo == null)
                return NoContent();

            return Ok(processo);
        }


        [HttpGet]
        [Route("movimentacao-instrucao/{dis_id}")]
        public async Task<IActionResult> BuscarMovimentacaoInstrucao(int dis_id)
        {

            var processo = await _instrucao.BuscarMovimentacaoInstrucao(dis_id);

            if (processo == null)
                return NoContent();

            return Ok(processo);
            
        }


        [HttpGet]
        [Route("buscar-setor")]
        public async Task<IActionResult> BuscarSetor()
        {           
            var setor = await _instrucao.BuscarSetor();

            if (setor == null)
            return NoContent();

            return Ok(setor);            
        }


        [HttpPost]
        [Route("encaminhar-instrucao")]
        public async Task<IActionResult> EncamimharInstrucao(InstrucaoModel instrucao)
        {
            using (var connection = _context.CreateConnection())
            {
                // Abrir a conexão
                connection.Open();

                // Iniciar a transação
                using (var transaction = connection.BeginTransaction())
                {
                    var pasta = instrucao.INSPRO_PRT_NUMERO.Replace("/", "");
                    try
                    {       
                        await _instrucao.EncaminharIntrucao(instrucao, connection, transaction);
                                             

                        transaction.Commit();

                        return Ok();

                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }          
        }

      

        [HttpPost]
        [Route("inserir-anexo")]
        public async Task<IActionResult> InserirAnexo([FromForm] List<IFormFile> arquivos, [FromForm] string protocoloJson)
        {

            var protocolo = JsonSerializer.Deserialize<ProtocoloModel>(protocoloJson);
            await _instrucao.IntoAnexo(arquivos, protocolo);

            return Ok();

        }


        [HttpGet("listar-imagens")]
        public IActionResult ListarImagens(string protocolo)
        {
            
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", protocolo);

            if (!Directory.Exists(folderPath))
                throw new ErrorOnValidationException(new List<string> { "Pasta do protocolo não encontrada." });  

            var arquivos = Directory.GetFiles(folderPath)
                                    .Select(Path.GetFileName)
                                    .ToList();
            return Ok(arquivos);
        }



        [HttpGet]
        [Route("buscar-anexo-banco/{prt_numero}/{usuario}")]
        public async Task<IActionResult> BuscarAnexosBanco(string prt_numero, string usuario)
        {

            var anexos = await _instrucao.BuscarAnexosBanco(prt_numero, usuario);

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

            await _instrucao.ExcluirAnexo(prtdoc_id);
            return Ok();

        }





    }
}
