using SIPROSHAREDPUBLICACAO.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDPUBLICACAO.Service.IRepository
{
    public interface IPublicacaoService
    {
        Task<int> QuantidadeProcesso(string usuario);

        Task GerarLote(string usuario);

        Task <List<PublicacaoModel>> BuscarLotes(string usuario);

        Task<PublicacaoModel> Buscar_Lote(string Lote);

        Task AtualizarPublicacao(PublicacaoModel publicacaoModel);

        Task ExcluirLote (string lote);  
        

    }
}
