﻿using Dapper;
using Microsoft.AspNetCore.Http;
using SIPROSHARED.DbContext;
using SIPROSHARED.Models;
using SIPROSHAREDINSTRUCAO.Models;
using SIPROSHAREDINSTRUCAO.Service.IRepository;
using System.Data;

namespace SIPROSHAREDINSTRUCAO.Service.Repository
{
    public class InstrucaoService : IInstrucaoService
    {
        
        private readonly DapperContext _context;

        public InstrucaoService(DapperContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));           
        }


        public async Task<List<InstrucaoProcessosModel>> LocalizarInstrucao(string usuario, string vlobusca)
        {
            
            var query = @"
				Select 
		                DIS_ID,
                        Convert(varchar(10),DIS_ORIGEM_DATA,103) as DIS_ORIGEM_DATA,
	                    MOVPRO_ID,
	                    MOVPRO_USUARIO_ORIGEM,
                        PRT_NUMERO,
                        PRT_AIT,
                        (Select top 1 SETSUB_NOME from SetorSub where (MOVPRO_SETOR_ORIGEM = SETSUB_ID)) as PRT_SETOR_ORIGEM,
                        ASS_NOME as PRT_NOME_ASSUNTO,
                        MOVPRO_PARECER_ORIGEM as PRT_PARECER
   
                    from Protocolo_distribuicao 
                        inner join Movimentacao_Processo on (DIS_MOV_ID = MOVPRO_ID)
						inner join Protocolo on(PRT_NUMERO = MOVPRO_PRT_NUMERO)
						inner join Assunto on (PRT_ASSUNTO = ASS_ID)							
			        where DIS_DESTINO_USUARIO = @usuario
                    and PRT_AIT like case when @vlobusca = 'Todos' then '%' else @vlobusca end
			        and DIS_DESTINO_STATUS = 'RECEBIDO' 
                    and MOVPRO_INSTRUCAO = 1
					";

            using (var connection = _context.CreateConnection())
            {
                var parametros = new { usuario, vlobusca };
                var processos = await connection.QueryAsync<InstrucaoProcessosModel>(query, parametros);

                return processos.ToList();
            }
            
        }

        public async Task<InstrucaoProcessosModel> GetInstrucao(int dis_id)
        {
       
            var query = @"
						Select 
		                       DIS_ID,
	                           Convert(varchar(10),DIS_ORIGEM_DATA,103) as DIS_ORIGEM_DATA,
                               MOVPRO_ID,
	                           MOVPRO_USUARIO_ORIGEM,                                  
                               PRT_NUMERO,
                               PRT_AIT,
                               PRT_DT_ABERTURA,
                               PRT_DT_POSTAGEM,
                               PRT_CPFCNJ_PROPRIETARIO,
                               PRT_NOMEPROPRIETARIO,
                               (Select top 1 SETSUB_NOME from SetorSub where (MOVPRO_SETOR_ORIGEM = SETSUB_ID)) as PRT_NOME_ORIGEM,
                               ASS_NOME as PRT_NOME_ASSUNTO,
                               MOVPRO_PARECER_ORIGEM as PRT_PARECER                                    	   
                          from Protocolo_distribuicao 
                               inner join Movimentacao_Processo on (DIS_MOV_ID = MOVPRO_ID)
							   inner join Protocolo on(PRT_NUMERO = MOVPRO_PRT_NUMERO)
							   inner join Assunto on (PRT_ASSUNTO = ASS_ID)							
		                 where DIS_ID = @DIS_ID 
		                   and DIS_DESTINO_STATUS = 'RECEBIDO' 
                           and MOVPRO_INSTRUCAO = 1
						";

            using (var connection = _context.CreateConnection())
            {
                var parametros = new { dis_id };
                var processo = await connection.QueryFirstOrDefaultAsync<InstrucaoProcessosModel>(query, parametros);

                return processo;
            }
            
        }

        public async Task<List<SetorModel>> BuscarSetor()
        {
            
            var query = @" SELECT SETSUB_ID, 
                                  UPPER(SETSUB_NOME) AS SETSUB_NOME 
                             FROM SetorSub  
                            WHERE SETSUB_Ativo = 1  
                         ORDER BY SETSUB_NOME"
            ;


            using (var connection = _context.CreateConnection())
            {

                var command = await connection.QueryAsync<SetorModel>(query);
                return command.ToList();
            }
            
        }

        public async Task<InstrucaoModel> BuscarMovimentacaoInstrucao(int dis_id)
        {
        
            var query = @"   
                Select MOVPRO_USUARIO_ORIGEM as INSPRO_Usuario_origem,
                       MOVPRO_PARECER_ORIGEM as INSPRO_Parecer,
                       MOVPRO_DATA_ORIGEM as INSPRO_DATA_ORIGEM,
                       PRTDOC_OBSERVACAO as INSPRO_Arquivo,
                       ssb.SETSUB_NOME as INSPRO_Setor_origem ,
		               ssa.SETSUB_NOME as INSPRO_Setor_destino
                  from Protocolo inner join Movimentacao_Processo on (PRT_NUMERO = MOVPRO_PRT_NUMERO) 
	                             inner join Protocolo_distribuicao on (DIS_MOV_ID = MOVPRO_ID)
				                 inner join SetorSub ssa on(MOVPRO_SETOR_DESTINO = ssa.SETSUB_ID)
		                         inner join SetorSub ssb on(MOVPRO_SETOR_ORIGEM = ssb.SETSUB_ID)
				                 left join Protocolo_Documento_Imagem on(movpro_id = PRTDOC_MOVPRO_ID)
                 where Movpro_instrucao = 1 and 
	                   DIS_DESTINO_STATUS = 'RECEBIDO' and
			           DIS_ID = @dis_id           
		             order by MOVPRO_ID";

            using (var connection = _context.CreateConnection())
            {
                var parametros = new { dis_id };
                var processo = await connection.QueryFirstOrDefaultAsync<InstrucaoModel>(query, parametros);

                return processo;
            }
           
            
        }


        //EVITAR ENVIAR PARA O PROPRIO SETOR
        public async Task EncaminharIntrucao(InstrucaoModel instrucaoProcesso, IDbConnection connection, IDbTransaction transaction)
        {
         
            // Adicionando parâmetros
            var dbParametro = new DynamicParameters();
            dbParametro.Add("@DIS_ID", instrucaoProcesso.INSPRO_Dis_id);
            dbParametro.Add("@MOVPRO_USUARIO_ORIGEM", instrucaoProcesso.INSPRO_Usuario_origem);
            dbParametro.Add("@MOVPRO_SETOR_DESTINO", instrucaoProcesso.INSPRO_Setor_destino);
            dbParametro.Add("@MOVPRO_PARECER_ORIGEM", instrucaoProcesso.INSPRO_Parecer);


            string query = @"   
                    Select
	                    DIS_ID,
	                    DIS_MOV_ID,
	                    DIS_DESTINO_USUARIO as USU_ORIGEM
	                into #Processo
                    from Protocolo_Distribuicao where Dis_Id = @DIS_ID
   
                Declare @Setor int
                    Set @Setor = (Select top 1 
						                SETSUBUSU_SETSUB_ID 
					                from SetorSubXUsuario 
					            where SETSUBUSU_USUARIO = @MOVPRO_USUARIO_ORIGEM)

	                Update PD
	                Set DIS_DESTINO_STATUS = 'INSTRUCAO->ENCAMINDO'
	                From #Processo P INNER JOIN Protocolo_Distribuicao PD ON(P.DIS_MOV_ID = PD.DIS_MOV_ID)

                    Insert Into Movimentacao_Processo
                    Select MOVPRO_PRT_NUMERO, 
		                @Setor,
			            USU_ORIGEM,
	                    Getdate(),
			            @MOVPRO_PARECER_ORIGEM,
			            'Instrucação encaminhada para o solicitante.',
			            @MOVPRO_SETOR_DESTINO,
			            'INSTRUCAO->ENCAMINHADO',
			            null,   
			            1
	                from #Processo inner join Movimentacao_Processo on (DIS_MOV_ID = MOVPRO_ID)
                    where MOVPRO_INSTRUCAO = 1	

	            Declare @UltimoID int
                    Set @UltimoID = SCOPE_IDENTITY();

	                Insert into Protocolo_Distribuicao
                    Select 
	                    @UltimoID,
		                USU_ORIGEM,
		                GETDATE(),
		                MOVPRO_USUARIO_ORIGEM AS USU_DESTINO,
		                0,
		                'RECEBIDO',
		                0,
		                0,
		                NULL
		            from #Processo inner join Movimentacao_Processo on(DIS_MOV_ID = MOVPRO_ID) ";

            await connection.ExecuteAsync(query, dbParametro, transaction);

        }

       

        public async Task IntoAnexo(List<IFormFile> arquivos, ProtocoloModel protocolo)
        {
            // Verifica se há arquivos na lista
            if (arquivos != null && arquivos.Count > 0)
            {
                foreach (var arquivo in arquivos)
                {
                    string fileName = arquivo.FileName;

                    // Lê o arquivo como um array de bytes
                    byte[] imagemBytes;
                    using (var memoryStream = new MemoryStream())
                    {
                        await arquivo.CopyToAsync(memoryStream);
                        imagemBytes = memoryStream.ToArray();
                    }

                    var dbParametro = new DynamicParameters();
                    dbParametro.Add("@PRTDOC_DOC_ID", 0);
                    dbParametro.Add("@PRTDOC_PRT_NUMERO", protocolo.PRT_NUMERO);
                    dbParametro.Add("@PRTDOC_IMAGEM", imagemBytes);
                    dbParametro.Add("@PRTDOC_OBSERVACAO", fileName);
                    dbParametro.Add("@PRTDOC_PRT_AIT", protocolo.PRT_AIT);
                    dbParametro.Add("@PRT_ATENDENTE", protocolo.PRT_ATENDENTE);

                    string query = @"

                        Declare @PRTDOC_PRT_SETOR int

					    Set @PRTDOC_PRT_SETOR = (Select top 1 SETSUBUSU_SETSUB_ID 
								        from SetorSubXUsuario 
								    where SETSUBUSU_USUARIO = @PRT_ATENDENTE )

                            INSERT INTO Protocolo_Documento_Imagem 
                                    (PRTDOC_DOC_ID, 
                                        PRTDOC_PRT_NUMERO, 
                                        PRTDOC_IMAGEM, 
                                        PRTDOC_OBSERVACAO, 
                                        PRTDOC_DATA_HORA, 
                                        PRTDOC_PRT_AIT, 
                                        PRTDOC_PRT_SETOR)
                                        VALUES 
                                    (@PRTDOC_DOC_ID, 
                                        @PRTDOC_PRT_NUMERO, 
                                        @PRTDOC_IMAGEM, 
                                        @PRTDOC_OBSERVACAO, 
                                        GETDATE(), 
                                        @PRTDOC_PRT_AIT, 
                                        @PRTDOC_PRT_SETOR)
                                        ";

                    using (var connection = _context.CreateConnection())
                    {
                        await connection.ExecuteAsync(query, dbParametro);
                    }
                }
            }


        }

        public async Task<List<Anexo_Model>> BuscarAnexo(string usuario, string ait)
        {
            {
               
                var query = @"

	                 Declare @Setor int

				   Set @Setor = (Select top 1 
                                 SETSUBUSU_SETSUB_ID 
					         from SetorSubXUsuario 
					        where SETSUBUSU_USUARIO = @usuario )

			                select PRTDOC_ID,
				                   PRTDOC_PRT_NUMERO,
			                       PRTDOC_OBSERVACAO,
				                   PRTDOC_PRT_AIT,
				                   Convert(varchar(10),PRTDOC_DATA_HORA,103) as PRTDOC_DATA_HORA 
			                  from Protocolo_Documento_Imagem 
                             where PRTDOC_PRT_SETOR = @Setor and PRTDOC_PRT_AIT = @ait ";


                using (var connection = _context.CreateConnection())
                {
                    var parametros = new { usuario, ait };
                    var command = await connection.QueryAsync<Anexo_Model>(query, parametros);
                    return command.ToList();
                }
               
               
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
                    string selectQuery = @"    SELECT 
                                           PRTDOC_ID,
                                           PRTDOC_PRT_NUMERO,
                                           PRTDOC_OBSERVACAO,
                                           PRTDOC_IMAGEM
                                    FROM Protocolo_Documento_Imagem 
                                    WHERE REPLACE(PRTDOC_PRT_NUMERO, '/', '') = @prt_numero
                                    and PRTDOC_PRT_SETOR = 49 "; //Passar o id do setor futuramente

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
                                int prtdoc_id = reader.GetInt32(reader.GetOrdinal("PRTDOC_ID"));


                                // Cria uma nova instância de AnexoModel
                                var anexo = new AnexoModel
                                {
                                    nome = nomeArquivo,
                                    caminhosrc = $"<img src='data:image/jpeg;base64,{imagemBase64}' alt='Imagem' style=\"width: 100%; height: 150px;\">",
                                    caminhohref = $"data:image/jpeg;base64,{imagemBase64}",
                                    prtdoc_id = prtdoc_id,
                                    prt_numero = prt_numero,
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

        public async Task ExcluirAnexo(int prtdoc_id)
        {
           
            // Adicionando parâmetros
            var dbParametro = new DynamicParameters();
            dbParametro.Add("@prtdoc_id", prtdoc_id);

            string query = @" Delete 
                                from Protocolo_Documento_Imagem 
                                where prtdoc_id = @prtdoc_id ";

            using (var con = _context.CreateConnection())
            {
                await con.ExecuteAsync(query, dbParametro);
            }
            
        }
            
 

     
    }
}
