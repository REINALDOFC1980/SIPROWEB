using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SIPROSHARED.API;
using SIPROSHARED.DbContext;
using SIPROSHARED.Models;
using System.Globalization;
using SIPROSHARED.Service.IRepository;

namespace SIPROAPI.Controllers
{
    [Route("sipro/pessoa")]
    [ApiController]
    public class PessoaController : ControllerBase
    {
        private readonly IPessoaService _pessoaService;
        private readonly Detran _detran;


        public PessoaController(IPessoaService pessoaService, Detran detran)
        {
            _pessoaService = pessoaService;
            _detran = detran;
        }

        [HttpGet]
        [Route("getpessoa/{cpf}")]
        public async Task<IActionResult> GetPessoa(string cpf)
        {
           
                // Buscar dados do solicitante pra saber se tem cadastro!
                var solicitante = await _pessoaService.GetDadosPessoa(cpf);

                if (solicitante == null)
                {
                    return NoContent();
                }

                return Ok(solicitante);
            
           
        }

        [HttpGet]
        [Route("getpessoadetram/{cpf}")]
        public async Task<IActionResult> GetPessoaDetram(string cpf)
        {
           
                ResultGetCnhPorCpf resultGetCnhPorCpf = await _detran.CnhCpfAsync(cpf);

                if (resultGetCnhPorCpf == null)
                {
                    return NoContent();
                }

                PessoaModel pessoaModel = new PessoaModel();

                if (resultGetCnhPorCpf != null)//caso tenha nos dois atualizar o registro na base local
                {

                    pessoaModel.pes_CPF = resultGetCnhPorCpf.cpf;
                    pessoaModel.pes_Nome = resultGetCnhPorCpf.nome;
                    pessoaModel.pes_NumRegistroCNH = resultGetCnhPorCpf.nrRegistro.PadLeft(11, '0');
                    pessoaModel.pes_UFCNH = resultGetCnhPorCpf.ufCnh;

                    if (DateTime.TryParse(resultGetCnhPorCpf.dataDeValidadeCnh, out var validadeCnh))
                    {
                        pessoaModel.pes_DT_Validade = validadeCnh.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        pessoaModel.pes_DT_Validade = null; // ou algum valor default
                    }

                   
                }

                return Ok(pessoaModel);
            

        }

        [HttpPost]
        [Route("addpessoa")]
        public async Task<IActionResult> AddPessoa(PessoaModel pessoa)
        {
       
                await _pessoaService.IntoPesssoa(pessoa);

                return Ok();
        }

        [HttpPost]
        [Route("alterpessoa")]
        public async Task<IActionResult> AlterPessoa(PessoaModel pessoa)
        {            

                await _pessoaService.UpdatePesssoa(pessoa);


                return Ok();

            

        }

       

    }
}
