using Microsoft.AspNetCore.Http;
using SIPROSHARED.Models;
using SIPROSHAREDINSTRUCAO.Models;
using System.Data;

namespace SIPROSHAREDINSTRUCAO.Service.IRepository
{

    public interface IInstrucaoService
    {
        Task<InstrucaoProcessosModel> GetInstrucao(int dis_id);

        Task<List<InstrucaoProcessosModel>> LocalizarInstrucao(string usuario, string vlobusca);

        //Task UploadAnexo(List<IFormFile> arquivos, string protocolo);
        Task IntoAnexo(List<IFormFile> arquivos, ProtocoloModel protocolo);

        Task<List<SetorModel>> BuscarSetor();

        Task<InstrucaoModel> BuscarMovimentacaoInstrucao(int dis_id);


        Task EncaminharIntrucao(InstrucaoModel instrucaoProcesso, IDbConnection connection, IDbTransaction transaction);

        Task<List<Anexo_Model>> BuscarAnexo(string usuario, string ait);

        Task<List<AnexoModel>> BuscarAnexosBanco(string prt_numero, string usuario);
       
        Task ExcluirAnexo(int prtdoc_id);









    }
}
