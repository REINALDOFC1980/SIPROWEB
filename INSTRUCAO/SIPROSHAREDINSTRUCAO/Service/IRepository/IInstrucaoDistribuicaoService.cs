using SIPROSHARED.Models;
using SIPROSHAREDINSTRUCAO.Models;
using System.Collections.Generic;
using System.Data;


namespace SIPROSHAREDINSTRUCAO.Service.IRepository
{
    public interface IInstrucaoDistribuicaoService
    {
        Task<List<AssuntoQtd>> GetQtdProcessosPorAssunto(string usuario);
        Task<List<ProtocoloDistribuicaoModel>> GetQtdProcessosDistribuidoPorUsuario(string usuario);
        Task<List<ListaProcessoUsuario>> GetProcessoSetor(string usuario);
        Task<ListaProcessoUsuario> GetProcesso(int movpro_id);
        Task<List<ListaProcessoUsuario>> GetProcessosUsuario(string usuario);

        Task<int> GetPresidente(string usuario);
        Task DistribuicaoProcesso(ProtocoloDistribuicaoModel distribuicaoModel, IDbConnection connection, IDbTransaction transaction);
        Task DistribuicaoProcessoEspecifico(ProtocoloDistribuicaoModel distribuicaoModel, IDbConnection connection, IDbTransaction transaction);
        Task RetirarProcesso(ProtocoloDistribuicaoModel distribuicaoModel);
        Task RetirarProcessoEspecifico(ProtocoloDistribuicaoModel distribuicaoModel);


    }
}
