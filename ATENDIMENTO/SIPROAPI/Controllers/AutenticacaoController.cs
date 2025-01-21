using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIPROSHARED.Models;
using SIPROSHARED.Service.IRepository;
using SIPROSHARED.Service.Repository;

namespace SIPROAPI.Controllers
{
    [Route("sipro/autenticacao")]
    [ApiController]
    public class AutenticacaoController : ControllerBase
    {
        private readonly IAutenticacaoService _autenticacaoServico;
        private readonly ILogger<AtendimentoController> _logger;

        public AutenticacaoController(IAutenticacaoService autenticacaoServico,
            ILogger<AtendimentoController> logger)
        {
            _autenticacaoServico = autenticacaoServico;
            _logger = logger;       
        }


        [HttpPost]
        public async Task<IActionResult> Login([FromBody] AutenticacaoModel request)
        {
            try
            {
                // Buscar dados do solicitante pra saber se tem cadastro!
                var autenticacao = await _autenticacaoServico.Autenticacao(request.Usu_Login, request.Usu_Senha);

                if (autenticacao == null)
                    return NotFound("Solicitante não encontrado.");
            
                var Token = _autenticacaoServico.GerearToken(autenticacao);
                    if (Token == null)
                    return Unauthorized();

                _logger.LogInformation("Usuário logado: "+ request.Usu_Login);

                return Ok(Token);

            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro interno no servidor.");
            }
        }



        
    }
}
