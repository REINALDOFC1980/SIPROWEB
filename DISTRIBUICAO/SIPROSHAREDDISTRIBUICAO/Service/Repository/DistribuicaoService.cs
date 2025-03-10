using Dapper;
using SIPROSHARED.DbContext;
using SIPROSHAREDDISTRIBUICAO.Models;
using SIPROSHAREDDISTRIBUICAO.Service.IRepository;
using System.Data;

namespace SIPROSHAREDDISTRIBUICAO.Service.Repository
{
    public class DistribuicaoService : IDistribuicaoService
    {
        private readonly DapperContext _context;

        public DistribuicaoService(DapperContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<int> GetPresidente(string usuario)
        {
            try
            {
                var query = @"select Count(1) from SetorSubXUsuario where SETSUBUSU_USUARIO = @Usuario and SETSUBUSU_PERFIL = 'Presidente' ";

                using (var connection = _context.CreateConnection())
                {
                    var parametros = new { usuario };
                    var count = await connection.ExecuteScalarAsync<int>(query, parametros);
                    return count;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao obter dados.", ex);
            }
        }

        public async Task<List<AssuntoQtd>> GetQtdProcessosPorAssunto(string usuario)
        {
              var query = @"  
	                 
                    Declare @Setor int

				     Set @Setor = (Select top 1 SETSUBUSU_SETSUB_ID 
							        from SetorSubXUsuario 
							       where SETSUBUSU_USUARIO = @usuario and 
								  	     SETSUBUSU_PERFIL = 'Presidente')

			         Select 
					    ASS_NOME as PRT_ASSUNTO, 
					    COUNT(1) as PRT_QTD
				     from Protocolo inner join Movimentacao_Processo on (PRT_NUMERO = MOVPRO_PRT_NUMERO)
							       inner join Assunto on (PRT_ASSUNTO = ASS_ID)
				     WHERE MOVPRO_STATUS = 'RECEBIDO'
				     AND MOVPRO_SETOR_DESTINO = @Setor
				     group by ASS_NOME";

              using (var connection = _context.CreateConnection())
              {
                  var parametros = new { usuario };
                  var command = await connection.QueryAsync<AssuntoQtd>(query, parametros);
                  return command.ToList();
              }
           
        }

        public async Task<List<ProtocoloDistribuicaoModel>>GetQtdProcessosDistribuidoPorUsuario(string usuario)
        {
           
                var query = @"

                       -- Declara a variável @Setor
                        DECLARE @Setor INT;

                        SET @Setor = (
                            SELECT TOP 1 SETSUBUSU_SETSUB_ID
                            FROM SetorSubXUsuario
                            WHERE 
                                SETSUBUSU_USUARIO = @usuario 
                                AND SETSUBUSU_PERFIL = 'Presidente'
                        );

                        WITH UltimoStatus AS (
                            SELECT 
                                DIS_ID,
                                PRT_NUMERO,
                                DIS_DESTINO_USUARIO,
                                DIS_DESTINO_STATUS,
                                ROW_NUMBER() OVER (
                                    PARTITION BY PRT_NUMERO
                                    ORDER BY DIS_ID DESC
                                ) AS RN
                            FROM 
                                Protocolo
                            INNER JOIN Movimentacao_Processo ON PRT_NUMERO = MOVPRO_PRT_NUMERO
                            INNER JOIN Protocolo_Distribuicao ON MOVPRO_ID = DIS_MOV_ID
                            WHERE
                                DIS_DESTINO_STATUS IN ('RECEBIDO', 'JULGANDO') 
                        )

                        , Filtrado AS (
                            SELECT 
                                PRT_NUMERO,
                                DIS_DESTINO_USUARIO,
                                DIS_DESTINO_STATUS
                            FROM 
                                UltimoStatus
                            WHERE 
                                RN = 1 
                        )



                        -- Consulta final para agrupar os dados
                        SELECT 
                            UPPER(SETSUBUSU_NOMECOMPLETO) AS DIS_NOME_USUARIO,
                            SETSUBUSU_USUARIO AS DIS_DESTINO_USUARIO,
                            SETSUBUSU_PERFIL AS DIS_PERFIL,
                            COUNT(Filtrado.PRT_NUMERO) AS DIS_QTD,
                            CAST(ROUND(COUNT(Filtrado.PRT_NUMERO) * 100.0 / NULLIF(SUM(COUNT(Filtrado.PRT_NUMERO)) OVER (), 0), 0) AS INT) AS DIS_PERCENTUAL
                        FROM 
                            SetorSubXUsuario 
                        LEFT JOIN 
                            Filtrado ON SETSUBUSU_USUARIO = Filtrado.DIS_DESTINO_USUARIO
                        WHERE 
                            SETSUBUSU_SETSUB_ID = @Setor
                        GROUP BY 
                            SETSUBUSU_NOMECOMPLETO, 
                            SETSUBUSU_PERFIL, 
                            SETSUBUSU_USUARIO
                        ORDER BY 
                            SETSUBUSU_PERFIL DESC;
                         ";

                using (var connection = _context.CreateConnection())
                {
                    var parametros = new { usuario };
                    var command = await connection.QueryAsync<ProtocoloDistribuicaoModel>(query, parametros);
                    return command.ToList();
                }
           
        }

        public async Task<List<ListaProcessoUsuario>> GetProcessosUsuario(string usuario)
        {
        
            var query = @"
				
                WITH Prioridade AS (
                   SELECT 
                       DIS_ID,
                       PRT_NUMERO,
                       PRT_AIT,
                       ASS_NOME AS PRT_ASSUNTO,
                       DIS_DESTINO_USUARIO AS PRT_USUARIO,
                       DIS_DESTINO_STATUS AS PRT_STATUS,
		               DIS_ORIGEM_DATA,
                       ROW_NUMBER() OVER (
                           PARTITION BY PRT_NUMERO 
                           ORDER BY 
                               DIS_ORIGEM_DATA DESC
                       ) AS RN
                   FROM 
                       Protocolo
                       INNER JOIN Movimentacao_Processo ON PRT_NUMERO = MOVPRO_PRT_NUMERO
                       INNER JOIN Protocolo_Distribuicao ON MOVPRO_ID = DIS_MOV_ID
                       INNER JOIN Assunto ON PRT_ASSUNTO = ASS_ID
                   WHERE 
                       DIS_DESTINO_USUARIO = @usuario
                      
               )
               SELECT 
                   DIS_ID, 
                   PRT_NUMERO,
                   PRT_AIT,
                   PRT_ASSUNTO,
                   PRT_USUARIO,
                   PRT_STATUS
               FROM 
                   Prioridade
               WHERE 
                   RN = 1 
                   AND PRT_STATUS IN ('RECEBIDO', 'JULGANDO','INSTRUCAO')
               ORDER BY 
                   DIS_ORIGEM_DATA DESC;";

            using (var connection = _context.CreateConnection())
            {
               var parametros = new { usuario };
               var command = await connection.QueryAsync<ListaProcessoUsuario>(query, parametros);
               return command.ToList();
            }        
                      
        }

        public async Task<List<ListaProcessoUsuario>> GetProcessoSetor(string usuario)
        {
            try
            {
                var query = @"
					    select MOVPRO_ID,
	                           PRT_NUMERO, 
	                           PRT_AIT, 
		                       ASS_NOME AS PRT_ASSUNTO, 
		                       CONVERT(VARCHAR(10),PRT_DT_CADASTRO,103) AS PRT_DT_CADASTRO
	            from Protocolo inner join Movimentacao_Processo on (PRT_NUMERO = MOVPRO_PRT_NUMERO)
				               inner join Assunto on (PRT_ASSUNTO = ASS_ID)
				               inner join SetorSubXUsuario on(SETSUBUSU_SETSUB_ID = MOVPRO_SETOR_DESTINO)
                         WHERE MOVPRO_STATUS = 'RECEBIDO'
                           AND SETSUBUSU_PERFIL = 'Presidente'
                           AND SETSUBUSU_USUARIO = @usuario";

                using (var connection = _context.CreateConnection())
                {
                    var parametros = new { usuario };
                    var command = await connection.QueryAsync<ListaProcessoUsuario>(query, parametros);
                    return command.ToList();
                }
            }
            catch (Exception ex)
            {
                // Melhor manipulação de exceções: log ou tratar adequadamente
                throw new Exception("Erro ao obter dados.", ex);
            }
        }

        public async Task<ListaProcessoUsuario> GetProcesso(int movpro_id)
        {
            try
            {
                var query = @"
                             SELECT MOVPRO_ID,
                                    PRT_NUMERO, 
                                    PRT_AIT, 
                                    ASS_NOME AS PRT_ASSUNTO, 
                                    CONVERT(VARCHAR(10), PRT_DT_CADASTRO, 103) AS PRT_DT_ABERTURA
                            FROM Protocolo 
                            INNER JOIN Movimentacao_Processo ON PRT_NUMERO = MOVPRO_PRT_NUMERO
                            INNER JOIN Assunto ON PRT_ASSUNTO = ASS_ID
                            INNER JOIN SetorSubXUsuario ON SETSUBUSU_SETSUB_ID = MOVPRO_SETOR_DESTINO
                            WHERE MOVPRO_STATUS = 'RECEBIDO'
                              AND SETSUBUSU_PERFIL = 'Presidente'
                              AND MOVPRO_ID = @movpro_id";

                using (var connection = _context.CreateConnection())
                {
                    var parametros = new { movpro_id };
                    // Aqui utilizamos QueryFirstOrDefaultAsync para mapear a linha retornada diretamente para o modelo ListaProcessoUsuario
                    var processo = await connection.QueryFirstOrDefaultAsync<ListaProcessoUsuario>(query, parametros);
                    return processo;
                }
            }
            catch (Exception ex)
            {
                // Melhor manipulação de exceções: log ou tratar adequadamente
                throw new Exception("Erro ao obter dados.", ex);
            }
        }

        public async Task DistribuicaoProcessoEspecifico(ProtocoloDistribuicaoModel distribuicaoModel, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                // Adicionando parâmetros
                var dbParametro = new DynamicParameters();
                dbParametro.Add("@UsuarioOrigem", distribuicaoModel.DIS_ORIGEM_USUARIO);
                dbParametro.Add("@UsuarioDestino", distribuicaoModel.DIS_DESTINO_USUARIO);
                dbParametro.Add("@dis_mov_id", distribuicaoModel.DIS_MOV_ID);
                dbParametro.Add("@MOVPRO_PARECER_ORIGEM", distribuicaoModel.MOVPRO_PARECER_ORIGEM);

                string query = @"                        
                       Select
                              MOVPRO_ID,
                              MOVPRO_PRT_NUMERO,
                              SETSUBUSU_SETSUB_ID,--sempre vai ser origem e destino
                              SETSUBUSU_USUARIO,
                              MOVPRO_INSTRUCAO,
		                      PRT_ACAO
                              INTO #temp_Distribuicao
                         from Movimentacao_Processo inner join SetorSubxUsuario on (MOVPRO_SETOR_DESTINO = SETSUBUSU_SETSUB_ID AND SETSUBUSU_PERFIL = 'Presidente')
	                                                inner join SetorSub on(SETSUBUSU_SETSUB_ID = SETSUB_ID)
								                    inner join Protocolo on(MOVPRO_PRT_NUMERO = PRT_NUMERO)
                        where MOVPRO_STATUS = 'RECEBIDO'
                        and SETSUBUSU_USUARIO = @UsuarioOrigem
                        and MOVPRO_ID = @dis_mov_id

                             UPDATE Mov
                          SET MOVPRO_STATUS = 'RECEBIDO->DISTRIBUIDO'
                         FROM Movimentacao_Processo Mov
                              INNER JOIN #temp_Distribuicao PDis
                              ON Mov.MOVPRO_ID = PDis.MOVPRO_ID

                 
                       Insert into Movimentacao_Processo
                       (
                               MOVPRO_PRT_NUMERO    
                              ,MOVPRO_SETOR_ORIGEM 
                              ,MOVPRO_USUARIO_ORIGEM                                                                                
                              ,MOVPRO_DATA_ORIGEM      
                              ,MOVPRO_PARECER_ORIGEM                                                                                                                                                                                                                                            
                              ,MOVPRO_ACAO_ORIGEM                                                                                                                                                                                                                                               
                              ,MOVPRO_SETOR_DESTINO 
                              ,MOVPRO_STATUS                                      
                              ,MOVPRO_PRTDOC_ID 
                         )

                        SELECT MOVPRO_PRT_NUMERO
                              ,SETSUBUSU_SETSUB_ID 
                              ,SETSUBUSU_USUARIO     
                              ,GETDATE()      
                              ,Null 
                              ,'Processo distribuido para o relator do setor'                          
                              ,SETSUBUSU_SETSUB_ID 
                              ,'DISTRIBUIDO'
                              ,Null 
                         FROM #temp_Distribuicao 
                        WHERE MOVPRO_INSTRUCAO is null
     
                    Declare  @UltimoID int
                         SET @UltimoID = SCOPE_IDENTITY();

                       insert into Protocolo_Distribuicao
                       select Case when MOVPRO_INSTRUCAO is null then @UltimoID else MOVPRO_ID end,
                              @UsuarioOrigem,
                              GETDATE(),
                              @UsuarioDestino AS DIS_DESTINO_USUARIO,
                              0 as DIS_DESTINO_STA_ID,
                              'RECEBIDO' AS DIS_DESTINO_STATUS,
                              0 AS DIS_NUMJULGADOS,
                              CASE WHEN PRT_ACAO = 'RETIFICAR VOTO' THEN 1 ELSE 0 END AS DIS_RETORNO,
                              NULL DIS_DATA_JULGAMENTO
                         from #temp_Distribuicao



                ";

           
                    await connection.ExecuteAsync(query, dbParametro, transaction);
               
            }
            catch (Exception ex)
            {
                // É recomendável registrar o erro ou retornar uma mensagem específica.
                throw new Exception("Erro ao distribuir o processo", ex);
            }
        }
        
        public async Task DistribuicaoProcesso(ProtocoloDistribuicaoModel distribuicaoModel, IDbConnection connection, IDbTransaction transaction)
        {
            
                // Adicionando parâmetros
                var dbParametro = new DynamicParameters();
                dbParametro.Add("@UsuarioOrigem", distribuicaoModel.DIS_ORIGEM_USUARIO);
                dbParametro.Add("@UsuarioDestino", distribuicaoModel.DIS_DESTINO_USUARIO);
                dbParametro.Add("@Qtd", distribuicaoModel.DIS_QTD);

                string query = @"

                      Select Top(@Qtd) 
                             MOVPRO_ID,
							 MOVPRO_PRT_NUMERO,
							 SETSUBUSU_SETSUB_ID,--sempre vai ser origem e destino
							 SETSUBUSU_USUARIO,
							 MOVPRO_INSTRUCAO,
							 PRT_ACAO
                        INTO #temp_Distribuicao
						From Movimentacao_Processo inner join SetorSubxUsuario on (MOVPRO_SETOR_DESTINO = SETSUBUSU_SETSUB_ID AND SETSUBUSU_PERFIL = 'Presidente')
						   						   inner join SetorSub on(SETSUBUSU_SETSUB_ID = SETSUB_ID)
														inner join Protocolo on(MOVPRO_PRT_NUMERO = PRT_NUMERO)
					  where MOVPRO_STATUS = 'RECEBIDO'
					    and SETSUBUSU_USUARIO = @UsuarioOrigem                          
                        
						
						--Cursor--
                        DECLARE @MOVPRO_ID int
                        DECLARE cursorDistribuicao CURSOR FOR
     
                        SELECT MOVPRO_ID 
                          FROM #temp_Distribuicao 

                        OPEN cursorDistribuicao;

                        FETCH NEXT FROM cursorDistribuicao INTO @MOVPRO_ID;

                        WHILE @@FETCH_STATUS = 0
                        BEGIN

                           UPDATE Mov
                              SET MOVPRO_STATUS = 'RECEBIDO->DISTRIBUIDO'
                             FROM Movimentacao_Processo Mov
	                        where Mov.MOVPRO_ID = @MOVPRO_ID

                 
                           Insert into Movimentacao_Processo
                           (
                                   MOVPRO_PRT_NUMERO    
                                  ,MOVPRO_SETOR_ORIGEM 
                                  ,MOVPRO_USUARIO_ORIGEM                                                                                
                                  ,MOVPRO_DATA_ORIGEM      
                                  ,MOVPRO_PARECER_ORIGEM                                                                                                                                                                                                                                            
                                  ,MOVPRO_ACAO_ORIGEM                                                                                                                                                                                                                                               
                                  ,MOVPRO_SETOR_DESTINO 
                                  ,MOVPRO_STATUS                                      
                                  ,MOVPRO_PRTDOC_ID 
                             )

                            SELECT MOVPRO_PRT_NUMERO
                                   ,SETSUBUSU_SETSUB_ID 
								   ,SETSUBUSU_USUARIO     
								   ,GETDATE()      
								   ,Null 
								   ,'Processo distribuido para o relator do setor'                          
								   ,SETSUBUSU_SETSUB_ID 
								   ,'DISTRIBUIDO'
								   ,Null 
                             FROM #temp_Distribuicao 
                            WHERE MOVPRO_INSTRUCAO is null
	                          and MOVPRO_ID = @MOVPRO_ID
     
                        Declare  @UltimoID int
                             SET @UltimoID = SCOPE_IDENTITY();

                           insert into Protocolo_Distribuicao
                           Select Case when MOVPRO_INSTRUCAO is null then @UltimoID else MOVPRO_ID end,
                                  @UsuarioOrigem,
                                  GETDATE(),
                                  @UsuarioDestino AS DIS_DESTINO_USUARIO,
                                  0 as DIS_DESTINO_STA_ID,
                                  'RECEBIDO' AS DIS_DESTINO_STATUS,
                                  0 AS DIS_NUMJULGADOS,
                                  CASE WHEN PRT_ACAO = 'RETIFICAR VOTO' THEN 1 ELSE 0 END AS DIS_RETORNO,
                                  NULL DIS_DATA_JULGAMENTO
                             from #temp_Distribuicao
	                         where MOVPRO_ID = @MOVPRO_ID
       
	                         FETCH NEXT FROM cursorDistribuicao INTO  @MOVPRO_ID;
                        END

  
                        CLOSE cursorDistribuicao;
                        DEALLOCATE cursorDistribuicao;
   
                ";

                await connection.ExecuteAsync(query, dbParametro, transaction);
                
           
        }

        public async Task RetirarProcesso(ProtocoloDistribuicaoModel distribuicaoModel)
        {



           
             // Adicionando parâmetros
             var dbParametro = new DynamicParameters();
             dbParametro.Add("@UsuarioDestino", distribuicaoModel.DIS_DESTINO_USUARIO);
             dbParametro.Add("@Qtd", distribuicaoModel.DIS_QTD);

             string query = @"  
                   
		          --Pegando os processo movimentado automaticamente
                 SELECT Top(@Qtd) 
                        PRT_NUMERO,
                        MOVPRO_SETOR_DESTINO, --SEMPRE VAI SER ORIGEM E DESTINO
                        DIS_ORIGEM_USUARIO,           
                        DIS_ID,
                        MOVPRO_ID,
                        MOVPRO_INSTRUCAO,
                        DIS_RETORNO
                    into #temp_Retirar
                    FROM Protocolo
                    INNER JOIN Movimentacao_Processo ON (PRT_NUMERO = MOVPRO_PRT_NUMERO)
                    INNER JOIN Protocolo_Distribuicao ON (MOVPRO_ID = DIS_MOV_ID) 
                    INNER JOIN Assunto ON (PRT_ASSUNTO = ASS_ID)
                    WHERE DIS_DESTINO_USUARIO = @UsuarioDestino
                    AND DIS_DESTINO_STATUS = 'RECEBIDO'
   

                       INSERT INTO Movimentacao_Processo
                          (
                          MOVPRO_PRT_NUMERO    
                         ,MOVPRO_SETOR_ORIGEM 
                         ,MOVPRO_USUARIO_ORIGEM                                                                                
                         ,MOVPRO_DATA_ORIGEM      
                         ,MOVPRO_PARECER_ORIGEM                                                                                                                                                                                                              
                         ,MOVPRO_ACAO_ORIGEM                                                                                                                                                                                                                                               
                         ,MOVPRO_SETOR_DESTINO 
                         ,MOVPRO_STATUS                                      
                         ,MOVPRO_PRTDOC_ID 
	
                         )
                        select  PRT_NUMERO           
                                ,MOVPRO_SETOR_DESTINO 
                                ,DIS_ORIGEM_USUARIO        
                                ,GETDATE()      
                                ,null 
                                ,'Processo retirado do relator e encaminhado ao setor responsável.'                                 
                                ,MOVPRO_SETOR_DESTINO 
                                ,'RECEBIDO' 
                                ,NULL
                          from #temp_Retirar	
                         WHERE MOVPRO_INSTRUCAO is null
  
                        Update Mov
                           set Mov.MOVPRO_STATUS = case when Mov.MOVPRO_INSTRUCAO is not null then 'RECEBIDO'  else 'DISTRIBUIDO->RETIRADO' end
                          from Movimentacao_Processo Mov, #temp_Retirar PDis
                         where Mov.MOVPRO_ID = PDis.MOVPRO_ID 
                           and isnull(DIS_RETORNO,0)=0
                  
		 
                     -- Excluir da tabela Protocolo_Distribuicao
                     DELETE pd
                       FROM Protocolo_Distribuicao pd
                      INNER JOIN #temp_Retirar tb ON pd.DIS_ID = tb.DIS_ID    ";

             using (var con = _context.CreateConnection())
             {
                 await con.ExecuteAsync(query, dbParametro);
             }
           
        }

        public async Task RetirarProcessoEspecifico(ProtocoloDistribuicaoModel distribuicaoModel)
        {

            try
            { // Adicionando parâmetros
                var dbParametro = new DynamicParameters();
                dbParametro.Add("@DIS_ID", distribuicaoModel.DIS_ID);

                string query = @"                        
		            
                   Select   PRT_NUMERO,
                            MOVPRO_SETOR_DESTINO, --SEMPRE VAI SER ORIGEM E DESTINO
                            DIS_ORIGEM_USUARIO,   
                            DIS_ID,
                            MOVPRO_ID,
                            DIS_RETORNO,
                            MOVPRO_INSTRUCAO
                      into #temp_Retirar
                      FROM Protocolo
                     INNER JOIN Movimentacao_Processo ON (PRT_NUMERO = MOVPRO_PRT_NUMERO)
                     INNER JOIN Protocolo_Distribuicao ON (MOVPRO_ID = DIS_MOV_ID) 
                     INNER JOIN Assunto ON (PRT_ASSUNTO = ASS_ID)
                 where Dis_Id = @DIS_ID
                 and  DIS_DESTINO_STATUS = 'RECEBIDO'
	
	

                      INSERT INTO Movimentacao_Processo
                          (
                          MOVPRO_PRT_NUMERO    
                         ,MOVPRO_SETOR_ORIGEM 
                         ,MOVPRO_USUARIO_ORIGEM                                                                                
                         ,MOVPRO_DATA_ORIGEM      
                         ,MOVPRO_PARECER_ORIGEM                                                                                                                                                                                                              
                         ,MOVPRO_ACAO_ORIGEM                                                                                                                                                                                                                                               
                         ,MOVPRO_SETOR_DESTINO 
                         ,MOVPRO_STATUS                                      
                         ,MOVPRO_PRTDOC_ID 
	
                         )
                        select  PRT_NUMERO           
                                ,MOVPRO_SETOR_DESTINO 
                                ,DIS_ORIGEM_USUARIO        
                                ,GETDATE()      
                                ,null 
                                ,'Processo retirado do relator e encaminhado ao setor responsável.'                                 
                                ,MOVPRO_SETOR_DESTINO 
                                ,'RECEBIDO' 
                                ,NULL
                          from #temp_Retirar	
                         WHERE MOVPRO_INSTRUCAO is null
      
  
  
                        Update Mov
                           set Mov.MOVPRO_STATUS = case when Mov.MOVPRO_INSTRUCAO is not null then 'RECEBIDO'  else 'DISTRIBUIDO->RETIRADO' end
                          from Movimentacao_Processo Mov, #temp_Retirar PDis
                         where Mov.MOVPRO_ID = PDis.MOVPRO_ID 
                           and isnull(DIS_RETORNO,0)=0
		 
                  -- Excluir da tabela Protocolo_Distribuicao
                  DELETE pd
                      FROM Protocolo_Distribuicao pd
                      INNER JOIN #temp_Retirar tb ON pd.DIS_ID = tb.DIS_ID";
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


