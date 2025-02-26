using Dapper;
using SIPROSHARED.DbContext;
using SIPROSHAREDINSTRUCAO.Models;
using SIPROSHAREDINSTRUCAO.Service.IRepository;
using System.Data;

namespace SIPROSHAREDINSTRUCAO.Service.Repository
{
    public class InstrucaoDistribuicaoService : IInstrucaoDistribuicaoService
    {
        private readonly DapperContext _context;

        public InstrucaoDistribuicaoService(DapperContext context)
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
                                  INSDIS_ID,
                                  PRT_NUMERO,
                                  INSDIS_USUARIO_DESTINO,
                                  INSDIS_STATUS,
                                  ROW_NUMBER() OVER (
                                      PARTITION BY PRT_NUMERO
                                      ORDER BY INSDIS_ID DESC
                                  ) AS RN
                              FROM 
                                  Protocolo
                              INNER JOIN Movimentacao_Processo ON PRT_NUMERO = MOVPRO_PRT_NUMERO
                              INNER JOIN Instrucao_Distribuicao ON MOVPRO_ID = INSDIS_MOV_ID
                              WHERE
                              INSDIS_STATUS IN ('RECEBIDO') 
                          )

                          , Filtrado AS (
                              SELECT 
                                  PRT_NUMERO,
                                  INSDIS_USUARIO_DESTINO,
                                  INSDIS_STATUS
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
                              Filtrado ON SETSUBUSU_USUARIO = Filtrado.INSDIS_USUARIO_DESTINO
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
				
                         WITH PEOCESSOS AS (
                                        SELECT 
                                            INSDIS_ID as DIS_ID,
                                            PRT_NUMERO,
                                            PRT_AIT,
                                            ASS_NOME AS PRT_ASSUNTO,
                                            INSDIS_USUARIO_DESTINO AS PRT_USUARIO,
                                            INSDIS_STATUS AS PRT_STATUS,
                                            INSDIS_DATA_ORIGEM,
                                            ROW_NUMBER() OVER (
                                                PARTITION BY PRT_NUMERO 
                                                ORDER BY 
                                                    INSDIS_DATA_ORIGEM DESC
                                            ) AS RN
                                        FROM 
                                            Protocolo
                                            INNER JOIN Movimentacao_Processo ON PRT_NUMERO = MOVPRO_PRT_NUMERO
                                            INNER JOIN Instrucao_Distribuicao ON MOVPRO_ID = INSDIS_MOV_ID
                                            INNER JOIN Assunto ON PRT_ASSUNTO = ASS_ID
                                        WHERE 
                                            INSDIS_USUARIO_DESTINO = @usuario
       
                                    )
                                    SELECT 
                                        DIS_ID, 
                                        PRT_NUMERO,
                                        PRT_AIT,
                                        PRT_ASSUNTO,
                                        PRT_USUARIO,
                                        PRT_STATUS
                                    FROM 
                                        PEOCESSOS
                                    WHERE 
                                        RN = 1 
                                        AND PRT_STATUS IN ('RECEBIDO')
                                    ORDER BY 
                                        INSDIS_DATA_ORIGEM DESC;";

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

                     DECLARE @SetorOrigem_Destino INT;        
		                 SET @SetorOrigem_Destino = (
                      SELECT TOP 1 SETSUBUSU_SETSUB_ID
                        FROM SetorSubXUsuario
                       WHERE SETSUBUSU_USUARIO = @UsuarioOrigem)


                      Select MOVPRO_ID,
	                         MOVPRO_PRT_NUMERO,
			                 MOVPRO_SETOR_DESTINO as SETOR_ORIGEM
	                    INTO #temp_Distribuicao
	                    from Movimentacao_Processo  
                  inner join SetorSubxUsuario on (MOVPRO_SETOR_DESTINO = SETSUBUSU_SETSUB_ID AND SETSUBUSU_PERFIL = 'Presidente')
                  inner join SetorSub on(SETSUBUSU_SETSUB_ID = SETSUB_ID)
                         and SETSUBUSU_USUARIO = @UsuarioOrigem
                         and MOVPRO_ID = @dis_mov_id
		                 and MOVPRO_STATUS = 'RECEBIDO'


	                  UPDATE Mov
                         SET MOVPRO_STATUS = 'RECEBIDO->DISTRIBUIDO'
                        FROM Movimentacao_Processo Mov
                  inner join #temp_Distribuicao PDis on Mov.MOVPRO_ID = PDis.MOVPRO_ID
	  
	  
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
                             ,@SetorOrigem_Destino 
                             ,@UsuarioOrigem     
                             ,GETDATE()     
                             ,@MOVPRO_PARECER_ORIGEM 
                             ,'Processo distribuido para um responsável para instrução'                    
                             ,@SetorOrigem_Destino --Sempre a origem vai ser odestino pq a distribuicao é local
                             ,'DISTRIBUIDO' 
                             ,null 
                        FROM #temp_Distribuicao 

                     Declare @UltimoID int,
			                 @Dis_Id int

                         SET @UltimoID = SCOPE_IDENTITY();

		                 SET @Dis_Id = (Select top 1 DIS_ID FROM Protocolo_Distribuicao PD  
			      inner join Movimentacao_Processo MP on(DIS_MOV_ID = MOVPRO_ID  )
		          inner join #temp_Distribuicao TD on (MP.MOVPRO_PRT_NUMERO = TD.MOVPRO_PRT_NUMERO)
	                   where DIS_DESTINO_STATUS = 'INSTRUCAO' 
	                order by DIS_ORIGEM_DATA desc)


                insert into Instrucao_Distribuicao
			                (INSDIS_MOV_ID 
			                ,INSDIS_DIS_ID 
			                ,INSDIS_USUARIO_ORIGEM 
			                ,INSDIS_DATA_ORIGEM     
			                ,INSDIS_USUARIO_DESTINO 
			                ,INSDIS_OBSERVACAO      
			                ,INSDIS_STATUS)
                      select @UltimoID,
			                 @Dis_Id,
                             @UsuarioOrigem,
                             GETDATE(),
                             @UsuarioDestino,
                             '' as Observaca,
                             'RECEBIDO' AS DIS_DESTINO_STATUS
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
            try
            {
                var dbParametro = new DynamicParameters();
                dbParametro.Add("@UsuarioOrigem", distribuicaoModel.DIS_ORIGEM_USUARIO);
                dbParametro.Add("@UsuarioDestino", distribuicaoModel.DIS_DESTINO_USUARIO);
                dbParametro.Add("@Qtd", distribuicaoModel.DIS_QTD);

                string query = @"                     
                    DECLARE @SetorOrigem INT;

                    SET @SetorOrigem = (
                        SELECT TOP 1 SETSUBUSU_SETSUB_ID
                        FROM SetorSubXUsuario
                        WHERE SETSUBUSU_USUARIO = @UsuarioOrigem
                    );

                    SELECT TOP (@Qtd) 
                        MOVPRO_ID,
                        MOVPRO_PRT_NUMERO,
                        MOVPRO_SETOR_DESTINO AS SETOR_ORIGEM
                    INTO #temp_Distribuicao
                    from Movimentacao_Processo inner join SetorSubxUsuario on (MOVPRO_SETOR_DESTINO = SETSUBUSU_SETSUB_ID AND SETSUBUSU_PERFIL = 'Presidente')
					                           inner join SetorSub on(SETSUBUSU_SETSUB_ID = SETSUB_ID)
                    where MOVPRO_STATUS = 'RECEBIDO'
                    and SETSUBUSU_USUARIO = @UsuarioOrigem

                    -- Declaração do cursor
		            DECLARE @MOVPRO_ID int,
                            @MOVPRO_PRT_NUMERO VARCHAR(20)

                    DECLARE cursorDistribuicao CURSOR FOR
     
                    SELECT MOVPRO_ID,MOVPRO_PRT_NUMERO 
                        FROM #temp_Distribuicao 

                    OPEN cursorDistribuicao;

                    FETCH NEXT FROM cursorDistribuicao INTO @MOVPRO_ID,@MOVPRO_PRT_NUMERO;
           

                    WHILE @@FETCH_STATUS = 0
                    BEGIN
                        -- Atualiza o status no Movimentacao_Processo
                        UPDATE Mov
                           SET MOVPRO_STATUS = 'RECEBIDO->DISTRIBUIDO'
                          FROM Movimentacao_Processo Mov
	                     WHERE Mov.MOVPRO_ID = @MOVPRO_ID

                        DECLARE @Dis_Id int

                           SET @Dis_Id = ISNULL((
                                        SELECT TOP 1 DIS_ID 
                                         FROM Protocolo_Distribuicao PD  
                                   INNER JOIN Movimentacao_Processo MP ON PD.DIS_MOV_ID = MP.MOVPRO_ID
                                   INNER JOIN #temp_Distribuicao TD ON MP.MOVPRO_PRT_NUMERO = TD.MOVPRO_PRT_NUMERO
                            WHERE DIS_DESTINO_STATUS = 'INSTRUCAO' 
	                         AND MP.MOVPRO_PRT_NUMERO = @MOVPRO_PRT_NUMERO
                            ORDER BY DIS_ORIGEM_DATA DESC
                        ), 0);


                        INSERT INTO Instrucao_Distribuicao
                        (
                            INSDIS_MOV_ID, 
                            INSDIS_DIS_ID, 
                            INSDIS_USUARIO_ORIGEM, 
                            INSDIS_DATA_ORIGEM,     
                            INSDIS_USUARIO_DESTINO, 
                            INSDIS_OBSERVACAO,      
                            INSDIS_STATUS
                        )
                        SELECT 
                            @MOVPRO_ID,
                            @Dis_Id,
                            @UsuarioOrigem,
                            GETDATE(),
                            @UsuarioDestino,
                            '' AS Observacao,
                            'RECEBIDO' AS DIS_DESTINO_STATUS
                        FROM #temp_Distribuicao
		                where MOVPRO_ID = @MOVPRO_ID

                        FETCH NEXT FROM cursorDistribuicao INTO @MOVPRO_ID,@MOVPRO_PRT_NUMERO;
                    END;


                    CLOSE cursorDistribuicao;
                    DEALLOCATE cursorDistribuicao;
   
                ";

                await connection.ExecuteAsync(query, dbParametro, transaction);

            }
            catch (Exception ex)
            {

                throw;
            }
                // Adicionando parâmetros
              
           
        }

        public async Task RetirarProcesso(ProtocoloDistribuicaoModel distribuicaoModel)
        {
            try
            {

                // Adicionando parâmetros
                var dbParametro = new DynamicParameters();
                dbParametro.Add("@UsuarioDestino", distribuicaoModel.DIS_DESTINO_USUARIO);
                dbParametro.Add("@Qtd", distribuicaoModel.DIS_QTD);

                string query = @"  
                   
		      --Pegando os processo movimentado automaticamente
 
	 
	           SELECT Top(@Qtd) 
                      MOVPRO_PRT_NUMERO,
                      INSDIS_ID,
                      MOVPRO_ID,
                      MOVPRO_USUARIO_ORIGEM,
                      MOVPRO_SETOR_ORIGEM
                 INTO #temp_Retirar
                 FROM Movimentacao_Processo 
           INNER JOIN Instrucao_Distribuicao ON (INSDIS_MOV_ID = MOVPRO_ID) 
                WHERE INSDIS_USUARIO_DESTINO = @UsuarioDestino
                  AND INSDIS_STATUS = 'RECEBIDO'   

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
              SELECT  MOVPRO_PRT_NUMERO           
                     ,MOVPRO_SETOR_ORIGEM 
                     ,MOVPRO_USUARIO_ORIGEM        
                     ,GETDATE()      
                     ,null 
                     ,'Processo retirado do membro e encaminhado ao setor responsável.'                                 
                     ,MOVPRO_SETOR_ORIGEM 
                     ,'RECEBIDO' 
                     ,NULL
                FROM #temp_Retirar	
  
               UPDATE Mov
                  SET Mov.MOVPRO_STATUS ='DISTRIBUIDO->RETIRADO'
                 FROM Movimentacao_Processo Mov, #temp_Retirar PDis
                WHERE Mov.MOVPRO_ID = PDis.MOVPRO_ID 
		 
           -- Excluir da tabela Protocolo_Distribuicao
               DELETE pd
                 FROM Instrucao_Distribuicao pd
           INNER JOIN #temp_Retirar tb ON pd.INSDIS_ID = tb.INSDIS_ID ";

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

        public async Task RetirarProcessoEspecifico(ProtocoloDistribuicaoModel distribuicaoModel)
        {
                // Adicionando parâmetros
                var dbParametro = new DynamicParameters();
                dbParametro.Add("@DIS_ID", distribuicaoModel.DIS_ID);

                string query = @"                        
		            
                             SELECT MOVPRO_PRT_NUMERO,
                                    INSDIS_ID,
                                    MOVPRO_ID,
                                    MOVPRO_USUARIO_ORIGEM,
                                    MOVPRO_SETOR_ORIGEM
                                INTO #temp_Retirar
                                FROM Movimentacao_Processo 
                        INNER JOIN Instrucao_Distribuicao ON (INSDIS_MOV_ID = MOVPRO_ID) 
                            WHERE INSDIS_ID = @DIS_ID
                                and  INSDIS_STATUS = 'RECEBIDO'


                        INSERT INTO Movimentacao_Processo
                                ( MOVPRO_PRT_NUMERO    
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
                                ,MOVPRO_SETOR_ORIGEM 
                                ,MOVPRO_USUARIO_ORIGEM        
                                ,GETDATE()      
                                ,NULL 
                                ,'Processo retirado do membro e encaminhado ao setor responsável.'                                 
                                ,MOVPRO_SETOR_ORIGEM 
                                ,'RECEBIDO' 
                                ,NULL
                            FROM #temp_Retirar	
 
                            UPDATE Mov
                            SET Mov.MOVPRO_STATUS ='DISTRIBUIDO->RETIRADO'
                            FROM Movimentacao_Processo Mov, #temp_Retirar PDis
                            WHERE Mov.MOVPRO_ID = PDis.MOVPRO_ID 
		 
                        DELETE pd
                            FROM Instrucao_Distribuicao pd
                    INNER JOIN #temp_Retirar tb ON pd.INSDIS_ID = tb.INSDIS_ID";

                using (var con = _context.CreateConnection())
                {
                    await con.ExecuteAsync(query, dbParametro);
                }
           
        }





    }
}


