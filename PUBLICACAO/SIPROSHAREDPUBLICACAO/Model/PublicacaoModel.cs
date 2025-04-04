using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace SIPROSHAREDPUBLICACAO.Model
{
    public class PublicacaoModel
    {
        public string? prt_lote { get; set; }
        public string? prt_dt_lote { get; set; }
        public int prt_publicacao_qtd  { get; set; }
        public string? prt_publicacao_dom  { get; set; }
        public string? prt_dt_publicacao { get; set; }
        public string? prt_usu_publicacao { get; set; }

    }
}
