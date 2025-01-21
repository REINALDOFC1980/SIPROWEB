using Microsoft.AspNetCore.Http;
using SIPROSHARED.Models;
using SIPROSHAREDJULGAMENTO.Models;

namespace SIPROSHAREDJULGAMENTO.Service.IRepository
{
    public interface IJulgamentoService
    {

        Task<ProtocoloJulgamento_Model> LocalizarProcesso(string usuario, string vlobusca);

        Task<List<ProtocoloJulgamento_Model>> LocalizarProcessos(string usuario, string vlobusca);

        Task<List<ProtocoloJulgamento_Model>> LocalizarProcessosAssinar(string usuario, string vlobusca);

        Task<List<MovimentacaoModel>> BuscarMovimentacao(string prt_numero);

        Task<List<SetorModel>> BuscarSetor();

        Task<List<MembroModel>> BuscarMembros(string usuario);

        Task<List<MotivoVotoModel>> BuscarMotivoVoto();

        Task<List<JulgamentoProcessoModel>> BuscarVotacao( int vlobusca);

        Task<JulgamentoProcessoModel> BuscarParecerRelator(int vlobusca);

        Task EncamimharProcessoInstrucao(InstrucaoProcessoModel instrucaoProcesso);

        Task<JulgamentoProcessoModel> VerificarVoto(string disjug_relator, int disjug_dis_id);

        Task InserirVotoRelator(JulgamentoProcessoModel julgamentoProcesso);

        Task InserirVotoMembro(JulgamentoProcessoModel julgamentoProcesso);

        Task IntoAnexo(List<IFormFile> arquivos, ProtocoloModel protocolo);

        Task<List<Anexo_Model>> BuscarAnexo(string usuario, string ait);

        Task ExcluirAnexo(int prtdoc_id);

        Task<List<InstrucaoProcessoModel>> BuscarHistoricoInstrucao(string vlobusca);

    }
}
