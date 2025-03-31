using Dapper;
using FluentValidation;
using SIPROSHARED.DbContext;
using SIPROSHARED.Models;
using SIPROSHARED.Validator;
using SIPROSHAREDHOMOLOGACAO.Models;
using SIPROSHAREDHOMOLOGACAO.Service.IRepository;
using SIPROSHAREDHOMOLOGACAO.Validator;
using SIRPOEXCEPTIONS.ExceptionBase;
using System.Data;
using System.Data.Common;

namespace SIPROSHAREDHOMOLOGACAO.Service.Repository
{
    public class HomologacaoService : IHomologacaoService
    {

        private readonly DapperContext _context;

        public HomologacaoService(DapperContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<HomologacaoModel>> LocalizarHomolgacao(int setor, string resultado)
        {
            //validação
            if (setor == 0)
            {
                throw new ErrorOnValidationException(new List<string> { "Setor está vazio!" });
            }

            var query = @"
				 select Movpro_id,
	                    SETSUB_NOME,
                        PRT_NUMERO,
                        PRT_AIT,
                        ASS_NOME as PRT_NOME_ASSUNTO,
                        dbo.GetResultadoDefesa( PRT_RESULTADO ) as PRT_RESULTADO
                   From Protocolo inner join Movimentacao_Processo on (prt_numero = MOVPRO_PRT_NUMERO)
                                  inner join Assunto on (PRT_ASSUNTO = ASS_ID)  
					              inner join SetorSub on(SETSUB_ID = MOVPRO_SETOR_ORIGEM)
		          Where MOVPRO_SETOR_ORIGEM = @setor
		            and MOVPRO_STATUS = 'HOMOLOGAR'
		            and PRT_RESULTADO like Case when @resultado = 'Todos' then '%' else @resultado end
 			";

            using (var connection = _context.CreateConnection())
            {
                var parametros = new { resultado, setor };
                var processos = await connection.QueryAsync<HomologacaoModel>(query, parametros);

                return processos.ToList();
            }
        }

        public async Task<HomologacaoModel> BuscarHomologacao(string prt_numero)
        {
            //validação
            if (string.IsNullOrEmpty(prt_numero))
            {
                throw new ErrorOnValidationException(new List<string> { "O número do processo não foi identificado." });
            }

            
                var query = @"				 
                 select MOVPRO_ID,
                        SETSUB_NOME,
                        SETSUB_ID,
                        PRT_NUMERO,
                        PRT_AIT,
		                Convert(varchar(10),CASE WHEN PRT_DT_POSTAGEM IS NULL THEN PRT_DT_ABERTURA ELSE PRT_DT_POSTAGEM END,103) as PRT_DT_ABERTURA,
                        ASS_NOME as PRT_NOME_ASSUNTO,
                        dbo.GetResultadoDefesa( PRT_RESULTADO ) as PRT_RESULTADO,
                        PRT_OBSERVACAO
                  FROM  Protocolo inner join Movimentacao_Processo on(prt_numero = MOVPRO_PRT_NUMERO)
			                      inner join Assunto on (PRT_ASSUNTO = ASS_ID)  
                                  inner join SetorSub on(SETSUB_ID = MOVPRO_SETOR_ORIGEM)
		          WHERE REPLACE(PRT_NUMERO, '/', '') = @prt_numero
		            and MOVPRO_STATUS = 'HOMOLOGAR'";

                using (var connection = _context.CreateConnection())
                {
                    var parametros = new { prt_numero };
                    var result = await connection.QueryFirstOrDefaultAsync<HomologacaoModel>(query, parametros);

                    return result;
                }
           

           
        }

        public async Task<List<Anexo_Model>> BuscarAnexo(string ait)
        {
            //validação
            if (string.IsNullOrEmpty(ait))
            {
                throw new ErrorOnValidationException(new List<string> { "O número do AIT não foi identificado." });
            }

            var query = @"


			                select PRTDOC_ID,
				                   PRTDOC_PRT_NUMERO,
			                       PRTDOC_OBSERVACAO,
				                   PRTDOC_PRT_AIT,
				                   Convert(varchar(10),PRTDOC_DATA_HORA,103) as PRTDOC_DATA_HORA 
			                  from Protocolo_Documento_Imagem 
                             where PRTDOC_PRT_AIT = @ait ";


                using (var connection = _context.CreateConnection())
                {
                    var parametros = new { ait };
                    var command = await connection.QueryAsync<Anexo_Model>(query, parametros);
                    return command.ToList();
                }


            
        }

        public async Task<List<SetorModel>> BuscarSetor()
        {
              var query = @" SELECT 
                               SETSUB_ID, 
                               UPPER(SETSUB_NOME) AS SETSUB_NOME 
                          FROM SetorSub  
                         WHERE SETSUB_Ativo = 1  
                         ORDER BY SETSUB_NOME ";

                using (var connection = _context.CreateConnection())
                {
                    var command = await connection.QueryAsync<SetorModel>(query);
                    return command.ToList();
                }

        }

        public async Task<List<JulgamentoModel>> BuscarVotacao(string processo)
        {
            //validação
            if (string.IsNullOrEmpty(processo))
            {
                throw new ErrorOnValidationException(new List<string> { "O número do processo não foi identificado." });
            }


           
                var query = @"  select 
                                    MOVPRO_ID, 
		                            DISJUG_RELATOR,
                                    FORMAT(DISJUG_RESULTADO_DATA, 'dd/MM/yyyy HH:mm') AS DISJUG_RESULTADO_DATA,
		                            Case when DISJUG_RESULTADO = 'I' then 'INDEFERIDO'
		                            when DISJUG_RESULTADO IN('D','O') then 'DEFERIDO' 
		                            ELSE 'AGUARDANDO...' END AS DISJUG_RESULTADO,
		                            Case when DISJUG_PARECER_RELATORIO is null then 'MEMBRO' else 'RELATOR' END AS DISJUG_TIPO
                              from  Protocolo_Distribuicao_Julgamento 
                                    inner join Protocolo_Distribuicao on(DISJUG_DIS_ID = DIS_ID)
	                                inner join Movimentacao_Processo on (DIS_MOV_ID = MOVPRO_ID) 
                              WHERE replace(movpro_prt_numero,'/','') = @processo";


                using (var connection = _context.CreateConnection())
                {
                    var parametros = new { processo };
                    var command = await connection.QueryAsync<JulgamentoModel>(query, parametros);
                    return command.ToList();
                }

            
        }

        public async Task RealizarHomologacao(JulgamentoModel julgamentoModel,  IDbConnection connection, IDbTransaction transaction)
        {
           
                //validando a model agendamento 
                var validator = new HomologarValidator();
                var result = validator.Validate(julgamentoModel);
                if (result.IsValid == false)
                    throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
                //fim



                var dbParametro = new DynamicParameters();
                dbParametro.Add("@MOVPRO_ID", julgamentoModel.MovPro_id);
                dbParametro.Add("@USUARIOORIGEM", julgamentoModel.Disjug_Homologador);
                dbParametro.Add("@MOVPRO_PARECER_ORIGEM", julgamentoModel.Disjug_Parecer_Relatorio);
                dbParametro.Add("@PRT_NUMERO", julgamentoModel.MovPro_Prt_Numero);
                dbParametro.Add("@SETORDESTINO", julgamentoModel.Disjul_SetSub_Id);


                var query = @"
	             UPDATE Movimentacao_Processo
	                Set MOVPRO_STATUS = 'HOMOLOGADO->PUBLICAR'
	              WHERE MOVPRO_ID = @MOVPRO_ID

                 insert into Movimentacao_Processo
                 Select MOVPRO_PRT_NUMERO, 
                        MOVPRO_SETOR_DESTINO as SETORORIGEM,
                        @USUARIOORIGEM,
                        Getdate(),
                        null,
                        'Processo homologado e encaminhado para publicação.',
                        @SETORDESTINO,
	                    'PUBLICAR',
		                null,   
		                null
                   from Movimentacao_Processo where MOVPRO_ID = @MOVPRO_ID

                 update Protocolo  
                    set PRT_HOMOLOGADOR = @USUARIOORIGEM,  
                        PRT_DT_HOMOLOGACAO = GETDATE(),  
                        PRT_ACAO = 'PUBLICAR'  
                  Where PRT_NUMERO = @PRT_NUMERO  
                ";
                await connection.ExecuteAsync(query, dbParametro, transaction);
           


        }


        public async Task HomologarTodos(HomologacaoModel homologacaoModel, IDbConnection connection, IDbTransaction transaction)
        {

            ////validando a model agendamento 
            //var validator = new HomologarValidator();
            //var result = validator.Validate(homologacaoModel);
            //if (result.IsValid == false)
            //    throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
            ////fim


            try
            {
                var dbParametro = new DynamicParameters();

                dbParametro.Add("@USUARIOORIGEM", homologacaoModel.PRT_HOMOLOGADOR);
                dbParametro.Add("@PRT_RESULTADO", homologacaoModel.PRT_RESULTADO);
                dbParametro.Add("@SETORDESTINO", homologacaoModel.SETSUB_ID);


                var query = @"
	            
                     Select Movpro_id,       
                            PRT_NUMERO        
                       into #temp_Movimentacao
                       From Protocolo inner join Movimentacao_Processo on (prt_numero = MOVPRO_PRT_NUMERO)
                                      inner join Assunto on (PRT_ASSUNTO = ASS_ID)  
			                          inner join SetorSub on(SETSUB_ID = MOVPRO_SETOR_ORIGEM)
                      Where MOVPRO_SETOR_ORIGEM = @SETORDESTINO
                        and MOVPRO_STATUS = 'HOMOLOGAR'
                        and PRT_RESULTADO like Case when @PRT_RESULTADO = 'Todos' then '%' else @PRT_RESULTADO end
 

                     UPDATE MP
                        Set MOVPRO_STATUS = 'HOMOLOGADO->PUBLICAR'
                       from #temp_Movimentacao TM, Movimentacao_Processo MP
                      WHERE MP.MOVPRO_ID = TM.Movpro_id


                     insert into Movimentacao_Processo
                     Select MOVPRO_PRT_NUMERO, 
                            MOVPRO_SETOR_DESTINO as SETORORIGEM,
                            @USUARIOORIGEM,
                            Getdate(),
                            null,
                            'Processo homologado e encaminhado para publicação.',
                            @SETORDESTINO,
                            'PUBLICAR',
                            null,   
                            null
                       from Movimentacao_Processo MP inner join #temp_Movimentacao TM on(MP.MOVPRO_ID = TM.Movpro_id)


                     update Protocolo  
                        set PRT_HOMOLOGADOR = @USUARIOORIGEM,  
                            PRT_DT_HOMOLOGACAO = GETDATE(),  
                            PRT_ACAO = 'PUBLICAR'
                    from  #temp_Movimentacao TM, Protocolo P		
                      Where TM.PRT_NUMERO = P.PRT_NUMERO 

                ";
                await connection.ExecuteAsync(query, dbParametro, transaction);
            }
            catch (Exception ex)
            {

                throw;
            }


          



        }



        public async Task<JulgamentoModel> BuscarParecer(string processo)
        {
           
            var query = @"  select top 1 
                                    MovPro_id,
                                    MovPro_Prt_Numero,
                                    Disjug_Parecer_Relatorio
                                from Protocolo_Distribuicao_Julgamento 
                        inner join Protocolo_Distribuicao on(DISJUG_DIS_ID = DIS_ID)
	                    inner join Movimentacao_Processo on (DIS_MOV_ID = MOVPRO_ID) 
                                where Replace(movpro_prt_numero,'/','') = @processo
	                            and Disjug_Parecer_Relatorio is not null";


            using (var connection = _context.CreateConnection())
            {
                var parametros = new { processo };
                var command = await connection.QueryFirstOrDefaultAsync<JulgamentoModel>(query, parametros);
                return command;
            }

           
        }

        public async Task<List<AnexoModel>> BuscarAnexosBanco(string prt_numero)
        {
            List<AnexoModel> anexoModel = new List<AnexoModel>();

            using (var connection = _context.CreateConnection())
            {                
                string selectQuery = @" SELECT PRTDOC_ID,
                                               PRTDOC_PRT_NUMERO,
                                               PRTDOC_OBSERVACAO,
                                               PRTDOC_IMAGEM
                                          FROM Protocolo_Documento_Imagem 
                                         WHERE REPLACE(PRTDOC_PRT_NUMERO, '/', '') = @prt_numero";

                using (var selectCommand = connection.CreateCommand())
                {
                    connection.Open();

                    selectCommand.CommandText = selectQuery;
                    var param = selectCommand.CreateParameter();
                    param.ParameterName = "@prt_numero";
                    param.Value = prt_numero;
                    selectCommand.Parameters.Add(param);

                    // Executa a consulta SELECT
                    using (var reader = selectCommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var imagemBytes = (byte[])reader["PRTDOC_IMAGEM"];
                            var imagemBase64 = Convert.ToBase64String(imagemBytes);
                            var nomeArquivo = reader["PRTDOC_OBSERVACAO"].ToString();

                            // Cria uma nova instância de AnexoModel
                            var anexo = new AnexoModel
                            {
                                nome = nomeArquivo,
                                caminhosrc = $"<img src='data:image/jpeg;base64,{imagemBase64}' alt='Imagem' style=\"width: 100%; height: 150px;\">",
                                caminhohref = $"data:image/jpeg;base64,{imagemBase64}"
                            };
                            
                            // Adiciona o objeto AnexoModel à lista
                            anexoModel.Add(anexo);
                        }
                    }
                }

                return anexoModel;
            }
           
        }

        public async Task RetornarJulgamento(RetificacaoModel retificacaoModel, IDbConnection connection, IDbTransaction transaction)
        {
            //validando a model agendamento 
            var validator = new RetificacaoValidator();
            var result = validator.Validate(retificacaoModel);
            if (result.IsValid == false)
                throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
            //fim


            var dbParametro = new DynamicParameters();
                dbParametro.Add("@MOVPRO_ID", retificacaoModel.MOVPRO_ID);
                dbParametro.Add("@MOVPRO_PARECER_ORIGEM", retificacaoModel.MOVPRO_PARECER_ORIGEM);
                dbParametro.Add("@MOVPRO_USUARIO_ORIGEM", retificacaoModel.MOVPRO_USUARIO_ORIGEM);


                var query = @"
		             DECLARE @SetorOrigem int

			             SET @SetorOrigem = (Select top 1 
			 		                                SETSUBUSU_SETSUB_ID 
								               from SetorSubXUsuario 
							                   where SETSUBUSU_USUARIO = @MOVPRO_USUARIO_ORIGEM)
	             

                    SELECT TOP 1 DIS_ID, DIS_MOV_ID, 
			                     MOVPRO_SETOR_ORIGEM AS SetorDestino, 
			                     MOVPRO_PRT_NUMERO 
                            INTO #Processo
                            FROM Movimentacao_Processo 
                      INNER JOIN Protocolo_Distribuicao ON (MovPro_id = DIS_MOV_ID)
                           WHERE MOVPRO_PRT_NUMERO = ( SELECT TOP 1 Movpro_prt_numero 
					        		                    FROM Movimentacao_Processo 
							                           WHERE MovPro_id = @MOVPRO_ID)

                          DELETE PD
                            FROM Protocolo_Distribuicao_Julgamento pd INNER JOIN #Processo ap ON (pd.DISJUG_DIS_ID = ap.DIS_ID)

                          UPDATE MP
                             SET MOVPRO_STATUS = 'HOMOLOGADO->DEVOLVIDO'
                            FROM Movimentacao_Processo MP where MOVPRO_ID = @MOVPRO_ID


                        --Mudando o status dos processo distribuido
                          UPDATE PD
                             SET DIS_DESTINO_STATUS = 'RECEBIDO',
                            FROM Protocolo_Distribuicao PD INNER JOIN #Processo P ON (pd.DIS_MOV_ID = p.DIS_MOV_ID)
                     

                          UPDATE P
                             SET PRT_ACAO = NULL,  
	                             PRT_DT_JULGAMENTO = NULL,  
	                             PRT_RESULTADO = NULL,  
                                 PRT_DT_HOMOLOGACAO = NULL  
                            FROM Protocolo P INNER JOIN  #Processo  ON (P.PRT_NUMERO = MOVPRO_PRT_NUMERO)
                 

                    -- Inserindo novo registro na Movimentacao_Processo
                          INSERT INTO Movimentacao_Processo
                          SELECT MOVPRO_PRT_NUMERO,
                                 @SetorOrigem,
                                 @MOVPRO_USUARIO_ORIGEM, 
                                 GETDATE(),
                                 @MOVPRO_PARECER_ORIGEM, -- Observação ou parecer!
                                'Processo devolvido para o Relator.',
                                 SetorDestino,
                                'RECEBIDO->DISTRIBUIDO',
                                 NULL,
                                 NULL
                           FROM #Processo;

                    -- Limpando a tabela temporária
                        DROP TABLE #Processo;

                ";
                await connection.ExecuteAsync(query, dbParametro, transaction);
           

         
        }


        /*
         */
    }
}


