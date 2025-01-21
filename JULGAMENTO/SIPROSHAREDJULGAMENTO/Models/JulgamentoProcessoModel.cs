using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDJULGAMENTO.Models
{
    public class JulgamentoProcessoModel
    {
        public int Disjug_Id { get; set; }
        public int Disjug_Dis_Id { get; set; }
        public string? Disjug_Relator { get; set; }
        public string? Disjug_Data { get; set; }
        public string? Disjug_Parecer_Relatorio { get; set; }
        public string? Disjug_Resultado { get; set; }
        public int Disjug_Motivo_Voto { get; set; }
        public string? Disjug_Resultado_Data { get; set; }
        public string? Disjug_Membro1 { get; set; }
        public string? Disjug_Membro2 { get; set; }
        public string? Disjul_tipo { get; set; }
        public int Dis_MOT_COD { get; set; }
        public int DisMov_Id { get; set; }
        public string? Prt_Numero { get; set; }
    }
}
