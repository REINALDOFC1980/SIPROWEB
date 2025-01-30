
using SIPROSHAREDHOMOLOGACAO.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDHOMOLOGACAO.Service.IRepository
{
    public interface IHomologacaoService
    {
        Task<List<HomologacaoModel>> LocalizarHomolgacao(int setor, string resultado);
        
        Task<HomologacaoModel> GetHomologacao(int movpro_id);

        Task<List<SetorModel>> BuscarSetor();

        Task<List<Anexo_Model>> BuscarAnexo(string usuario, string ait);

        Task RealizarHomologacao(HomologacaoModel homologacaoModel, IDbConnection connection, IDbTransaction transaction);
    }
}
