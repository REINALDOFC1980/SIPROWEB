using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDJULGAMENTO.Models
{
    public class InstrucaoProcessoModel
    {
        public string? INSPRO_Usuario_origem { get; set; }
        public string? INSPRO_Setor_origem { get; set; }
        public string? INSPRO_Setor_destino { get; set; }        
        public string? INSPRO_Parecer { get; set; }
        public string? INSPRO_Arquivo { get; set; }
        public int INSPRO_Mov_id { get; set; }
        public int INSPRO_Dis_id { get; set; }
        public string? INSPRO_DATA_ORIGEM { get; set; }

    }
}
