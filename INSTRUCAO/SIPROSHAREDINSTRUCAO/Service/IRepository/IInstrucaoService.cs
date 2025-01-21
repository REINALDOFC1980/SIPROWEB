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

        Task UploadAnexo(List<IFormFile> arquivos, string protocolo);


        Task<List<SetorModel>> BuscarSetor();

        Task<InstrucaoModel> BuscarMovimentacaoInstrucao(int dis_id);





        Task EncaminharIntrucao(InstrucaoModel instrucaoProcesso, IDbConnection connection, IDbTransaction transaction);


        Task<List<Anexo_Model>> BuscarAnexo(string usuario, string ait);

        Task ExcluirAnexo(int prtdoc_id);

        Task SalvarAnexo(string folderPath, string usuario, int mov_id, IDbConnection connection, IDbTransaction transaction);








    }
}
