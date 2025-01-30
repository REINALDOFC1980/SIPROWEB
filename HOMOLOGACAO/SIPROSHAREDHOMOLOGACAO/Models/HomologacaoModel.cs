using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDHOMOLOGACAO.Models
{
    public class HomologacaoModel
    {

        //MULTAS
        public int MOVPRO_ID { get; set; }         
        public string? PRT_AIT { get; set; }
        public string? PRT_AIT_SITUACAO { get; set; }
        public string? PRT_PLACA { get; set; }
        public string? PRT_DT_COMETIMENTO { get; set; }
        public string? PRT_DT_PRAZO { get; set; }
        public string? PRT_RENAVAN { get; set; }
       


        //PROTOCOLO
        public string? PRT_NUMERO { get; set; }
        public string? PRT_NOME_ORIGEM { get; set; }
        public string? PRT_DT_ABERTURA { get; set; }
        public string? PRT_NOME_ASSUNTO { get; set; }
        public string? PRT_TIPO_SOLICITANTE { get; set;}
        public string? PRT_OBSERVACAO { get; set; }
        public string? PRT_RESULTADO { get; set; }
        public string? SETSUB_NOME { get; set; }

    }
}
