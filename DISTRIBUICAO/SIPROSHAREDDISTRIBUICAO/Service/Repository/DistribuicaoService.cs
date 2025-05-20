using Dapper;
using SIPROSHARED.DbContext;
using SIPROSHARED.Validator;
using SIPROSHAREDDISTRIBUICAO.Models;
using SIPROSHAREDDISTRIBUICAO.Service.IRepository;
using SIRPOEXCEPTIONS.ExceptionBase;
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


        public async Task<List<AssuntoQtd>> GetQtdProcessosPorAssunto(string usuario)
        {
            if (string.IsNullOrEmpty(usuario))
                throw new ErrorOnValidationException(new List<string> { "Erro de parametro. Favor entrar em contato com ADM!" });


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
            if (string.IsNullOrEmpty(usuario))
                throw new ErrorOnValidationException(new List<string> { "Erro de parametro. Favor entrar em contato com ADM!" });


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
                                DIS_DESTINO_STATUS IN ('RECEBIDO', 'JULGANDO','INSTRUCAO') 
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

            if (string.IsNullOrEmpty(usuario))
                throw new ErrorOnValidationException(new List<string> { "Erro de parametro. Favor entrar em contato com ADM!" });

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
                        INSDIS_USUARIO_DESTINO,
		                SETSUB_NOME AS SETORORIGEM,
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
                        LEFT JOIN Instrucao_Distribuicao ON (INSDIS_DIS_ID = DIS_ID  AND INSDIS_STATUS = 'RECEBIDO')
		                LEFT JOIN SetorSubXUsuario ON (DIS_DESTINO_USUARIO = SETSUBUSU_USUARIO)
                        LEFT JOIN SetorSub on (SETSUB_ID = SETSUBUSU_SETSUB_ID)
                    WHERE 
                        DIS_DESTINO_USUARIO = @usuario
        
                )
                SELECT 
                    DIS_ID, 
                    PRT_NUMERO,
                    PRT_AIT,
                    PRT_ASSUNTO,
                    PRT_USUARIO,
                    PRT_STATUS,
                    isnull(SETSUB_NOME,SETORORIGEM) as SETSUB_NOME

               FROM Prioridade LEFT JOIN SetorSubXUsuario ON (INSDIS_USUARIO_DESTINO = SETSUBUSU_USUARIO)
                            LEFT JOIN SetorSub on (SETSUB_ID = SETSUBUSU_SETSUB_ID)
                WHERE 
                    RN = 1 
                    AND PRT_STATUS IN ('RECEBIDO', 'JULGANDO','INSTRUCAO')
                ORDER BY 
                    DIS_ORIGEM_DATA DESC";

            using (var connection = _context.CreateConnection())
            {
               var parametros = new { usuario };
               var command = await connection.QueryAsync<ListaProcessoUsuario>(query, parametros);
               return command.ToList();
            }        
                      
        }

        public async Task<List<ListaProcessoUsuario>> GetProcessoSetor(string usuario)
        {
            if (string.IsNullOrEmpty(usuario))
                throw new ErrorOnValidationException(new List<string> { "Erro de parametro. Favor entrar em contato com ADM!" });


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

        public async Task<ListaProcessoUsuario> GetProcesso(int movpro_id)
        {

            if (movpro_id == 0)
                throw new ErrorOnValidationException(new List<string> { "Erro de parametro. Favor entrar em contato com ADM!" });

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

        public async Task DistribuicaoProcessoEspecifico(ProtocoloDistribuicaoModel distribuicaoModel, IDbConnection connection, IDbTransaction transaction)
        {

            //Validando 
            var validator = new ProtocoloDistribuicaoValidator();
            var result = validator.Validate(distribuicaoModel);
            if (result.IsValid == false)
                throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());


            // Adicionando parâmetros
            var dbParametro = new DynamicParameters();
            dbParametro.Add("@UsuarioOrigem", distribuicaoModel.DIS_ORIGEM_USUARIO);
            dbParametro.Add("@UsuarioDestino", distribuicaoModel.DIS_DESTINO_USUARIO);
            dbParametro.Add("@dis_mov_id", distribuicaoModel.DIS_MOV_ID);

            string query = @"   
                     DECLARE @SetorOrigem_Destino INT;    

		                 SET @SetorOrigem_Destino = (
                      SELECT TOP 1 SETSUBUSU_SETSUB_ID
                        FROM SetorSubXUsuario
                       WHERE SETSUBUSU_USUARIO = @UsuarioOrigem)
                       
                      Select
                             MOVPRO_ID
                        INTO #temp_Distribuicao
                        from Movimentacao_Processo inner join SetorSubxUsuario on (MOVPRO_SETOR_DESTINO = SETSUBUSU_SETSUB_ID AND SETSUBUSU_PERFIL = 'Presidente')
                                                   inner join SetorSub on(SETSUBUSU_SETSUB_ID = SETSUB_ID)
					                               inner join Protocolo on(MOVPRO_PRT_NUMERO = PRT_NUMERO)
                       where MOVPRO_STATUS = 'RECEBIDO'
                         and SETSUBUSU_USUARIO = @UsuarioOrigem
                         and MOVPRO_ID = @dis_mov_id

                     UPDATE Mov
                        SET MOVPRO_STATUS = 'RECEBIDO->DISTRIBUIDO'
                       FROM Movimentacao_Processo Mov inner join  #temp_Distribuicao PDis
                         ON Mov.MOVPRO_ID = PDis.MOVPRO_ID


                     insert into Protocolo_Distribuicao(
		                    DIS_MOV_ID,
		                    DIS_ORIGEM_USUARIO,
		                    DIS_ORIGEM_DATA,
		                    DIS_DESTINO_USUARIO,
		                    DIS_DESTINO_STATUS)
                     select MOVPRO_ID,
                            @UsuarioOrigem,
                            GETDATE(),
                            @UsuarioDestino,
                            'RECEBIDO'
                       from #temp_Distribuicao
            ";
           
                await connection.ExecuteAsync(query, dbParametro, transaction);
          
        }
        
        public async Task DistribuicaoProcesso(ProtocoloDistribuicaoModel distribuicaoModel, IDbConnection connection, IDbTransaction transaction)
        {
            try
            {
                if (string.IsNullOrEmpty(distribuicaoModel.DIS_ORIGEM_USUARIO) || string.IsNullOrEmpty(distribuicaoModel.DIS_DESTINO_USUARIO))
                    throw new ErrorOnValidationException(new List<string> { "Erro de parametro. Favor entrar em contato com ADM!" });

                // Adicionando parâmetros
                var dbParametro = new DynamicParameters();
                dbParametro.Add("@UsuarioOrigem", distribuicaoModel.DIS_ORIGEM_USUARIO);
                dbParametro.Add("@UsuarioDestino", distribuicaoModel.DIS_DESTINO_USUARIO);
                dbParametro.Add("@Qtd", distribuicaoModel.DIS_QTD);

                string query = @"

                     Select Top(@Qtd) 
                            MOVPRO_ID
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
	   
                        insert into Protocolo_Distribuicao(
                               DIS_MOV_ID,
                               DIS_ORIGEM_USUARIO,
                               DIS_ORIGEM_DATA,
                               DIS_DESTINO_USUARIO,
                               DIS_DESTINO_STATUS)
                        select MOVPRO_ID,
                               @UsuarioOrigem,
                               GETDATE(),
                               @UsuarioDestino,
                               'RECEBIDO'
                          from #temp_Distribuicao
	                     where MOVPRO_ID = @MOVPRO_ID
       
                         FETCH NEXT FROM cursorDistribuicao INTO  @MOVPRO_ID;
                     END

  
                   CLOSE cursorDistribuicao
                   DEALLOCATE cursorDistribuicao ";

                using (var con = _context.CreateConnection())
                {
                    await con.ExecuteAsync(query, dbParametro);
                }
            }
            catch (Exception)
            {

                throw;
            }

            

                        
        }

        public async Task RetirarProcesso(ProtocoloDistribuicaoModel distribuicaoModel)
        {

            if (string.IsNullOrEmpty(distribuicaoModel.DIS_DESTINO_USUARIO))
                throw new ErrorOnValidationException(new List<string> { "Erro de parametro. Favor entrar em contato com ADM!" });


            // Adicionando parâmetros
            var dbParametro = new DynamicParameters();
             dbParametro.Add("@UsuarioDestino", distribuicaoModel.DIS_DESTINO_USUARIO);
             dbParametro.Add("@Qtd", distribuicaoModel.DIS_QTD);

                string query = @"  

		          --Pegando os processo movimentado automaticamente
                 SELECT Top(@Qtd) 
                        DIS_ID,
                        MOVPRO_ID
                    into #temp_Retirar
                    FROM Protocolo
                    INNER JOIN Movimentacao_Processo ON (PRT_NUMERO = MOVPRO_PRT_NUMERO)
                    INNER JOIN Protocolo_Distribuicao ON (MOVPRO_ID = DIS_MOV_ID) 
                    INNER JOIN Assunto ON (PRT_ASSUNTO = ASS_ID)
                    WHERE DIS_DESTINO_USUARIO = @UsuarioDestino
                    AND DIS_DESTINO_STATUS = 'RECEBIDO'

                    Update Mov
                    set Mov.MOVPRO_STATUS = 'RECEBIDO' 
                    from Movimentacao_Processo Mov, #temp_Retirar PDis
                    where Mov.MOVPRO_ID = PDis.MOVPRO_ID 
                 
		 
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
            if (distribuicaoModel.DIS_ID == 0)
                throw new ErrorOnValidationException(new List<string> { "Erro de parametro. Favor entrar em contato com ADM!" });


            try
            { // Adicionando parâmetros
                var dbParametro = new DynamicParameters();
                dbParametro.Add("@DIS_ID", distribuicaoModel.DIS_ID);

                string query = @"                        
		            
                       Select  DIS_ID,
                               MOVPRO_ID           
                          into #temp_Retirar
                          FROM Protocolo
                         INNER JOIN Movimentacao_Processo ON (PRT_NUMERO = MOVPRO_PRT_NUMERO)
                         INNER JOIN Protocolo_Distribuicao ON (MOVPRO_ID = DIS_MOV_ID) 
                         where Dis_Id = @DIS_ID
                           and DIS_DESTINO_STATUS = 'RECEBIDO'
	  
                        Update Mov
                           set Mov.MOVPRO_STATUS = 'RECEBIDO'  
                          from Movimentacao_Processo Mov, #temp_Retirar PDis
                         where Mov.MOVPRO_ID = PDis.MOVPRO_ID 
		 
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


