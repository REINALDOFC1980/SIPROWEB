using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDDISTRIBUICAO.Models
{
    public class ListaProcessoUsuario
    {

        public int DIS_ID { get; set; }
        public int MOVPRO_ID { get; set; }
        public string? PRT_NUMERO { get; set; }
        public string? PRT_AIT { get; set; }
        public string? PRT_DT_ABERTURA { get; set; }
        public string? PRT_ASSUNTO { get; set; }
        public string? PRT_USUARIO { get; set; }
        public string? PRT_STATUS { get; set; }
        public string? PRT_QTD { get; set; }
    }
}
