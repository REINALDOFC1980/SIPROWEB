using SIPROSHARED.Models;

using System.Data;


namespace SIPROSHARED.Service.IRepository
{
    public interface IAtendimentoService
    {  

        Task<AgendaModel> GetAgendamento(string cpf);


        Task UpdateAgendamento(string ait, int codservico, string situacao);
  
        Task<int> DuplicidadeServico(string ait, int codservico);

        Task<bool> CadastrarAberturaAsync(AgendaModel agendaModel);

        Task IntoAnexoFolder(string folderPath, ProtocoloDocImgModel protocoloDoc, IDbConnection connection, IDbTransaction transaction);
        
        Task<List<AnexoModel>> BuscarAnexos();
       
        Task<List<AnexoModel>> BuscarAnexosTemp(string folderPath);
       
        Task<List<DocumentosModel>> BuscarDocumentos(int codservico);
       
        Task<string> RealizarAbertura(ProtocoloModel dadosMulta, IDbConnection connection, IDbTransaction transaction);
       
        Task<ProtocoloModel> BuscarProtocolo(string numeroprotocolo);
       
        Task<List<ProtocoloModel>> BuscarProtocoloAll(string vloBusca);
       
        Task<List<MovimentacaoModel>> BuscarMovimentacao(string vloBusca);



    }
}
