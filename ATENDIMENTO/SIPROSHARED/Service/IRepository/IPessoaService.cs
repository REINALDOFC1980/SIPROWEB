using SIPROSHARED.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHARED.Service.IRepository
{
    public interface IPessoaService
    {
        Task<PessoaModel> GetDadosPessoa(string cpf_cnpj);

        Task IntoPesssoa(PessoaModel pessoaModel);

        Task UpdatePesssoa(PessoaModel pessoaModel);
    }
}
