using Dapper;
using FluentValidation;
using SIPROSHARED.DbContext;
using SIPROSHARED.Models;
using SIPROSHARED.Validator;
using SIPROSHAREDPUBLICACAO.Model;
using SIPROSHAREDPUBLICACAO.Service.IRepository;
using SIRPOEXCEPTIONS.ExceptionBase;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDPUBLICACAO.Service.Repository
{
    public class PublicacaoService : IPublicacaoService
    {
        private readonly DapperContext _context;

        public PublicacaoService(DapperContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> QuantidadeProcesso(string usuario)
        {
            try
            {
                var query = @"
                             
                        declare @SetorUsuario int    

                        Select @SetorUsuario =  SETSUBUSU_SETSUB_ID  
                          From SetorSubXUsuario  
                         where SETSUBUSU_USUARIO = @usuario  and SETSUBUSU_PERFIL = 'Presidente'  
  

                        select COUNT(distinct(prt_numero)) as Prt_publicacao_qtd 
                                   from Protocolo as a   
                                   join Movimentacao_Processo as b on (a.PRT_NUMERO = b.MOVPRO_PRT_NUMERO)  
                                   join SetorSub as c on (c.SETSUB_ID = b.MOVPRO_SETOR_DESTINO)  
                                   join SetorsubxUsuario  as g on (g.SETSUBUSU_SETSUB_ID = c.SETSUB_ID)  
  
                             where b.MOVPRO_SETOR_DESTINO = @SetorUsuario
                               and PRT_ACAO = 'PUBLICAR'";

                using (var connection = _context.CreateConnection())
                {
                    var parametros = new { usuario };
                    int quantidade = await connection.ExecuteScalarAsync<int>(query, parametros);
                    return quantidade;
                }
            }
            catch (Exception ex)
            {
                // Melhor manipulação de exceções: log ou tratar adequadamente
                throw new Exception("Erro ao obter dados.", ex);
            }

        }


        public async Task GerarLote(string usuario)
        {

            try
            {

                var dbParametro = new DynamicParameters();
                dbParametro.Add("@Usuario", usuario);

                string query = @"

               
                        --pegando o setor do usuário  
                        declare 
                        @SetorUsuario int  
 
 
                        select @SetorUsuario = SETSUBUSU_SETSUB_ID  
                        from SetorSubXUsuario  
                        where SETSUBUSU_USUARIO =  @Usuario AND SETSUBUSU_PERFIL = 'Presidente' 
 
                        Select distinct a.PRT_NUMERO
                          into #Publicar   
                          from Protocolo as a   
                               join Movimentacao_Processo as b on (a.PRT_NUMERO = b.MOVPRO_PRT_NUMERO)  
                               join SetorSub as c on (c.SETSUB_ID = b.MOVPRO_SETOR_DESTINO)  
                               join SetorsubxUsuario  as g on (g.SETSUBUSU_SETSUB_ID = c.SETSUB_ID)             
                         where b.MOVPRO_SETOR_DESTINO = @SetorUsuario
                           and PRT_ACAO = 'PUBLICAR'
  

                                declare @NLoteGerado varchar(20)  
                                exec Stb_GerarLote @NLoteGerado out   
  
  
                                update protocolo  
                                   set PRT_LOTE = @NLoteGerado,  
                                       PRT_DT_LOTE = GETDATE(),  
                                       PRT_ACAO = 'LOTE GERADO'
                                  from #Publicar  
                                 where protocolo.PRT_NUMERO = #Publicar.PRT_NUMERO  
  
                                drop table #Publicar  


            ";
                using (var con = _context.CreateConnection())
                {
                    await con.ExecuteAsync(query, dbParametro);
                }
            }
            catch (Exception ex)
            {

                throw;
            }



        }

        public async Task<PublicacaoModel> Buscar_Lote(string Lote)
        {
            var query = @"
                        Select 
                            prt_lote, 
                            Convert(varchar(10), PRT_DT_LOTE, 103) as prt_dt_lote, 
                            COUNT(1) AS prt_publicacao_qtd,  
                            ISNULL(PRT_PUBLICACAO_DOM, '') as prt_publicacao_dom, 
                            ISNULL(Convert(varchar(10), PRT_DT_PUBLICACAO, 103), '') as prt_dt_publicacao
                        from Protocolo
                        where REPLACE(PRT_LOTE, '/', '') = @Lote
                        group by PRT_LOTE, PRT_DT_LOTE, PRT_PUBLICACAO_DOM, PRT_DT_PUBLICACAO
                         ";

            using (var connection = _context.CreateConnection())
            {
                var parametros = new { Lote };
                var command = await connection.QueryFirstOrDefaultAsync<PublicacaoModel>(query, parametros);
                return command;
            }

        }

        public async Task<List<PublicacaoModel>> BuscarLotes(string usuario)
        {

            var query = @"
                            Select 
                            prt_lote, 
                            Convert(varchar(10), PRT_DT_LOTE, 103) as prt_dt_lote, 
                            COUNT(1) AS prt_publicacao_qtd,  
                            ISNULL(PRT_PUBLICACAO_DOM, '') as prt_publicacao_dom, 
                            ISNULL(Convert(varchar(10), PRT_DT_PUBLICACAO, 103), '') as prt_dt_publicacao
                          from Protocolo
                         where PRT_DT_LOTE is not null 
                               and PRT_ACAO NOT IN('ARQUIVADO')
                         group by PRT_LOTE, 
                               PRT_DT_LOTE,
                               PRT_PUBLICACAO_DOM, 
                               PRT_DT_PUBLICACAO
                         ";

            using (var connection = _context.CreateConnection())
            {
                var parametros = new { usuario };
                var command = await connection.QueryAsync<PublicacaoModel>(query, parametros);
                return command.ToList();
            }
        }

        public async Task AtualizarPublicacao(PublicacaoModel publicacaoModel)
        {

            var dbParametro = new DynamicParameters();
            dbParametro.Add("PRT_DT_PUBLICACAO", publicacaoModel.prt_dt_publicacao);
            dbParametro.Add("PRT_PUBLICACAO_DOM", publicacaoModel.prt_publicacao_dom);
            dbParametro.Add("PRT_LOTE", publicacaoModel.prt_lote);
            dbParametro.Add("@Usuario", publicacaoModel.prt_usu_publicacao);


            string query = @"
                           
                             UPDATE Protocolo  
                                SET PRT_DT_PUBLICACAO = convert(datetime, @PRT_DT_PUBLICACAO, 103),  
                                    PRT_PUBLICACAO_DOM = @PRT_PUBLICACAO_DOM,  
                                    PRT_ACAO = 'ARQUIVADO',  
                                    PRT_DT_ARQUIVO = GETDATE(),  
                                    PRT_USUARIOARQUIVO = @Usuario  
                             FROM Protocolo   
                             WHERE PRT_LOTE = @PRT_LOTE 
  
  
                              Select Prt_Numero 
                              Into #Temp_Arquivar
                              from Protocolo where PRT_LOTE = @PRT_LOTE

                              UPDATE MP
                                 SET MOVPRO_STATUS = 'PUBLICADO/ARQUIVAR'
	                            from #Temp_Arquivar TA INNER JOIN Movimentacao_Processo MP on( Prt_Numero = MOVPRO_PRT_NUMERO)
                               WHERE MOVPRO_STATUS = 'PUBLICAR'

                              
                            Insert into Movimentacao_Processo
                             ( MOVPRO_PRT_NUMERO    
                               ,MOVPRO_SETOR_ORIGEM 
                               ,MOVPRO_USUARIO_ORIGEM                                                                                
                               ,MOVPRO_DATA_ORIGEM      
                               ,MOVPRO_ACAO_ORIGEM                                                                                                                                                                                                                                               
                               ,MOVPRO_SETOR_DESTINO 
                               ,MOVPRO_STATUS)
                         select MOVPRO_PRT_NUMERO,
                                MOVPRO_SETOR_ORIGEM,
                                MOVPRO_USUARIO_ORIGEM,
                                GETDATE(),
                               'Processo publicado no DOM e arquivado.',
                                0,
                               'ARQUIVADO'

                            from #Temp_Arquivar inner join Movimentacao_Processo MP on(PRT_NUMERO = MOVPRO_PRT_NUMERO) and MOVPRO_STATUS = 'PUBLICADO/ARQUIVAR'";


            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, dbParametro);
            }
        }

        public async Task ExcluirLote (string lote)
        {
            try
            {
                var dbParametro = new DynamicParameters();
                dbParametro.Add("@prt_lote", lote);

                string query = @"UPDATE Protocolo  
                                    SET PRT_ACAO = 'PUBLICAR',  
                                        PRT_LOTE = '',
		                                PRT_DT_LOTE = NULL
                                  WHERE Replace(PRT_LOTE,'/','') = @prt_lote";

                using (var con = _context.CreateConnection())
                {
                    await con.ExecuteAsync(query, dbParametro);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
           
        }

        public async Task<List<PublicacaoDOMModel>> GerarDOM(string lote)
        {

            var query = @"
                           SELECT PES_Nome,
                           PRT_NUMERO,
	                       PRT_AIT, 
	                       Case when PRT_RESULTADO = 'D' then 'DEFERIDO' ELSE 'INDEFERIDO' END PRT_RESULTADO
                           FROM Protocolo 
                           INNER JOIN Pessoa on(PRT_CPF_SOLICITANTE = PES_CPF) 
                           where REPLACE(PRT_LOTE, '/', '') = @Lote
                         ";

            using (var connection = _context.CreateConnection())
            {
                var parametros = new { lote };
                var result = await connection.QueryAsync<PublicacaoDOMModel>(query, parametros);
                return result.ToList();
            }


        }
    }
}
