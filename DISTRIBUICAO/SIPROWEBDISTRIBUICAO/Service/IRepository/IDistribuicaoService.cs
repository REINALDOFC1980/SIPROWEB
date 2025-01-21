using SIPROSHARED.Models;
using SIPROSHAREDDISTRIBUICAO.Models;
using System.Collections.Generic;
using System.Data;


namespace SIPROSHAREDDISTRIBUICAO.Service.IRepository
{
    public interface IDistribuicaoService
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
