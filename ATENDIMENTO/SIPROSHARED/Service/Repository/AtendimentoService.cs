using Dapper;
using FluentValidation;
using SIPROSHARED.API;
using SIPROSHARED.DbContext;
using SIPROSHARED.Models;
using SIPROSHARED.Service.IRepository;
using SIPROSHARED.Validator;
using SIRPOEXCEPTIONS.ExceptionBase;
using System.Data;
using System.Globalization;

namespace SIPROSHARED.Service.Repository
{
    public class AtendimentoService: IAtendimentoService
    {
        private readonly DapperContext _context;

        public AtendimentoService(DapperContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

       
        public async Task<AgendaModel> GetAgendamento(string cpf)
        {

            //validação
            if (string.IsNullOrEmpty(cpf) || cpf.Length != 11)
            {
                throw new ErrorOnValidationException(new List<string> { "O CPF deve ter exatamente 11 caracteres." });
            }
         
            var query = @"  
             Select top 1
                     Age_Id   
                    ,Age_Dt_Programada       
                    ,Age_AIT              
                    ,Age_Doc_Solicitante 
                    ,Age_Cod_Assunto 
                    ,Age_Cod_Geral 
                    ,Age_Cod_Origem 
                    ,Age_Placa 
                    ,Age_Abertura
                    ,Ass_Nome
                    ,Ori_Descricao  
                    from Agendamento  
                inner join Assunto on(ASS_ID = Age_Cod_Assunto) 
                inner join Origem on (ORI_CODIGO = Age_Cod_Origem)	
                where Age_Doc_Solicitante = @cpf
                and IsNull(Age_Situacao,'') = '' 
                order by Age_Abertura desc";

            using var connection = _context.CreateConnection();
            var parametros = new { cpf };
            var command = await connection.QueryFirstOrDefaultAsync<AgendaModel>(query, parametros);

            //validando todos os campos obrigatórios!
            if (command != null)
            {
                var validator = new AgendamentoValidator();
                var result = validator.Validate(command);
                if (result.IsValid == false)
                    throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
            }

            return command;

        }

        public async Task UpdateAgendamento(string ait, int codservico, string situacao)
        {
          
            var dbParametro = new DynamicParameters();

            dbParametro.Add("Age_AIT", ait);
            dbParametro.Add("Age_Cod_Assunto", codservico);
            dbParametro.Add("Age_Situacao", situacao);        

            string query = @"
                           Update agendamento
                            set Age_Situacao = @Age_Situacao
                            where Age_AIT = @Age_AIT
                            and Age_Cod_Assunto = @Age_Cod_Assunto
                            and Age_Situacao is null";

            using (var connection = _context.CreateConnection())
            {
                await connection.ExecuteAsync(query, dbParametro);
            }               

        }

        public async Task<List<DocumentosModel>> BuscarDocumentos(int codservico)
        {
            if (codservico == 0)
                throw new ErrorOnValidationException(new List<string> { "O código do serviço é obrigatório." });


            var query = @"  
	                      	 Select DOC_ID, 
                                    DOC_NOME 
                               from VW_AssuntoXDocumento 
                              where ASSDOC_ASS_ID = @codservico 
                                and ASS_ATIVO = 1";

            using (var connection = _context.CreateConnection())
            {
                var parametros = new { codservico };
                var command = await connection.QueryAsync<DocumentosModel>(query, parametros);

                return command.ToList();
            }
            

        }       

        public async Task<int> DuplicidadeServico(string ait, int codservico)
        {

                var query = @" Select Count(1) as T                                                           
                                 From Protocolo 
                                where Prt_AIT = @ait
                                  and Prt_Assunto = @codservico  
                                  and isnull(Prt_Situacao,'') in ('','ARQUIVADO!')" ;                           

                using (var connection = _context.CreateConnection())
                {
                    var parametros = new { ait, codservico };
                    var count = await connection.ExecuteScalarAsync<int>(query, parametros);
                    return count;
                }
            
        }

        public async Task<bool> CadastrarAberturaAsync(AgendaModel agendaModel)
        {
        
                //validando a model agendamento 
                var validator = new AgendamentoValidator();
                var result = validator.Validate(agendaModel);
                if (result.IsValid == false)
                    throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
                //fim

                var dbParametro = new DynamicParameters();

                dbParametro.Add("@Age_Doc_Solicitante", agendaModel.Age_Doc_Solicitante);
                dbParametro.Add("@Age_AIT", agendaModel.Age_AIT);
                dbParametro.Add("@Age_Cod_Assunto", agendaModel.Age_Cod_Assunto);
                dbParametro.Add("@Age_Cod_Origem", agendaModel.Age_Cod_Origem);



                using (var connection = _context.CreateConnection())
                {                   

                    // Insere o novo registro
                    string insertQuery = @"
        

                        -- Deleta o registro caso já exista
                        DELETE FROM Agendamento 
                        WHERE Age_AIT = @Age_AIT 
                          AND Age_Cod_Assunto = @Age_Cod_Assunto;

                        -- Insere o novo registro
                        INSERT INTO Agendamento
                        (      
                            Age_Dt_Programada,
                            Age_AIT,
                            Age_Doc_Solicitante,
                            Age_Cod_Assunto,
                            Age_Cod_Geral,
                            Age_Cod_Origem,
                            Age_Abertura,
                            Age_Tipo
                        )
                        SELECT 
                            GETDATE(),       
                            @Age_AIT,              
                            @Age_Doc_Solicitante, 
                            @Age_Cod_Assunto, 
                            00, -- Coloque um comentário para explicar este valor
                            @Age_Cod_Origem, 
                            GETDATE(),
                            'Presencial';

";

                    await connection.ExecuteAsync(insertQuery, dbParametro);
                }

                // Retorne true se tudo deu certo
                return true;
            
        }

        public async Task<List<AnexoModel>> BuscarAnexos()
        {
            
                List<AnexoModel> anexoModel = new List<AnexoModel>();

                var query = @"
                              SELECT TOP 1 prt_imagem FROM Imagem_anexo";

                using (var connection = _context.CreateConnection())
                {
                    var imagensBytes = await connection.QueryAsync<byte[]>(query);

                    foreach (var imagemBytes in imagensBytes)
                    {
                        // Converte os bytes da imagem para uma string base64
                        string imagemBase64 = Convert.ToBase64String(imagemBytes);

                        // Cria uma nova instância de AnexoModel
                        var anexo = new AnexoModel();

                        // Formata a string base64 para ser usada em uma tag <img> no HTML e atribui ao caminhoA
                        anexo.caminhosrc = $"<img src='data:image/jpeg;base64,{imagemBase64}' alt='Imagem' style=\"width: 100%; height: 150px;\">";

                        // Atribui o caminhoB
                        anexo.caminhohref = $"data:image/jpeg;base64,{imagemBase64}";

                        // Adiciona o objeto AnexoModel à lista
                        anexoModel.Add(anexo);
                    }

                    return anexoModel;
                }
            
        }

        public async Task<List<AnexoModel>> BuscarAnexosTemp(string folderPath)
        {
            // Lista de nomes dos arquivos na pasta
            string[] fileNames = Directory.GetFiles(folderPath);
            string Nome = "";
            // Lista para armazenar os objetos AnexoModel
            List<AnexoModel> anexoModel = new List<AnexoModel>();

            if (fileNames.Length > 0)
            {
                using (var connection = _context.CreateConnection())
                {
                    // Abre a conexão
                    connection.Open();

                    // Criação da tabela temporária
                    string createTableQuery = @" CREATE TABLE #Imagem_anexo (
                                                prt_imagem VARBINARY(MAX),
                                                prt_nome varchar(30)
                                                )";

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = createTableQuery;
                         command.ExecuteNonQuery();
                    }


                    //Lendo e inserindo as img na temp
                    foreach (string fileName in fileNames)
                    {
                        // Lê o arquivo como um array de bytes
                        byte[] imagemBytes = File.ReadAllBytes(fileName);
                         Nome = Path.GetFileNameWithoutExtension(fileName);

                        string insertQuery = @" INSERT INTO #Imagem_anexo 
                                                (prt_imagem,
                                                 prt_nome)
                                                VALUES 
                                                (@Imagem,
                                                 @Nome)";

                        using (var insertCommand = connection.CreateCommand())
                        {
                            insertCommand.CommandText = insertQuery;

                            // Criar e adicionar parâmetro para imagem
                            var imagemParametro = insertCommand.CreateParameter();
                            imagemParametro.ParameterName = "@Imagem";
                            imagemParametro.Value = imagemBytes;
                            insertCommand.Parameters.Add(imagemParametro);

                            // Criar e adicionar parâmetro para nome
                            var nomeParametro = insertCommand.CreateParameter();
                            nomeParametro.ParameterName = "@Nome";
                            nomeParametro.Value = Nome;
                            insertCommand.Parameters.Add(nomeParametro);

                            // Insere a imagem na tabela temporária
                            insertCommand.ExecuteNonQuery();
                        }
                    }

                  
                    // Agora, você pode recuperar as imagens da tabela temporária
                    string selectQuery = @"SELECT prt_imagem, prt_nome FROM #Imagem_anexo";

                    using (var selectCommand = connection.CreateCommand())
                    {
                        selectCommand.CommandText = selectQuery;

                        // Executa a consulta SELECT
                        using (var reader =  selectCommand.ExecuteReader())
                        {
                            while ( reader.Read())
                            {
                                // Lê os bytes da imagem
                                byte[] imagemBytes = (byte[])reader["prt_imagem"];

                                // Converte os bytes da imagem para uma string base64
                                string imagemBase64 = Convert.ToBase64String(imagemBytes);

                                string nomeArquivo = reader["prt_nome"].ToString();

                                // Cria uma nova instância de AnexoModel
                                var anexo = new AnexoModel();

                                // Obter o nome do arquivo correspondente e atribuir ao anexo                        
                                anexo.nome = nomeArquivo;

                                // Formata a string base64 para ser usada em uma tag <img> no HTML e atribui ao caminhoA
                                anexo.caminhosrc = $"<img src='data:image/jpeg;base64,{imagemBase64}' alt='Imagem' style=\"width: 100%; height: 150px;\">";

                                // Atribui o caminhoB
                                anexo.caminhohref = $"data:image/jpeg;base64,{imagemBase64}";

                                // Adiciona o objeto AnexoModel à lista
                                anexoModel.Add(anexo);
                            }
                        }
                    }
                }
            }
            else
            {
                
                throw new ErrorOnValidationException(new List<string> { "Não há arquivos na pasta." });
            }

            return anexoModel;
        }

        public async Task<string> RealizarAbertura(ProtocoloModel dadosMulta, IDbConnection connection, IDbTransaction transaction)
        {
           
                string? protocoloNumero = null;

                var validator = new ProtocoloValidator();
                var result = validator.Validate(dadosMulta);
                if (result.IsValid == false)
                    throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());



                var dbParametro = new DynamicParameters();
                dbParametro.Add("PRT_NUMERO", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);
                dbParametro.Add("PRT_ORIGEM", dadosMulta.PRT_ORIGEM);
                dbParametro.Add("PRT_ASSUNTO ", dadosMulta.PRT_ASSUNTO);
                dbParametro.Add("PRT_ATENDENTE", dadosMulta.PRT_ATENDENTE);
                dbParametro.Add("PRT_OBSERVACAO", dadosMulta.PRT_OBSERVACAO);
                dbParametro.Add("PRT_AIT", dadosMulta.PRT_AIT);
                dbParametro.Add("PRT_TIPO_SOLICITANTE", dadosMulta.PRT_TIPO_SOLICITANTE);
                dbParametro.Add("PRT_CPF_SOLICITANTE", dadosMulta.PRT_CPF_SOLICITANTE);
                dbParametro.Add("PRT_CPFCNJ_PROPRIETARIO", dadosMulta.PRT_CPFCNJ_PROPRIETARIO);
                dbParametro.Add("PRT_NOMEPROPRIETARIO", dadosMulta.PRT_NOMEPROPRIETARIO);
                dbParametro.Add("PRT_CPF_CONDUTOR", dadosMulta.PRT_CPF_CONDUTOR);
                dbParametro.Add("PRT_NUMREGISTRO_CNH", dadosMulta.PRT_NUMREGISTRO_CNH);
                dbParametro.Add("PRT_CNH_VALIDADE", dadosMulta.PRT_DT_VALIDADE);
                dbParametro.Add("PRT_PLACA", dadosMulta.PRT_PLACA);


                dbParametro.Add("PRT_RESS_BANCO", dadosMulta.PRT_RESS_BANCO);
                dbParametro.Add("PRT_RESS_TIPO", dadosMulta.PRT_RESS_TIPO);
                dbParametro.Add("PRT_RESS_TITULAR", dadosMulta.PRT_RESS_TITULAR);
                dbParametro.Add("PRT_RESS_CPF", dadosMulta.PRT_RESS_CPF);
                dbParametro.Add("PRT_RESS_AGENCIA", dadosMulta.PRT_RESS_AGENCIA);
                dbParametro.Add("PRT_RESS_CONTA", dadosMulta.PRT_RESS_CONTA);
                dbParametro.Add("PRT_RESS_OPERACAO", dadosMulta.PRT_RESS_OPERACAO);

                dbParametro.Add("PRT_RESTRICAO", dadosMulta.PRT_RESTRICAO);
                dbParametro.Add("PRT_AIT_SITUACAO", dadosMulta.PRT_AIT_SITUACAO);

                dbParametro.Add("PRT_DT_ARQUIVO", dadosMulta.PRT_DT_ARQUIVO);
                dbParametro.Add("PRT_USUARIOARQUIVO", dadosMulta.PRT_USUARIOARQUIVO);

                dbParametro.Add("PRT_CNH_ESTRANGEIRA", dadosMulta.PRT_CNH_ESTRANGEIRA);
                dbParametro.Add("PRT_CNH_ESTRANGEIRA_NOME", dadosMulta.PRT_CNH_ESTRANGEIRA_NOME);
                dbParametro.Add("PRT_CNH_ESTRANGEIRA_PAIS", dadosMulta.PRT_CNH_ESTRANGEIRA_PAIS);

                dbParametro.Add("PRT_DT_POSTAGEM", dadosMulta.PRT_DT_POSTAGEM);
                dbParametro.Add("PRT_NUMERO_GEP", dadosMulta.PRT_NUMERO_GEP);

                string query = @"

                            EXEC Sp_GerarNumeroProcessoProtocolo @PRT_NUMERO OUT;

                            IF NOT EXISTS (
                                SELECT 1 FROM Protocolo 
                                WHERE PRT_ASSUNTO = @PRT_ASSUNTO 
                                AND PRT_AIT = @PRT_AIT
                            )
                            BEGIN
                                INSERT INTO Protocolo (
                                    PRT_NUMERO, PRT_ORIGEM, PRT_ASSUNTO, PRT_ASSUNTOTIPO, PRT_DT_ABERTURA, PRT_ATENDENTE, PRT_OBSERVACAO, 
                                    PRT_AIT, PRT_CPF_SOLICITANTE, PRT_CPFCNJ_PROPRIETARIO, PRT_NOMEPROPRIETARIO, PRT_CPF_CONDUTOR, 
                                    PRT_NUMREGISTRO_CNH, PRT_DT_CADASTRO, PRT_TIPO_SOLICITANTE, PRT_CNH_VALIDADE, PRT_PLACA, 
                                    PRT_RESS_BANCO, PRT_RESS_TIPO, PRT_RESS_TITULAR, PRT_RESS_CPF, PRT_RESS_AGENCIA, 
                                    PRT_RESS_CONTA, PRT_RESS_OPERACAO, PRT_DT_POSTAGEM, PRT_RESTRICAO, PRT_CNH_ESTRANGEIRA, 
                                    PRT_CNH_ESTRANGEIRA_NOME, PRT_CNH_ESTRANGEIRA_PAIS, PRT_NUMERO_GEP, PRT_AIT_SITUACAO, 
                                    PRT_SITUACAO, PRT_ACAO, PRT_DT_ARQUIVO, PRT_USUARIOARQUIVO
                                )
                                VALUES (
                                    @PRT_NUMERO, @PRT_ORIGEM, @PRT_ASSUNTO, 1, GETDATE(), @PRT_ATENDENTE, @PRT_OBSERVACAO, 
                                    @PRT_AIT, @PRT_CPF_SOLICITANTE, @PRT_CPFCNJ_PROPRIETARIO, @PRT_NOMEPROPRIETARIO, @PRT_CPF_CONDUTOR, 
                                    @PRT_NUMREGISTRO_CNH, GETDATE(), @PRT_TIPO_SOLICITANTE, TRY_CONVERT(datetime, @PRT_CNH_VALIDADE, 103), 
                                    @PRT_PLACA, @PRT_RESS_BANCO, @PRT_RESS_TIPO, @PRT_RESS_TITULAR, @PRT_RESS_CPF, @PRT_RESS_AGENCIA, 
                                    @PRT_RESS_CONTA, @PRT_RESS_OPERACAO, @PRT_DT_POSTAGEM, @PRT_RESTRICAO, @PRT_CNH_ESTRANGEIRA, 
                                    @PRT_CNH_ESTRANGEIRA_NOME, @PRT_CNH_ESTRANGEIRA_PAIS, @PRT_NUMERO_GEP, @PRT_AIT_SITUACAO,
                                    CASE WHEN @PRT_RESTRICAO NOT IN (1,5) THEN 'ARQUIVADO!'END,
                                    CASE WHEN @PRT_RESTRICAO NOT IN (1,5) THEN 'ARQUIVADO!' END,
                                    CASE WHEN @PRT_RESTRICAO NOT IN (1,5) THEN GETDATE() ELSE NULL END,
                                    CASE WHEN @PRT_RESTRICAO NOT IN (1,5) THEN @PRT_USUARIOARQUIVO ELSE NULL END
                                );

                                -- Dando baixa no agendamento
                                UPDATE agendamento
                                   SET Age_Situacao = 'Atendido'
                                 WHERE Age_AIT = @PRT_AIT
                                   AND Age_Cod_Assunto = @PRT_ASSUNTO
                                   AND Age_Doc_Solicitante = @PRT_CPF_SOLICITANTE;

                                
                              -- Adicionando movimentação
                                INSERT INTO Movimentacao_Processo (
                                       MOVPRO_PRT_NUMERO, 
									   MOVPRO_USUARIO_ORIGEM, 
									   MOVPRO_SETOR_ORIGEM, 
                                       MOVPRO_ACAO_ORIGEM, 
									   MOVPRO_DATA_ORIGEM, 
									   MOVPRO_SETOR_DESTINO, 
                                       MOVPRO_PRTDOC_ID, 
									   MOVPRO_STATUS
                                )
                                VALUES (
                                    @PRT_NUMERO, 
									@PRT_ATENDENTE,
									49,
                                    'Abertura de processo realizada com sucesso.', 
									GETDATE(), 
                                    109,
									NULL, 
									'ABERTURA'
                                );

                            END;

                    
                        ";



                await connection.ExecuteAsync(query, dbParametro, transaction);
                protocoloNumero = dbParametro.Get<string>("PRT_NUMERO");

                return protocoloNumero;
           


            
        }

        public async Task IntoAnexoFolder(string folderPath, ProtocoloDocImgModel protocoloDoc, IDbConnection connection, IDbTransaction transaction)
        {
           
                // Lista de nomes dos arquivos na pasta
                string[] filePaths = Directory.GetFiles(folderPath);

                // Verifica se há arquivos na pasta
                if (filePaths.Length > 0)
                {
                    //using (var connection = _context.CreateConnection())
                    //{
                    foreach (string filePath in filePaths)
                    {
                        string fileName = Path.GetFileName(filePath);

                        // Lê o arquivo como um array de bytes
                        byte[] imagemBytes = File.ReadAllBytes(filePath);

                        var dbParametro = new DynamicParameters();
                        dbParametro.Add("@PRTDOC_DOC_ID", 0);
                        dbParametro.Add("@PRTDOC_PRT_NUMERO", protocoloDoc.PRTDOC_PRT_NUMERO);
                        dbParametro.Add("@PRTDOC_IMAGEM", imagemBytes);
                        dbParametro.Add("@PRTDOC_OBSERVACAO", fileName);
                        dbParametro.Add("@PRTDOC_PRT_AIT", protocoloDoc.PRTDOC_PRT_AIT);
                        dbParametro.Add("@PRTDOC_PRT_SETOR", protocoloDoc.PRTDOC_PRT_SETOR);

                        string query = @"
                       
                       Declare @PRTDOC_MOVPRO_ID int
                       Set @PRTDOC_MOVPRO_ID = (Select top 1 MOVPRO_ID from Movimentacao_Processo where MovPro_Prt_numero = @PRTDOC_PRT_NUMERO order by MOVPRO_ID desc)

                       INSERT INTO Protocolo_Documento_Imagem
                                  ( PRTDOC_DOC_ID, 
                                    PRTDOC_PRT_NUMERO, 
                                    PRTDOC_IMAGEM, 
                                    PRTDOC_OBSERVACAO, 
                                    PRTDOC_DATA_HORA, 
                                    PRTDOC_PRT_AIT, 
                                    PRTDOC_PRT_SETOR, 
                                    PRTDOC_MOVPRO_ID )
                             SELECT 
                                    @PRTDOC_DOC_ID, 
                                    @PRTDOC_PRT_NUMERO, 
                                    @PRTDOC_IMAGEM, 
                                    @PRTDOC_OBSERVACAO, 
                                    GETDATE(), 
                                    @PRTDOC_PRT_AIT, 
                                    @PRTDOC_PRT_SETOR, 
                                    @PRTDOC_MOVPRO_ID
                                ";
                        await connection.ExecuteAsync(query, dbParametro, transaction);
                    }
                }
                else
                {
                    // Não há arquivos na pasta
                    Console.WriteLine("Não há arquivos na pasta.");
                }
            
           
        }

        public async Task<ProtocoloModel> BuscarProtocolo(string PRT_NUMERO)
        {
            
                var query = @" 
                 Select 
	                    PRT_NUMERO
                       ,PRT_ORIGEM
                       ,ORI_DESCRICAO as PRT_NOME_ORIGEM 
                       ,PRT_DT_CADASTRO
                       ,PRT_DT_ABERTURA 
	                   ,PRT_DT_POSTAGEM
                       ,PRT_ASSUNTO
                       ,Ass_Nome AS PRT_NOME_ASSUNTO
                       ,PRT_TIPO_SOLICITANTE
                       ,PRT_CPF_SOLICITANTE
                       ,S.PES_NOME AS PRT_NOME_SOLICITANTE
                       ,PRT_NUMERO_GEP
                       ,PRT_ATENDENTE
                       ,PRT_OBSERVACAO
                       ,PRT_AIT 
                       ,PRT_AIT_SITUACAO
                       ,CASE WHEN PRT_SITUACAO is null THEN 'ANDAMENTO' else PRT_SITUACAO end PRT_SITUACAO 
                       ,CASE WHEN PRT_ACAO is null THEN 'ANDAMENTO' else PRT_ACAO end PRT_ACAO
                       ,'' as PRT_VEICULO_PLACA
                       ,PRT_DT_COMETIMENTO
                       ,'' as PRT_DT_PRAZO        
                       ,PRT_CPFCNJ_PROPRIETARIO
                       ,PRT_NOMEPROPRIETARIO
                       ,'' as PRT_ENDERECO_PROPRIETARIO
                       ,'' as PRT_CIDADE_PROPRIETARIO       
                       ,PRT_CPF_CONDUTOR
                       ,case when c.PES_Nome IS NULL then 'NÃO SE APLICA'  else c.PES_Nome end as PRT_NOME_CONDUTOR
                       ,PRT_NUMREGISTRO_CNH
                       ,PRT_CNH_VALIDADE as PRT_DT_VALIDADE
                       ,UPPER(c.PES_UFCNH) AS PRT_UF_CNH
                       ,PRT_RESTRICAO 
                       ,RES_NOME as PRT_RESTRICAO_NOME

                       ,PRT_DT_JULGAMENTO
                       ,case when PRT_RESULTADO = 'D' then 'Deferido'  when PRT_RESULTADO = 'I' then 'Indeferido'  else 'Aguardando' end as PRT_RESULTADO 
                       ,PRT_DT_PUBLICACAO
                       ,PRT_LOTE

                       ,PRT_RESS_BANCO
                       ,PRT_RESS_TIPO
                       ,PRT_RESS_TITULAR
                       ,PRT_RESS_CPF
                       ,PRT_RESS_AGENCIA
                       ,PRT_RESS_CONTA
                       ,PRT_RESS_OPERACAO       
                       ,PRT_CNH_ESTRANGEIRA
                       ,PRT_CNH_ESTRANGEIRA_NOME
                       ,convert(varchar(10),isnull(PRT_DT_POSTAGEM, PRT_DT_ABERTURA),103) as PRT_DT_ABERTURA
                       ,PRT_CNH_ESTRANGEIRA_PAIS 
                        FROM  Protocolo inner join Origem O on(PRT_ORIGEM = O.ORI_CODIGO)
										inner join Assunto A on (PRT_ASSUNTO = A.ASS_ID)
                                        inner join TipoRestricao on(PRT_RESTRICAO = RES_ID)
										Left join Pessoa as S on(PRT_CPF_SOLICITANTE = S.PES_CPF)										
									    Left join Pessoa as C on(PRT_CPF_CONDUTOR = C.PES_CPF)
	                         	 WHERE Prt_Numero = @PRT_NUMERO
                                order by PRT_DT_CADASTRO
                        
                             ";

                using (var connection = _context.CreateConnection())
                {
                    var parametros = new { PRT_NUMERO };
                    var command = await connection.QueryFirstOrDefaultAsync<ProtocoloModel>(query, parametros);
                    return command;
                }
           
            
        }

        public async Task<List<ProtocoloModel>> BuscarProtocoloAll(string vloBusca)
        {
              var query = @" 
                 Select  
	                    PRT_NUMERO
                       ,PRT_ORIGEM
                       ,ORI_DESCRICAO as PRT_NOME_ORIGEM 
                       ,PRT_DT_CADASTRO
                       ,PRT_DT_ABERTURA 
	                   ,PRT_DT_POSTAGEM
                       ,PRT_ASSUNTO
                       ,Ass_Nome AS PRT_NOME_ASSUNTO
                       ,PRT_TIPO_SOLICITANTE
                       ,PRT_CPF_SOLICITANTE
                       ,S.PES_NOME AS PRT_NOME_SOLICITANTE
                       ,PRT_NUMERO_GEP
                       ,PRT_ATENDENTE
                       ,PRT_OBSERVACAO
                       ,PRT_AIT 
                       ,PRT_AIT_SITUACAO
                       ,CASE WHEN PRT_ACAO is null THEN 'ANDAMENTO' else PRT_ACAO end PRT_ACAO
                       ,'' as PRT_VEICULO_PLACA
                       ,PRT_DT_COMETIMENTO
                       ,'' as PRT_DT_PRAZO        
                       ,PRT_CPFCNJ_PROPRIETARIO
                       ,PRT_NOMEPROPRIETARIO
                       ,'' as PRT_ENDERECO_PROPRIETARIO
                       ,'' as PRT_CIDADE_PROPRIETARIO       
                       ,PRT_CPF_CONDUTOR
                       ,case when c.PES_Nome IS NULL then 'NÃO SE APLICA'  else c.PES_Nome end as PRT_NOME_CONDUTOR
                       ,PRT_NUMREGISTRO_CNH
                       ,PRT_CNH_VALIDADE as PRT_DT_VALIDADE
                       ,UPPER(c.PES_UFCNH) AS PRT_UF_CNH
                     

                       ,PRT_DT_JULGAMENTO
                       ,PRT_RESULTADO
                       ,PRT_DT_PUBLICACAO
                       ,PRT_LOTE

                       ,PRT_RESTRICAO 
                       ,RES_NOME as PRT_RESTRICAO_NOME
                       ,PRT_RESS_BANCO
                       ,PRT_RESS_TIPO
                       ,PRT_RESS_TITULAR
                       ,PRT_RESS_CPF
                       ,PRT_RESS_AGENCIA
                       ,PRT_RESS_CONTA
                       ,PRT_RESS_OPERACAO       
                       ,PRT_CNH_ESTRANGEIRA
                       ,PRT_CNH_ESTRANGEIRA_NOME
                       ,convert(varchar(10),isnull(PRT_DT_POSTAGEM, PRT_DT_ABERTURA),103) as PRT_DT_ABERTURA
                       ,PRT_CNH_ESTRANGEIRA_PAIS 
                   FROM Protocolo inner join Origem O on(PRT_ORIGEM = O.ORI_CODIGO)
								  inner join Assunto A on (PRT_ASSUNTO = A.ASS_ID)
                                  inner join TipoRestricao on(PRT_RESTRICAO = RES_ID)
								  left join Pessoa as S on(PRT_CPF_SOLICITANTE = S.PES_CPF)										
							      left join Pessoa as C on(PRT_CPF_CONDUTOR = C.PES_CPF)
	                         	 WHERE (Prt_Numero  = @vloBusca or Prt_AIT = @vloBusca  )
                                 order by PRT_DT_CADASTRO
                             ";

                using (var connection = _context.CreateConnection())
                {
                    var parametros = new { vloBusca };
                    var command = await connection.QueryAsync<ProtocoloModel>(query, parametros);
                    return command.ToList();
                }
            
           
        }

        public async Task<List<MovimentacaoModel>> BuscarMovimentacao(string vloBusca)
        {
            
                var query = @" SELECT  
                                     
		                       MOVPRO_PRT_NUMERO       
		                      ,MOVPRO_USUARIO_ORIGEM   
		                      ,O.SETSUB_NOME AS MOVPRO_SETOR_ORIGEM     
		                      ,MOVPRO_ACAO_ORIGEM                                                                                                                                                                                                                                                    
		                      ,CONVERT(VARCHAR(10),MOVPRO_DATA_ORIGEM,103) as MOVPRO_DATA_ORIGEM
                              ,CONVERT(Date,MOVPRO_DATA_ORIGEM,103)
		                      ,Case when MOVPRO_SETOR_DESTINO = 0 then 'Tramitação Automática' else D.SETSUB_NOME end AS MOVPRO_SETOR_DESTINO  
		                      ,MOVPRO_PRTDOC_ID
                          FROM Movimentacao_Processo inner join SetorSub O on (MOVPRO_SETOR_ORIGEM = O.SETSUB_ID)
												     inner join SetorSub D on (MOVPRO_SETOR_DESTINO = D.SETSUB_ID)

                         WHERE MOVPRO_PRT_NUMERO = @vloBusca and MOVPRO_PRTDOC_ID is null
                         order by MOVPRO_ID asc  ";
                        

                using (var connection = _context.CreateConnection())
                {
                    var parametros = new { vloBusca };
                    var command = await connection.QueryAsync<MovimentacaoModel>(query, parametros);
                    return command.ToList();
                }
            
        }




       
    }
}

