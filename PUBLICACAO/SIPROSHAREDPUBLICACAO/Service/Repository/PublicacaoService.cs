using Dapper;
using SIPROSHARED.DbContext;
using SIPROSHAREDPUBLICACAO.Service.IRepository;
using System;
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
  

                        select COUNT(distinct(prt_numero)) as PRT_PUBLICACAO_QDT 
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
 
                        select     distinct a.PRT_NUMERO
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


    }
}
