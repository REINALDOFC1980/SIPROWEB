using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIPROSHARED.DbContext;
using SIPROSHARED.Models;
using SIPROSHAREDINSTRUCAO.Models;
using SIPROSHAREDINSTRUCAO.Service.IRepository;
using SIRPOEXCEPTIONS.ExceptionBase;
using System.Net.Http;

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
        public async Task<IActionResult> LocalizarInstrucao(string resultado, string vlobusca)
        {

            var processo = await _instrucao.LocalizarInstrucao(resultado, vlobusca);

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
                        
                        await _instrucao.SalvarAnexo(pasta, instrucao.INSPRO_Usuario_origem, instrucao.INSPRO_Dis_id, connection, transaction);

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

        //anexo
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFiles([FromForm] List<IFormFile> arquivos, [FromForm] string protocolo)
        {
            await _instrucao.UploadAnexo(arquivos, protocolo);
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


        [HttpDelete("excluir-imagem/{protocolo}/{arquivo}")]
        public IActionResult ExcluirImagem(string protocolo, string arquivo)
        {
            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", protocolo);
            var filePath = Path.Combine(folderPath, arquivo);

            if (!System.IO.File.Exists(folderPath))
                throw new ErrorOnValidationException(new List<string> { "Arquivo do protocolo não encontrada." });

           
            System.IO.File.Delete(filePath);
            return Ok();
             
        }





    }
}
