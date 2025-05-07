using SIPROSHAREDJULGAMENTO.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDJULGAMENTO.Service.IRepository
{
    public interface IExcluirVotoService
    {

        Task<List<ExcluirModel>> LocalizarProcessosExcluirVoto(string usuario, string situacao, string processo);

        Task<List<ExcluirDetalheModel>> BuscarVotacao(string processo);

        Task<ExcluirDetalheModel> BuscarParecer(string processo);

        Task ExcluirVoto(ExcluirDetalheModel excluirModel, IDbConnection connection, IDbTransaction transaction);

    }
}
