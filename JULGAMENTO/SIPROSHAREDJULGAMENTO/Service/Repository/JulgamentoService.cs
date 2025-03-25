using Dapper;
using Microsoft.AspNetCore.Http;
using SIPROSHARED.DbContext;
using SIPROSHARED.Models;
using SIPROSHAREDHOMOLOGACAO.Validator;
using SIPROSHAREDJULGAMENTO.Models;
using SIPROSHAREDJULGAMENTO.Service.IRepository;
using SIRPOEXCEPTIONS.ExceptionBase;
using System.Data;
using System.Data.Common;
using System.Transactions;

namespace SIPROSHAREDJULGAMENTO.Service.Repository
{
    public class JulgamentoService : IJulgamentoService
    {
        private readonly DapperContext _context;

        public JulgamentoService(DapperContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<List<ProtocoloJulgamento_Model>> LocalizarProcessos(string usuario, string vlobusca)
        {
            //validação

            var query = @"
							SELECT PRT_NUMERO
                                  ,MOVPRO_SETOR_ORIGEM as PRT_COD_ORIGEM
	                              ,ORI_DESCRICAO as PRT_NOME_ORIGEM
		                          ,ASS_NOME as PRT_NOME_ASSUNTO
		                          ,PRT_DT_ABERTURA
		                          ,PRT_DT_POSTAGEM
		                          ,PRT_CPFCNJ_PROPRIETARIO
		                          ,PRT_NOMEPROPRIETARIO							  
								  ,PRT_AIT
                                  ,PRT_PLACA
                                  ,'Julgar' AS  PRT_SITUACAO
                                  ,PRT_OBSERVACAO
                              FROM Protocolo inner join Movimentacao_Processo on (PRT_NUMERO = MOVPRO_PRT_NUMERO)
				                             inner join Protocolo_distribuicao on (MOVPRO_ID = DIS_MOV_ID)
				                             inner join Assunto on (PRT_ASSUNTO = ASS_ID)
				                             inner join Origem on(PRT_ORIGEM = ORI_CODIGO)
							WHERE DIS_DESTINO_USUARIO = @usuario
                              and PRT_AIT like case when @vlobusca = 'Todos' then '%' else @vlobusca end
							  and DIS_DESTINO_STATUS = 'RECEBIDO'
                              and isnull(DIS_RETORNO,0) = 0
                              and DIS_ID not in (select DISJUG_DIS_ID from Protocolo_Distribuicao_Julgamento)
							";

                using (var connection = _context.CreateConnection())
                {
                    var parametros = new { usuario, vlobusca };
                    var processos = await connection.QueryAsync<ProtocoloJulgamento_Model>(query, parametros);

                    return processos.ToList();
                }
            
        }

        public async Task<List<ProtocoloJulgamento_Model>> LocalizarProcessosAssinar(string usuario, string vlobusca)
        {
           
                var query = @"
							SELECT PRT_NUMERO 
                                  ,ORI_DESCRICAO as PRT_NOME_ORIGEM
                                  ,ASS_NOME as PRT_NOME_ASSUNTO
                                  ,Convert(varchar(10),PRT_DT_ABERTURA,103) as PRT_DT_ABERTURA
                                  ,Convert(varchar(10),PRT_DT_POSTAGEM,103) as PRT_DT_POSTAGEM
                                  ,PRT_CPFCNJ_PROPRIETARIO
                                  ,PRT_NOMEPROPRIETARIO							  
	                              ,PRT_AIT
                                  ,PRT_PLACA
                                  ,DIS_DESTINO_STATUS	
                                  ,'Assinar' AS  PRT_SITUACAO
                                  ,PRT_OBSERVACAO
                              FROM Protocolo inner join Movimentacao_Processo on (PRT_NUMERO = MOVPRO_PRT_NUMERO)
                                             inner join Protocolo_distribuicao on (MOVPRO_ID = DIS_MOV_ID)
				                             inner join Protocolo_Distribuicao_Julgamento on (DIS_ID = DISJUG_DIS_ID)
                                             inner join Assunto on (PRT_ASSUNTO = ASS_ID)
                                             inner join Origem on(PRT_ORIGEM = ORI_CODIGO)
                            WHERE DISJUG_RELATOR = @usuario
                              and PRT_AIT like case when @vlobusca = 'Todos' then '%' else @vlobusca end
                              and DIS_DESTINO_STATUS in('JULGANDO')
                              and DISJUG_RESULTADO_DATA IS NULL  
							";

                using (var connection = _context.CreateConnection())
                {
                    var parametros = new { usuario, vlobusca };
                    var processos = await connection.QueryAsync<ProtocoloJulgamento_Model>(query, parametros);

                    return processos.ToList();
                }
        }

        public async Task<List<ProtocoloJulgamento_Model>> LocalizarRetificacao(string usuario, string vlobusca)
        {
            var query = @"
							SELECT PRT_NUMERO
                                  ,MOVPRO_SETOR_ORIGEM as PRT_COD_ORIGEM
	                              ,ORI_DESCRICAO as PRT_NOME_ORIGEM
		                          ,ASS_NOME as PRT_NOME_ASSUNTO
		                          ,PRT_DT_ABERTURA
		                          ,PRT_DT_POSTAGEM
		                          ,PRT_CPFCNJ_PROPRIETARIO
		                          ,PRT_NOMEPROPRIETARIO							  
								  ,PRT_AIT
                                  ,PRT_PLACA
                                  ,'Julgar' AS  PRT_SITUACAO
                                  ,PRT_OBSERVACAO
                              FROM Protocolo inner join Movimentacao_Processo on (PRT_NUMERO = MOVPRO_PRT_NUMERO)
				                             inner join Protocolo_distribuicao on (MOVPRO_ID = DIS_MOV_ID)
				                             inner join Assunto on (PRT_ASSUNTO = ASS_ID)
				                             inner join Origem on(PRT_ORIGEM = ORI_CODIGO)
							WHERE DIS_DESTINO_USUARIO = @usuario
                              and PRT_AIT like case when @vlobusca = 'Todos' then '%' else @vlobusca end
							  and DIS_DESTINO_STATUS = 'RECEBIDO'
                              and DIS_RETORNO = 1
                                and DIS_ID not in (select DISJUG_DIS_ID from Protocolo_Distribuicao_Julgamento)
							";

            using (var connection = _context.CreateConnection())
            {
                var parametros = new { usuario, vlobusca };
                var processos = await connection.QueryAsync<ProtocoloJulgamento_Model>(query, parametros);

                return processos.ToList();
            }
        }

        public async Task<ProtocoloJulgamento_Model> LocalizarProcesso(string usuario, string vlobusca)
        {
          
            var query = @"
				 SELECT    PRT_NUMERO 
                                  ,MOVPRO_SETOR_ORIGEM as PRT_COD_ORIGEM
                                  ,ORI_DESCRICAO as PRT_NOME_ORIGEM
                                  ,ASS_NOME as PRT_NOME_ASSUNTO
                                  ,PRT_DT_ABERTURA
                                  ,PRT_DT_POSTAGEM
                                  ,PRT_CPFCNJ_PROPRIETARIO
                                  ,PRT_NOMEPROPRIETARIO							  
	                              ,PRT_AIT
                                  ,PRT_PLACA
 	                              ,DIS_ID
                                  ,PRT_OBSERVACAO
                         FROM Protocolo inner join Movimentacao_Processo  on (PRT_NUMERO = MOVPRO_PRT_NUMERO)
			                          inner join Protocolo_distribuicao on (MOVPRO_ID = DIS_MOV_ID)
			                          inner join Assunto on (PRT_ASSUNTO = ASS_ID)
			                          inner join Origem on(PRT_ORIGEM = ORI_CODIGO)
			                    WHERE 
			                    PRT_AIT = @vlobusca
			                      and DIS_DESTINO_STATUS IN('RECEBIDO','JULGANDO')
				";

            using (var connection = _context.CreateConnection())
            {
                var parametros = new { usuario, vlobusca };                   
                var processo = await connection.QueryFirstOrDefaultAsync<ProtocoloJulgamento_Model>(query, parametros);

                return processo;
            }
           
        }

        public async Task<List<MovimentacaoModel>> BuscarMovimentacao(string prt_numero)
        {
            
            var query = @" SELECT  
                                 
		                   MOVPRO_PRT_NUMERO       
		                  ,MOVPRO_USUARIO_ORIGEM   
		                  ,O.SETSUB_NOME AS MOVPRO_SETOR_ORIGEM     
		                  ,MOVPRO_ACAO_ORIGEM                                                                                                                                                                                                                                                    
		                  ,CONVERT(VARCHAR(10),MOVPRO_DATA_ORIGEM,103) as MOVPRO_DATA_ORIGEM
                          ,CONVERT(Date,MOVPRO_DATA_ORIGEM,103)
		                  ,Case when MOVPRO_SETOR_DESTINO = 0 then 'Tramitação Automática' 
                                                              else D.SETSUB_NOME end AS MOVPRO_SETOR_DESTINO  
		                  ,MOVPRO_PRTDOC_ID
                      FROM Movimentacao_Processo inner join SetorSub O on (MOVPRO_SETOR_ORIGEM = O.SETSUB_ID)
									 inner join SetorSub D on (MOVPRO_SETOR_DESTINO = D.SETSUB_ID)

                     WHERE MOVPRO_PRT_NUMERO = @prt_numero and MOVPRO_PRTDOC_ID is null
                     order by MOVPRO_ID asc  ";


            using (var connection = _context.CreateConnection())
            {
                var parametros = new { prt_numero };
                var command = await connection.QueryAsync<MovimentacaoModel>(query, parametros);
                return command.ToList();
            }
        }
          
        public async Task<List<SetorModel>> BuscarSetor()
        {
           
            var query = @" SELECT SETSUB_ID, 
                                  UPPER(SETSUB_NOME) AS SETSUB_NOME 
                             FROM SetorSub  
                            WHERE SETSUB_Ativo = 1  
                              and SETSUB_ID NOT IN (45,46,47,52,53,54,55,56)
                         ORDER BY SETSUB_NOME";
           
           
            using (var connection = _context.CreateConnection())
            {
           
                var command = await connection.QueryAsync<SetorModel>(query);
                return command.ToList();
            }
           
        }

        public async Task<List<MembroModel>> BuscarMembros(string usuario)
        {
   
            var query = @"  SELECT UPPER(SETSUBUSU_USUARIO) AS SETSUBUSU_USUARIO,
                                   UPPER(SETSUBUSU_NOMECOMPLETO) AS SETSUBUSU_NOMECOMPLETO
                              FROM SetorSubXUsuario 
                             WHERE SETSUBUSU_ATIVO = 1 
                               AND SETSUBUSU_PERFIL IN('Membros','Suplente','Presidente')
                               AND SETSUBUSU_USUARIO NOT IN (@usuario)
                               AND SETSUBUSU_SETSUB_ID = (
                                   SELECT SETSUBUSU_SETSUB_ID
                                   FROM SetorSubXUsuario
                                   WHERE SETSUBUSU_USUARIO = @usuario
                                   --AND SETSUBUSU_PERFIL = 'Presidente'
                              )
                              ORDER BY SETSUBUSU_NOMECOMPLETO ";


            using (var connection = _context.CreateConnection())
            {
                var parametros = new { usuario };
                var command = await connection.QueryAsync<MembroModel>(query, parametros);
                return command.ToList();
            }
            
        }

        public async Task<List<MotivoVotoModel>> BuscarMotivoVoto()
        {
            var query = @"   select MOT_COD,  
                                    MOT_NOME
                               from MotivoVotoDefesa
                                Order by MOT_NOME";
            
            
            using (var connection = _context.CreateConnection())
            {
                
                var command = await connection.QueryAsync<MotivoVotoModel>(query);
                return command.ToList();
            }            
        }

        public async Task EncamimharProcessoInstrucao(InstrucaoProcessoModel instrucaoProcesso)
        {
         
            // Adicionando parâmetros
            var dbParametro = new DynamicParameters();
            dbParametro.Add("@INSPRO_Dis_id", instrucaoProcesso.INSPRO_Dis_id);
            dbParametro.Add("@INSPRO_Setor_destino", instrucaoProcesso.INSPRO_Setor_destino);
            dbParametro.Add("@INSPRO_Parecer", instrucaoProcesso.INSPRO_Parecer);


            string query = @"   Select
                                DIS_ID,
                                DIS_MOV_ID,
	                            DIS_DESTINO_USUARIO as USU_ORIGEM,
                                DIS_RETORNO
                           into #Processo
                           from Protocolo_Distribuicao where Dis_Id = @INSPRO_Dis_id


                        insert into Movimentacao_Processo
                        select MOVPRO_PRT_NUMERO           
                              ,MOVPRO_SETOR_DESTINO AS	SETOR_ORIGEM
                              ,USU_ORIGEM 
                              ,GETDATE()      
                              ,@INSPRO_Parecer
                              ,'Processo encaminhado para instrução.' as MOVPRO_ACAO_ORIGEM                                 
                              ,@INSPRO_Setor_destino
                              ,'RECEBIDO' 
                              ,MOVPRO_PRTDOC_ID
                              ,1 AS INSTRUCAO
                         from #Processo pr inner join Movimentacao_Processo mp on(pr.DIS_MOV_ID = mp.MOVPRO_ID)


                        Update Mov
                           set MOVPRO_STATUS = 'DISTRIBUIDO->INSTRUCAO'
                          from Movimentacao_Processo Mov, #Processo PDis
                          where Mov.MOVPRO_ID = PDis.DIS_MOV_ID 
                            and isnull(DIS_RETORNO,0)=0 
		 
  		
                        Update Dis
                           set DIS_DESTINO_STATUS = 'INSTRUCAO'
                          from Protocolo_Distribuicao Dis, #Processo PDis
                         where Dis.DIS_ID = PDis.DIS_ID                  
                           ";

            using (var connection = _context.CreateConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await connection.ExecuteAsync(query, dbParametro, transaction);
                        transaction.Commit(); // Commit se tudo ocorrer bem
                    }
                    catch
                    {
                        transaction.Rollback(); // Rollback em caso de erro
                        throw;
                    }
                }
            }
        }

        public async Task<JulgamentoProcessoModel> BuscarParecerRelator(int vlobusca)
        {

            var query = @"   Select   top 1 
                                      Disjug_Id
		                             ,Disjug_Dis_Id   
		                             ,DISJUG_DIS_ID 
		                             ,Disjug_Relator            
		                             ,Convert(varchar(10),Disjug_Data,103)  as Disjug_Data           
		                             ,Disjug_Parecer_Relatorio                                                                                                                                                                                                                                         
		                             ,Disjug_Resultado 
		                             ,Disjug_Motivo_Voto                                
		                             ,Convert(varchar(10),Disjug_Resultado_Data,103) as Disjug_Resultado_Data
                                from Protocolo_Distribuicao_Julgamento where DISJUG_DIS_ID = @vlobusca order by Disjug_Id ";


            using (var connection = _context.CreateConnection())
            {

                var parametros = new { vlobusca };
                var parecer = await connection.QueryFirstOrDefaultAsync<JulgamentoProcessoModel>(query, parametros);


                return parecer;
            }
            
        }

        public async Task InserirVotoRelator(JulgamentoProcessoModel julgamentoProcesso)
        {


            //validando a model agendamento 
            var validator = new VotoRelatorValidator();
            var result = validator.Validate(julgamentoProcesso);
            if (result.IsValid == false)
                throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
            //fim


            var dbParametro = new DynamicParameters();
            dbParametro.Add("@Disjug_Dis_Id", julgamentoProcesso.Disjug_Dis_Id);  
            dbParametro.Add("@Disjug_Relator", julgamentoProcesso.Disjug_Relator);
            dbParametro.Add("@Disjug_Parecer_Relatorio", julgamentoProcesso.Disjug_Parecer_Relatorio.Trim());
            dbParametro.Add("@Disjug_Resultado", julgamentoProcesso.Disjug_Resultado);
            dbParametro.Add("@Disjug_Motivo_Voto", julgamentoProcesso.Disjug_Motivo_Voto);
            dbParametro.Add("@Disjug_Membro1", julgamentoProcesso.Disjug_Membro1);
            dbParametro.Add("@Disjug_Membro2", julgamentoProcesso.Disjug_Membro2);

            // Adicionar este parâmetro

            var query = @"
                      -- Inserindo o relator principal
                        IF NOT EXISTS (
                            SELECT 1 FROM Protocolo_Distribuicao_Julgamento 
                            WHERE Disjug_Dis_Id = @Disjug_Dis_Id 
                            AND Disjug_Relator = @Disjug_Relator
                        )
                        BEGIN
                            INSERT INTO Protocolo_Distribuicao_Julgamento
                            VALUES 
                            (
                                @Disjug_Dis_Id,
                                @Disjug_Relator,
                                GETDATE(),
                                @Disjug_Parecer_Relatorio,
                                @Disjug_Resultado,
                                @Disjug_Motivo_Voto,
                                GETDATE(),               
                                0
                            );
                        END;

                        -- Inserindo Membro 1
                        IF NOT EXISTS (
                            SELECT 1 FROM Protocolo_Distribuicao_Julgamento 
                            WHERE Disjug_Dis_Id = @Disjug_Dis_Id 
                            AND Disjug_Relator = @Disjug_Membro1
                        )
                        BEGIN
                            INSERT INTO Protocolo_Distribuicao_Julgamento (DISJUG_DIS_ID, DISJUG_RELATOR)  
                            VALUES (@Disjug_Dis_Id, @Disjug_Membro1);
                        END;

                        -- Inserindo Membro 2
                        IF NOT EXISTS (
                            SELECT 1 FROM Protocolo_Distribuicao_Julgamento 
                            WHERE Disjug_Dis_Id = @Disjug_Dis_Id 
                            AND Disjug_Relator = @Disjug_Membro2
                        )
                        BEGIN
                            INSERT INTO Protocolo_Distribuicao_Julgamento (DISJUG_DIS_ID, DISJUG_RELATOR)  
                            VALUES (@Disjug_Dis_Id, @Disjug_Membro2);
                        END;

                         Update Protocolo_Distribuicao
                          set DIS_DESTINO_STATUS = 'JULGANDO'
                        where DIS_ID = @Disjug_Dis_Id



                    ";

            using (var connection = _context.CreateConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await connection.ExecuteAsync(query, dbParametro, transaction);
                        transaction.Commit(); // Commit se tudo ocorrer bem
                    }
                    catch
                    {
                        transaction.Rollback(); // Rollback em caso de erro
                        throw;
                    }
                }
            }
        }

        public async Task InserirVotoMembro(JulgamentoProcessoModel julgamentoProcesso)
        {

            //validando a model agendamento 
            var validator = new VotoMembroValidator();
            var result = validator.Validate(julgamentoProcesso);
            if (result.IsValid == false)
                throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
            //fim



            var dbParametro = new DynamicParameters();
            dbParametro.Add("@Disjug_Dis_Id", julgamentoProcesso.Disjug_Dis_Id);
            dbParametro.Add("@Disjug_Relator", julgamentoProcesso.Disjug_Relator);   
            dbParametro.Add("@Disjug_Resultado", julgamentoProcesso.Disjug_Resultado);     

            var query = @"

                    Declare @SetoroOrigem int, @qtd INT
                    Set @SetoroOrigem = (Select top 1 
						                SETSUBUSU_SETSUB_ID 
					                from SetorSubXUsuario 
					            where SETSUBUSU_USUARIO = @Disjug_Relator)


                    

                    Update Protocolo_Distribuicao_Julgamento
                       set DISJUG_RESULTADO = @Disjug_Resultado,
                           DISJUG_RESULTADO_DATA = Getdate()
                     where Disjug_Dis_Id = @Disjug_Dis_Id
                       and Disjug_Relator = @Disjug_Relator
                       and DISJUG_RESULTADO_DATA is null;

                    Set @qtd = (select Count(1) 
                                from Protocolo_Distribuicao_Julgamento 
                                where Disjug_Dis_Id = @Disjug_Dis_Id 
                                  and DISJUG_RESULTADO_DATA is not null 
                                  AND DISJUG_RESULTADO = @Disjug_Resultado);

                    If(@qtd = 3)
                    begin	   
                        Select MOVPRO_ID,
                               DIS_MOV_ID, 
                               MOVPRO_PRT_NUMERO,
                               DIS_DESTINO_USUARIO as MOVPRO_USUARIO_ORIGEM 
                        into #temp_julgado
                        from Movimentacao_Processo  
                        inner join Protocolo_Distribuicao on (MOVPRO_ID = DIS_MOV_ID) 
                        where DIS_ID = @Disjug_Dis_Id;

                        Update mp
                        set mp.MOVPRO_STATUS = 'JULGADO->HOMOLOGAR'
                        from Movimentacao_Processo mp, #temp_julgado tj
                        where mp.MOVPRO_ID = tj.DIS_MOV_ID;

                        Update Protocolo_Distribuicao
                        set DIS_DESTINO_STATUS = 'JULGADO'	
                        where DIS_ID = @Disjug_Dis_Id;

                        Update Protocolo
                        set PRT_ACAO = 'HOMOLOGAR',
                            PRT_DT_JULGAMENTO = GETDATE(),
                            PRT_RESULTADO = @Disjug_Resultado
                        from #temp_julgado
                        where PRT_NUMERO = MOVPRO_PRT_NUMERO;

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
                        select   
                               mp.MOVPRO_PRT_NUMERO,
                               @SetoroOrigem,
                               tj.MOVPRO_USUARIO_ORIGEM AS USERLOGADO,
                               GETDATE(),
                               NULL,
                               'Processo julgado e encaminhado para homologação',
                               48, --Setor SETAP porem poder ser alterado futuramente
                               'HOMOLOGAR',
                               NULL
                        from Movimentacao_Processo mp, #temp_julgado tj
                        where mp.MOVPRO_ID = tj.DIS_MOV_ID;
                    end;
                ";


            using (var connection = _context.CreateConnection())
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        await connection.ExecuteAsync(query, dbParametro, transaction);
                        transaction.Commit(); // Commit se tudo ocorrer bem
                    }
                    catch
                    {
                        transaction.Rollback(); // Rollback em caso de erro
                        throw;
                    }
                }
            }
        }

        public async Task<List<JulgamentoProcessoModel>> BuscarVotacao(int vlobusca)
        {
          
            var query = @"   select 
		                        DISJUG_RELATOR,
                                FORMAT(DISJUG_RESULTADO_DATA, 'dd/MM/yyyy HH:mm') AS DISJUG_RESULTADO_DATA,
		                        Case when DISJUG_RESULTADO = 'I' then 'INDEFERIDO'
		                          when DISJUG_RESULTADO IN('D','O') then 'DEFERIDO' 
		                       ELSE 'AGUARDANDO...' END AS DISJUG_RESULTADO,
		                        Case when DISJUG_PARECER_RELATORIO is null then 'MEMBRO' else 'PRESIDENTE' END AS DISJUG_TIPO
                         from Protocolo_Distribuicao_Julgamento
                         where DISJUG_DIS_ID = @vlobusca ";


            using (var connection = _context.CreateConnection())
            {
                var parametros = new { vlobusca };
                var command = await connection.QueryAsync<JulgamentoProcessoModel>(query, parametros);
                return command.ToList();
            }
          
        }

        public async Task<JulgamentoProcessoModel> VerificarVoto(string disjug_relator, int disjug_dis_id)
        {
           
            var query = @"SELECT DISJUG_ID,
                                 DISJUG_RESULTADO_DATA 
                            FROM Protocolo_Distribuicao_Julgamento 
                           WHERE DISJUG_RELATOR = @disjug_relator and 
                                 DISJUG_DIS_ID = @disjug_dis_id  ";

            using (var connection = _context.CreateConnection())
            {
                var parametros = new { disjug_relator, disjug_dis_id };
                var result = await connection.QueryFirstOrDefaultAsync<JulgamentoProcessoModel>(query, parametros);
                return result;
            }                                                                        
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

        public async Task<List<InstrucaoProcessoModel>> BuscarHistoricoInstrucao(string vlobusca)
        {
            
            var query = @"   
                           Select MOVPRO_USUARIO_ORIGEM as INSPRO_Usuario_origem,
                                  MOVPRO_PARECER_ORIGEM as INSPRO_Parecer,
                                  MOVPRO_DATA_ORIGEM as INSPRO_DATA_ORIGEM,
                                  '' as INSPRO_Arquivo,
                                  ssb.SETSUB_NOME AS INSPRO_Setor_origem ,
		                          ssa.SETSUB_NOME AS INSPRO_Setor_destino
                             from Movimentacao_Processo
			                      inner join SetorSub ssa on(MOVPRO_SETOR_DESTINO = ssa.SETSUB_ID)
			                      inner join SetorSub ssb on(MOVPRO_SETOR_ORIGEM = ssb.SETSUB_ID)
                            where Movpro_instrucao = 1 and 
                                  replace(MOVPRO_PRT_NUMERO,'/','') = @vlobusca 
                                  order by MOVPRO_ID";


            using (var connection = _context.CreateConnection())
            {
                var parametros = new { vlobusca };
                var command = await connection.QueryAsync<InstrucaoProcessoModel>(query, parametros);
                return command.ToList();
            }
           
        }


    }
}
