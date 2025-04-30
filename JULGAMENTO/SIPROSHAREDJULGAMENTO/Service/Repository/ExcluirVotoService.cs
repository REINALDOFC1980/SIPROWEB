using Dapper;
using Polly;
using SIPROSHARED.DbContext;
using SIPROSHAREDJULGAMENTO.Models;
using SIPROSHAREDJULGAMENTO.Service.IRepository;
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

        public async Task<ExcluirDetalheModel> BuscarParecer(string processo)
        {

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
                var command = await connection.QueryAsync<ExcluirDetalheModel>(query, parametros);
                return command.ToList();
            }

        }

        public async Task ExcluirVoto(ExcluirDetalheModel excluirModel, IDbConnection connection, IDbTransaction transaction)
        {
            ////validando a model agendamento 
            //var validator = new RetificacaoValidator();
            //var result = validator.Validate(retificacaoModel);
            //if (result.IsValid == false)
            //    throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
            ////fim


            //var dbParametro = new DynamicParameters();
            //dbParametro.Add("@MOVPRO_ID", retificacaoModel.MOVPRO_ID);
            //dbParametro.Add("@MOVPRO_PARECER_ORIGEM", retificacaoModel.MOVPRO_PARECER_ORIGEM);
            //dbParametro.Add("@MOVPRO_USUARIO_ORIGEM", retificacaoModel.MOVPRO_USUARIO_ORIGEM);


            //var query = @"
		          //   DECLARE @SetorOrigem int

			         //    SET @SetorOrigem = (Select top 1 
			 		      //                          SETSUBUSU_SETSUB_ID 
								    //           from SetorSubXUsuario 
							     //              where SETSUBUSU_USUARIO = @MOVPRO_USUARIO_ORIGEM)
	             

            //        SELECT TOP 1 DIS_ID, DIS_MOV_ID, 
			         //            MOVPRO_SETOR_ORIGEM AS SetorDestino, 
			         //            MOVPRO_PRT_NUMERO 
            //                INTO #Processo
            //                FROM Movimentacao_Processo 
            //          INNER JOIN Protocolo_Distribuicao ON (MovPro_id = DIS_MOV_ID)
            //               WHERE MOVPRO_PRT_NUMERO = ( SELECT TOP 1 Movpro_prt_numero 
					       // 		                    FROM Movimentacao_Processo 
							     //                      WHERE MovPro_id = @MOVPRO_ID)

            //              DELETE PD
            //                FROM Protocolo_Distribuicao_Julgamento pd INNER JOIN #Processo ap ON (pd.DISJUG_DIS_ID = ap.DIS_ID)

            //              UPDATE MP
            //                 SET MOVPRO_STATUS = 'HOMOLOGADO->DEVOLVIDO'
            //                FROM Movimentacao_Processo MP where MOVPRO_ID = @MOVPRO_ID


            //            --Mudando o status dos processo distribuido
            //              UPDATE PD
            //                 SET DIS_DESTINO_STATUS = 'RECEBIDO',
			         //            DIS_RETORNO_OBS = @MOVPRO_PARECER_ORIGEM
            //                FROM Protocolo_Distribuicao PD INNER JOIN #Processo P ON (pd.DIS_MOV_ID = p.DIS_MOV_ID)
                     

            //              UPDATE P
            //                 SET PRT_ACAO = NULL,  
	           //                  PRT_DT_JULGAMENTO = NULL,  
	           //                  PRT_RESULTADO = NULL,  
            //                     PRT_DT_HOMOLOGACAO = NULL  
            //                FROM Protocolo P INNER JOIN  #Processo  ON (P.PRT_NUMERO = MOVPRO_PRT_NUMERO)
                 

            //        -- Inserindo novo registro na Movimentacao_Processo
            //              INSERT INTO Movimentacao_Processo
            //              SELECT MOVPRO_PRT_NUMERO,
            //                     @SetorOrigem,
            //                     @MOVPRO_USUARIO_ORIGEM, 
            //                     GETDATE(),
            //                     @MOVPRO_PARECER_ORIGEM, -- Observação ou parecer!
            //                    'Processo devolvido para o Relator.',
            //                     SetorDestino,
            //                    'RECEBIDO->DISTRIBUIDO',
            //                     NULL,
            //                     NULL
            //               FROM #Processo;

            //        -- Limpando a tabela temporária
            //            DROP TABLE #Processo;

            //    ";
            //await connection.ExecuteAsync(query, dbParametro, transaction);


        }
    }
}
