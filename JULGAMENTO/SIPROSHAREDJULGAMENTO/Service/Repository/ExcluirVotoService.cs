using Dapper;
using Polly;
using SIPROSHARED.DbContext;
using SIPROSHAREDJULGAMENTO.Models;
using SIPROSHAREDJULGAMENTO.Service.IRepository;
using SIPROSHAREDJULGAMENTO.Validator;
using SIRPOEXCEPTIONS.ExceptionBase;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDJULGAMENTO.Service.Repository
{
    public class ExcluirVotoService : IExcluirVotoService
    {
        private readonly DapperContext _context;

        public ExcluirVotoService(DapperContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }


        /*Excluir Voto*/
        public async Task<List<ExcluirModel>> LocalizarProcessosExcluirVoto(string usuario, string situacao, string processo)
        {
            if  (string.IsNullOrEmpty(processo)  || string.IsNullOrEmpty(situacao) || string.IsNullOrEmpty(processo))
                throw new ErrorOnValidationException(new List<string> { "O valor do parametro não foi passado para realizar a busca." });


            try
            {
                var query = @"
							 
	                 Declare @Setor int

					 Set @Setor = (Select top 1 
                                            SETSUBUSU_SETSUB_ID 
			  		                                from SetorSubXUsuario 
								                            where SETSUBUSU_USUARIO = @usuario)

                        Select MOVPRO_PRT_NUMERO as PrtNumero,  
                                DIS_DESTINO_USUARIO as PrtRelator,
			                    ASS_NOME  as PrtAssunto,
	                            PRT_ACAO as PrtAcao
                    from Protocolo_distribuicao as a   
                    join Movimentacao_Processo as b on (a.DIS_MOV_ID = b.MOVPRO_ID)  
                    join Protocolo as c on (c.PRT_NUMERO  = b.MOVPRO_PRT_NUMERO)     
		            join Assunto as d on (d.ASS_ID = PRT_ASSUNTO)
                    Where PRT_ACAO like case when @situacao = 'TODOS' then '%' else @situacao end and 
                            REPLACE(MOVPRO_PRT_NUMERO,'/','') like case when @processo = 'TODOS' then '%' else @processo end and  
                            MOVPRO_SETOR_ORIGEM = @Setor AND
                            PRT_ACAO in ('EM JULGAMENTO', 'HOMOLOGAR')
                                order by MOVPRO_PRT_NUMERO 
							";

                using (var connection = _context.CreateConnection())
                {
                    var parametros = new { usuario, situacao, processo };
                    var processos = await connection.QueryAsync<ExcluirModel>(query, parametros);

                    return processos.ToList();
                }
            }
            catch (Exception ex)
            {

                throw;
            }



        }

        public async Task<ExcluirDetalheModel> BuscarParecer(string processo)
        {

            if (string.IsNullOrEmpty(processo))
                throw new ErrorOnValidationException(new List<string> { "O valor do parametro não foi fornecido para realizar busca." });

            var query = @"  select top 1 
                                    MovPro_id,
                                    MovPro_Prt_Numero,
                                    Disjug_Parecer_Relatorio
                                from Protocolo_Distribuicao_Julgamento 
                        inner join Protocolo_Distribuicao on(DISJUG_DIS_ID = DIS_ID)
	                    inner join Movimentacao_Processo on (DIS_MOV_ID = MOVPRO_ID) 
                                where Replace(movpro_prt_numero,'/','') = @processo
	                            and Disjug_Parecer_Relatorio is not null"
            ;


            using (var connection = _context.CreateConnection())
            {
                var parametros = new { processo };
                var command = await connection.QueryFirstOrDefaultAsync<ExcluirDetalheModel>(query, parametros);
                return command;
            }

        }

        public async Task<List<ExcluirDetalheModel>> BuscarVotacao(string processo)
        {      
            if (string.IsNullOrEmpty(processo))
                throw new ErrorOnValidationException(new List<string> { "O valor do parametro não foi fornecido para realizar busca." });

            var query = @"SELECT 
                                MOVPRO_ID, 
		                        DISJUG_RELATOR,
                                FORMAT(DISJUG_RESULTADO_DATA, 'dd/MM/yyyy HH:mm') AS DISJUG_RESULTADO_DATA,
		                        Case when DISJUG_RESULTADO = 'I' then 'INDEFERIDO'
		                        when DISJUG_RESULTADO IN('D','O') then 'DEFERIDO' 
		                        ELSE 'AGUARDANDO...' END AS DISJUG_RESULTADO,	                  
	                            PRT_ACAO as MovPro_Acao,
		                        Case when DISJUG_PARECER_RELATORIO is null then 'MEMBRO' else 'RELATOR' END AS DISJUG_TIPO
                          FROM  Protocolo_Distribuicao_Julgamento 
                                inner join Protocolo_Distribuicao on(DISJUG_DIS_ID = DIS_ID)
	                            inner join Movimentacao_Processo on (DIS_MOV_ID = MOVPRO_ID) 
		                        inner join Protocolo on (MOVPRO_PRT_NUMERO = PRT_NUMERO)
                          WHERE replace(movpro_prt_numero,'/','') = @processo";


            using (var connection = _context.CreateConnection())
            {
                var parametros = new { processo };
                var command = await connection.QueryAsync<ExcluirDetalheModel>(query, parametros);
                return command.ToList();
            }

        }

        public async Task ExcluirVoto(ExcluirDetalheModel excluirModel, IDbConnection connection, IDbTransaction transaction)
        {
             //validando a model agendamento 
            var validator = new ExcluirVotoValidator();
            var result = validator.Validate(excluirModel);
            if (result.IsValid == false)
                throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
            //fim


            var dbParametro = new DynamicParameters();
            dbParametro.Add("@MOVPRO_ID", excluirModel.MovPro_id);
            dbParametro.Add("@MOVPRO_PARECER_ORIGEM", "");
            dbParametro.Add("@MOVPRO_USUARIO_ORIGEM", excluirModel.Disjul_Usuario);


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
                  
	                       DELETE M 
		                     FROM Movimentacao_Processo M inner join #Processo P on( M.MOVPRO_PRT_NUMERO = P.MOVPRO_PRT_NUMERO)
                            WHERE MOVPRO_STATUS = 'HOMOLOGAR'

		                   UPDATE MP
                              SET MP.MOVPRO_STATUS = 'DISTRIBUIDO'
                             FROM Movimentacao_Processo MP INNER JOIN #Processo P ON ( MP.MOVPRO_ID = P.DIS_MOV_ID)
  

                          UPDATE PD
                             SET DIS_DESTINO_STATUS = 'RECEBIDO',
			                     DIS_RETORNO_OBS = @MOVPRO_PARECER_ORIGEM
                            FROM Protocolo_Distribuicao PD INNER JOIN #Processo P ON (pd.DIS_MOV_ID = p.DIS_MOV_ID)
                     

                          UPDATE P
                             SET PRT_ACAO = NULL,  
	                             PRT_DT_JULGAMENTO = NULL,  
	                             PRT_RESULTADO = NULL,  
                                 PRT_DT_HOMOLOGACAO = NULL  
                            FROM Protocolo P INNER JOIN  #Processo ON (P.PRT_NUMERO = MOVPRO_PRT_NUMERO)
                 


                    -- Limpando a tabela temporária
                        DROP TABLE #Processo;

                ";
            await connection.ExecuteAsync(query, dbParametro, transaction);


        }
    }
}
