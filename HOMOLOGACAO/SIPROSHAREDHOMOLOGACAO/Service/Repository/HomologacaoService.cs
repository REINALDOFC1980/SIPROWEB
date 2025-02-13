using Dapper;
using SIPROSHARED.DbContext;
using SIPROSHARED.Models;
using SIPROSHAREDHOMOLOGACAO.Models;
using SIPROSHAREDHOMOLOGACAO.Service.IRepository;
using SIRPOEXCEPTIONS.ExceptionBase;
using System.Data;

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

        public async Task<HomologacaoModel> GetHomologacao(int movpro_id)
        {
            var query = @"				 
                 select MOVPRO_ID,
                        SETSUB_NOME,
                        PRT_NUMERO,
                        PRT_AIT,
		                Convert(varchar(10),CASE WHEN PRT_DT_POSTAGEM IS NULL THEN PRT_DT_ABERTURA ELSE PRT_DT_POSTAGEM END,103) as PRT_DT_ABERTURA,
                        ASS_NOME as PRT_NOME_ASSUNTO,
                        dbo.GetResultadoDefesa( PRT_RESULTADO ) as PRT_RESULTADO,
                        PRT_OBSERVACAO
                  FROM  Protocolo inner join Movimentacao_Processo on(prt_numero = MOVPRO_PRT_NUMERO)
			                      inner join Assunto on (PRT_ASSUNTO = ASS_ID)  
                                  inner join SetorSub on(SETSUB_ID = MOVPRO_SETOR_ORIGEM)
		          WHERE Movpro_id = @movpro_id
		            and MOVPRO_STATUS = 'HOMOLOGAR'";

            using (var connection = _context.CreateConnection())
            {
                var parametros = new { movpro_id };
                var processo = await connection.QueryFirstOrDefaultAsync<HomologacaoModel>(query, parametros);

                return processo;
            }
        }

        public async Task<List<Anexo_Model>> BuscarAnexo(string ait)
        {
            {

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
        }

        public async Task<List<SetorModel>> BuscarSetor()
        {
            try
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
            catch (Exception ex)
            {
                throw;
            }



        }

        public async Task<List<JulgamentoModel>> BuscarVotacao(string processo)
        {
            try
            {
                var query = @"  select 
		                            DISJUG_RELATOR,
                                    FORMAT(DISJUG_RESULTADO_DATA, 'dd/MM/yyyy HH:mm') AS DISJUG_RESULTADO_DATA,
		                            Case when DISJUG_RESULTADO = 'I' then 'INDEFERIDO'
		                            when DISJUG_RESULTADO IN('D','O') then 'DEFERIDO' 
		                            ELSE 'AGUARDANDO...' END AS DISJUG_RESULTADO,
		                            Case when DISJUG_PARECER_RELATORIO is null then 'MEMBRO' else 'PRESIDENTE' END AS DISJUG_TIPO
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
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RealizarHomologacao(HomologacaoModel homologacaoModel, IDbConnection connection, IDbTransaction transaction)
        {
            var dbParametro = new DynamicParameters();
            dbParametro.Add("@MOVPRO_ID", 0);
            dbParametro.Add("@USUARIOORIGEM", "");
            dbParametro.Add("@MOVPRO_PARECER_ORIGEM", "");
            dbParametro.Add("@@PRT_NUMERO", homologacaoModel.PRT_NUMERO);


            var query = @"
	             UPDATE Movimentacao_Processo
	                Set MOVPRO_STATUS = 'HOLOMOLOGADO->PUBLICAR'
	              WHERE MOVPRO_ID = @MOVPRO_ID

                 insert into Movimentacao_Processo
                 Select MOVPRO_PRT_NUMERO, 
                        MOVPRO_SETOR_DESTINO as SETORORIGEM,
                        @USUARIOORIGEM,
                        Getdate(),
                        @MOVPRO_PARECER_ORIGEM,
                        'Processo homologado e encaminhado para publicação.',
                        00 as SETORDESTINO,
	                    'PUBLICAR',
		                null,   
		                null
                   from Movimentacao_Processo where MOVPRO_ID = @MOVPRO_ID

                 update Protocolo  
                    set PRT_HOMOLOGADOR = @Homologador,  
                        PRT_DT_HOMOLOGACAO = GETDATE(),  
                        PRT_ACAO = 'PUBLICAR'  
                  Where PRT_NUMERO = @PRT_NUMERO  


                ";
            await connection.ExecuteAsync(query, dbParametro, transaction);

        }

        public async Task<JulgamentoModel> BuscarParecer(string processo)
        {
            try
            {
                var query = @"  select top 1 
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
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<AnexoModel>> BuscarAnexosBanco(string prt_numero)
        {
            try
            {
                List<AnexoModel> anexoModel = new List<AnexoModel>();

                using (var connection = _context.CreateConnection())
                {
                    // Agora, você pode recuperar as imagens da tabela temporária
                    string selectQuery = @"    SELECT PRTDOC_ID,
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
            catch (Exception ex)
            {

                throw;
            }

           
        }
    }
}


