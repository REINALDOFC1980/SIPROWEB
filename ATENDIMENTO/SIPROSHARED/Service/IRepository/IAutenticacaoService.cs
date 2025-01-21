using SIPROSHARED.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHARED.Service.IRepository
{
    public interface IAutenticacaoService
    {
        Task<AutenticacaoModel> Autenticacao(string login, string senha);

        string GerearToken(AutenticacaoModel autenticacaoModel);
    }
}
