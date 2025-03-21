
  

insert into ContaCorrenteEmpresa
SELECT 
NumeroDam, --NUMERO DO DAM	
 GETDATE(),
ValorCobrado,	
'Baixa Manual',
1,	
Empresa_ID,
'ASS32A1SD3A2S1DA1SD1AS5ASDA6S5D',--ID DE ACORDO A MODALIDADE
1,
Valor,	
1	,
'SISTEMA',	
GETDATE(),	
'Arquivo de Origem: baixa Manual'
 FROM [Recursos_DAMSTP_SQL_TRANS_HOMOLOGACAO].dbo.DAM where Situacao in ('Emitido')

 GO

 update ep
 set emp_Saldo = Valor
 from EmpresaAplicativo ep ,Recursos_DAMSTP_SQL_TRANS_HOMOLOGACAO.dbo.DAM  d where ep.emp_Empresaid = d.Empresa_ID
 and Situacao in ('Emitido')
 GO
 update d
 set DataPagamento = GETDATE(),
     Situacao = 'Pago',
	 DataCredito = GETDATE()-1,
	 ValorRecebido = ValorCobrado
 from EmpresaAplicativo ep ,Recursos_DAMSTP_SQL_TRANS_HOMOLOGACAO.dbo.DAM  d where ep.emp_Empresaid = d.Empresa_ID
  and Situacao in ('Emitido')

