using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using SIPROSHARED.Models;
using SIPROSHARED.Service.IRepository;
using SIPROSHARED.Service.Repository;
using SIRPOEXCEPTIONS.Log;

namespace SIPROAPI.Controllers
{
    [Route("sipro/autenticacao")]
    [ApiController]
    public class AutenticacaoController : ControllerBase
    {
        private readonly IAutenticacaoService _autenticacaoServico;
        private readonly ILoggerManagerService _logger;

        public AutenticacaoController(IAutenticacaoService autenticacaoServico, ILoggerManagerService logger)
        {
            _autenticacaoServico = autenticacaoServico;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] AutenticacaoModel request)
        {
            try
            {
                var autenticacao = await _autenticacaoServico.Autenticacao(request.Usu_Login, request.Usu_Senha);

                if (autenticacao == null)
                    return NotFound("Solicitante não encontrado.");

                _logger.LogInfo("Login:  {Usu_Login}", request.Usu_Login);

                var Token = _autenticacaoServico.GerearToken(autenticacao);

                if (Token == null)
                    return Unauthorized();

                return Ok(Token);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ocorreu um erro interno no servidor.");
            }
        }

        [HttpGet("logout")]
        public IActionResult Logout([FromQuery] string login)
        {
            _logger.LogInfo("Logout: {login}", login);
            return Ok();
        }

    }

}
