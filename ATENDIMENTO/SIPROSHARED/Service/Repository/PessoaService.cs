using Dapper;
using FluentValidation;
using SIPROSHARED.DbContext;
using SIPROSHARED.Models;
using SIPROSHARED.Service.IRepository;
using SIPROSHARED.Validator;
using SIRPOEXCEPTIONS.ExceptionBase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHARED.Service.Repository
{
    public class PessoaService : IPessoaService
    {
        private readonly DapperContext _context;

        public PessoaService(DapperContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

        }
        public async Task<PessoaModel> GetDadosPessoa(string cpf_cnpj)
        {
            if (string.IsNullOrEmpty(cpf_cnpj))
                throw new ErrorOnValidationException(new List<string> { "Erro de parametro. Favor entrar em contato com ADM!" });

            var query = @"   
                      Select Top 1
                             pes_ID     
                            ,pes_Tipo                                           
                            ,pes_Nome                                                                                                                                                                                                                                                                                                                                                                                       
                            ,pes_CPF                                            
                            ,pes_RG                                                   
                            ,pes_EndLogradouro                                  
                            ,pes_EndNumero 
                            ,Substring(pes_EndComplemento,0,20) pes_EndComplemento                                                                                   
                            ,Substring(pes_EndBairro,0,20) as pes_EndBairro                                     
                            ,pes_Municipio                                      
                            ,pes_UF 
                            ,pes_EndCEP   
                            ,pes_UFCNH 
                            ,RIGHT('00000000000' + pes_NumRegistroCNH, 11) as pes_NumRegistroCNH                                    
                            ,Convert(varchar(10),pes_DT_Validade,103)  as pes_DT_Validade       
                            ,pes_Email                                                              
                            ,pes_Celular     
                            ,pes_Telefone    
                            ,Convert(varchar(10),pes_DataCadastro,103)  as pes_DataCadastro      
                            ,pes_Pais                                                               
                        from Pessoa where (pes_CPF = @cpf_cnpj)
                            ";

            using (var connection = _context.CreateConnection())
            {
                var parametros = new { cpf_cnpj };
                var command = await connection.QueryFirstOrDefaultAsync<PessoaModel>(query, parametros);
                               
                return command;
            }
      
    }

        public async Task IntoPesssoa(PessoaModel pessoaModel)
        {
                //validando!
                var validator = new PessoaValidator();
                var result = validator.Validate(pessoaModel);
                if (result.IsValid == false)
                    throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
                //fim

                var dbParametro = new DynamicParameters();
                dbParametro.Add("pes_Estrangeiro", pessoaModel.pes_Estrangeiro);
                dbParametro.Add("pes_CPF", pessoaModel.pes_CPF);
                dbParametro.Add("pes_Nome", pessoaModel.pes_Nome);
                dbParametro.Add("pes_EndLogradouro", pessoaModel.pes_EndLogradouro);
                dbParametro.Add("pes_EndNumero", pessoaModel.pes_EndNumero);
                dbParametro.Add("pes_EndComplemento", pessoaModel.pes_EndComplemento);
                dbParametro.Add("pes_EndBairro", pessoaModel.pes_EndBairro);
                dbParametro.Add("pes_Municipio", pessoaModel.pes_Municipio);
                dbParametro.Add("pes_UF", pessoaModel.pes_UF);
                dbParametro.Add("pes_EndCEP", pessoaModel.pes_EndCEP);
                dbParametro.Add("pes_UFCNH", pessoaModel.pes_UFCNH);
                dbParametro.Add("pes_NumRegistroCNH", pessoaModel.pes_NumRegistroCNH);
                dbParametro.Add("pes_DT_Validade", pessoaModel.pes_DT_Validade);
                dbParametro.Add("pes_Email", pessoaModel.pes_Email);
                dbParametro.Add("pes_Celular", pessoaModel.pes_Celular);
                dbParametro.Add("pes_Telefone", pessoaModel.pes_Telefone);

                dbParametro.Add("pes_Pais", pessoaModel.pes_Pais);

                string query = @"
                               insert into Pessoa
			                  (  pes_Tipo         
                                ,pes_Nome          
                                ,pes_CPF                          
                                ,pes_EndLogradouro 
                                ,pes_EndNumero     
                                ,pes_EndComplemento
                                ,pes_EndBairro     
                                ,pes_Municipio     
                                ,pes_UF            
                                ,pes_EndCEP        
                                ,pes_UFCNH         
                                ,pes_NumRegistroCNH
                                ,pes_DT_Validade   
                                ,pes_Email         
                                ,pes_Celular       
                                ,pes_Telefone      
                                ,pes_DataCadastro  
                                ,pes_Pais )
			                   values
			                   (  Case when @pes_Estrangeiro is null then 'Física' else 'Estrangeira' end                
			                     ,@pes_Nome          
                                 ,@pes_CPF                    
                                 ,@pes_EndLogradouro 
                                 ,@pes_EndNumero     
                                 ,@pes_EndComplemento
                                 ,@pes_EndBairro     
                                 ,@pes_Municipio     
                                 ,@pes_UF            
                                 ,@pes_EndCEP        
                                 ,@pes_UFCNH         
                                 ,@pes_NumRegistroCNH
                                 ,@pes_DT_Validade   
                                 ,@pes_Email         
                                 ,@pes_Celular       
                                 ,@pes_Telefone      
                                 ,Getdate()  
                                 ,@pes_Pais)";

                using (var connection = _context.CreateConnection())
                {
                    await connection.ExecuteAsync(query, dbParametro);
                }
            
        }

        public async Task UpdatePesssoa(PessoaModel pessoaModel)
        {
   

                //validando!
                var validator = new PessoaValidator();
                var result = validator.Validate(pessoaModel);
                if (result.IsValid == false)
                    throw new ErrorOnValidationException(result.Errors.Select(e => e.ErrorMessage).ToList());
                //fim


                var dbParametro = new DynamicParameters();
                dbParametro.Add("pes_ID", pessoaModel.pes_ID);
                dbParametro.Add("pes_CPF", pessoaModel.pes_CPF);
                dbParametro.Add("pes_Nome", pessoaModel.pes_Nome);
                dbParametro.Add("pes_EndLogradouro", pessoaModel.pes_EndLogradouro);
                dbParametro.Add("pes_EndNumero", pessoaModel.pes_EndNumero);
                dbParametro.Add("pes_EndComplemento", pessoaModel.pes_EndComplemento);
                dbParametro.Add("pes_EndBairro", pessoaModel.pes_EndBairro);
                dbParametro.Add("pes_Municipio", pessoaModel.pes_Municipio);
                dbParametro.Add("pes_UF", pessoaModel.pes_UF);
                dbParametro.Add("pes_EndCEP", pessoaModel.pes_EndCEP);
                dbParametro.Add("pes_UFCNH", pessoaModel.pes_UFCNH);
                dbParametro.Add("pes_NumRegistroCNH", pessoaModel.pes_NumRegistroCNH);
                dbParametro.Add("pes_DT_Validade", pessoaModel.pes_DT_Validade);
                dbParametro.Add("pes_Email", pessoaModel.pes_Email);
                dbParametro.Add("pes_Celular", pessoaModel.pes_Celular);
                dbParametro.Add("pes_Telefone", pessoaModel.pes_Telefone);
                dbParametro.Add("pes_DataCadastro", pessoaModel.pes_DataCadastro);
                dbParametro.Add("pes_Pais", pessoaModel.pes_Pais);

                string query = @"
                              Update  Pessoa
                                 Set  pes_Nome           = @pes_Nome                                
	                                 ,pes_EndLogradouro  = @pes_EndLogradouro 
	                                 ,pes_EndNumero      = @pes_EndNumero     
	                                 ,pes_EndComplemento = @pes_EndComplemento
	                                 ,pes_EndBairro      = @pes_EndBairro     
	                                 ,pes_Municipio      = @pes_Municipio     
	                                 ,pes_UF             = @pes_UF            
	                                 ,pes_EndCEP         = @pes_EndCEP        
	                                 ,pes_UFCNH          = @pes_UFCNH         
	                                 ,pes_NumRegistroCNH = @pes_NumRegistroCNH
	                                 ,pes_DT_Validade    = @pes_DT_Validade   
	                                 ,pes_Email          = @pes_Email         
	                                 ,pes_Celular        = @pes_Celular       
	                                 ,pes_Telefone       = @pes_Telefone      
	                                 ,pes_Pais 			 = @pes_Pais
	                            Where pes_ID             = @pes_ID ";


                using (var connection = _context.CreateConnection())
                {
                    await connection.ExecuteAsync(query, dbParametro);
                }


        }

    }
}
