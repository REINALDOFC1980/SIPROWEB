﻿using Microsoft.AspNetCore.Mvc;
using SIPROSHARED.DbContext;
using SIPROSHAREDDISTRIBUICAO.Models;
using SIPROSHAREDDISTRIBUICAO.Service.IRepository;

namespace SIPROAPIDISTRIBUICAO.Controllers
{
    [Route("sipro/distribuicao")]
    [ApiController]
    public class DistribuicaoController : ControllerBase
    {
        private readonly ILogger<DistribuicaoController> _logger;
        private readonly IDistribuicaoService _distribuicao;
        private readonly DapperContext _context;


          public DistribuicaoController(IDistribuicaoService distribuicao,  
                                        DapperContext context,
                                        ILogger<DistribuicaoController> logger)
          {
             _distribuicao = distribuicao;
             _context = context;
             _logger = logger;
          }


        [HttpGet]
        [Route("Getprocessosassunto/{buscas}")]
        public async Task<IActionResult> GetProcessoDistribuir(string buscas)
        {    
             var processo = await _distribuicao.GetQtdProcessosPorAssunto(buscas);

             if (processo == null || processo.Count == 0)
             {
                 processo = new List<AssuntoQtd>
                 {
                     new AssuntoQtd
                     {
                         PRT_ASSUNTO = "NÃO CONSTA PROCESSO",
                         PRT_QTD = 0
                     }
                 };
             }

             return Ok(processo);           
        }

        [HttpGet]
        [Route("GetProcessosDistribuidoUsuario/{buscas}")]
        public async Task<IActionResult> GetQtdProcessosDistribuidoPorUsuario(string buscas)
        {           
            var processo = await _distribuicao.GetQtdProcessosDistribuidoPorUsuario(buscas);

            if (processo == null)            
                return NoContent();
            
            return Ok(processo);           
        }

        [HttpGet]
        [Route("GetProcessoSetor/{buscas}")]
        public async Task<IActionResult> GetProcessoSetor(string buscas)
        {

             var processo = await _distribuicao.GetProcessoSetor(buscas);

             if (processo == null)
                 return NoContent();

            return Ok(processo);
           
        }

        [HttpGet]
        [Route("GetProcesso/{buscas}")]
        public async Task<IActionResult> GetProcesso(int buscas)
        {
           
            var processo = await _distribuicao.GetProcesso(buscas);

            if (processo == null)
                return NoContent();

            return Ok(processo);
           
        }
        
        [HttpGet]
        [Route("GetProcessosUsuario/{buscas}")]        
        public async Task<IActionResult> GetProcessosUsuario(string buscas)
        {

            var processo = await _distribuicao.GetProcessosUsuario(buscas);

            if (processo == null)
                return NoContent();

            return Ok(processo);
           
        }
        
        [HttpPost]
        [Route("adddistribuicaoprocesso")]
        public async Task<IActionResult> AddDistribuicaoProcesso(ProtocoloDistribuicaoModel distribuicaoModel)
        {
            using (var connection = _context.CreateConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await _distribuicao.DistribuicaoProcesso(distribuicaoModel, connection, transaction);
                      
                        transaction.Commit();

                        _logger.LogInformation("ID Movimentacao: {MovimentacaoId}", distribuicaoModel.DIS_MOV_ID);
                        
                        _logger.LogInformation("Usuário: {Usuario}", distribuicaoModel.DIS_ORIGEM_USUARIO);

                        return Ok(new { message = "Processo distribuído com sucesso." });

                    }
                    catch
                    {
                        transaction.Rollback();
                        throw; 
                    }
                }
            }
        }

        [HttpPost]
        [Route("adddistribuicaoprocessoespecifico")]
        public async Task<IActionResult> AddDistribuicaoProcessoEspecifico(ProtocoloDistribuicaoModel distribuicaoModel)
        {
            using (var connection = _context.CreateConnection())
            {
                // Abrir a conexão
                connection.Open();

                // Iniciar a transação
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {

                        await _distribuicao.DistribuicaoProcessoEspecifico(distribuicaoModel, connection, transaction);
                        transaction.Commit();

                        _logger.LogInformation("ID Movimentacao: {MovimentacaoId}", distribuicaoModel.DIS_MOV_ID);
                        _logger.LogInformation("Usuário: {Usuario}", distribuicaoModel.DIS_ORIGEM_USUARIO);

                        return Ok();

                    }
                    catch (Exception)
                    {
                        // Registrar a exceção (supondo que um logger esteja configurado)
                        //_logger.LogError(ex, "Ocorreu um erro ao buscar os dados do solicitante.");
                        transaction.Rollback();
                        throw;
                    }
                }
            }         
        }

        [HttpPost]
        [Route("retirarprocesso")]
        public async Task<IActionResult> RetirarProcesso(ProtocoloDistribuicaoModel distribuicaoModel)
        {
            await _distribuicao.RetirarProcesso(distribuicaoModel);

            _logger.LogInformation("ID Movimentacao: {MovimentacaoId}", distribuicaoModel.DIS_MOV_ID);
            _logger.LogInformation("Usuário: {Usuario}", distribuicaoModel.DIS_ORIGEM_USUARIO);

            return Ok(); 
        }

        [HttpPost]
        [Route("retirarprocessoespecifico")]
        public async Task<IActionResult> RetirarProcessoespecifico(ProtocoloDistribuicaoModel distribuicaoModel)
        {
          
            await _distribuicao.RetirarProcessoEspecifico(distribuicaoModel);

            _logger.LogInformation("ID Movimentacao: {MovimentacaoId}", distribuicaoModel.DIS_MOV_ID);
            _logger.LogInformation("Usuário: {Usuario}", distribuicaoModel.DIS_ORIGEM_USUARIO);
            return Ok();
            

        }




    }



}
