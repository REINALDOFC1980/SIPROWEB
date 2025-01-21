using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDINSTRUCAO.Models
{
    public class InstrucaoProcessosModel
    {

        public int DIS_ID { get; set; }
        public string? DIS_ORIGEM_DATA { get; set; }

        public int MOVPRO_ID { get; set; }
        public int MOVPRO_PARECER_ORIGEM { get; set; }
        public string? MOVPRO_USUARIO_ORIGEM { get; set; }

        public string? PRT_NUMERO { get; set; }
        public string? PRT_NOME_ORIGEM { get; set; }

        public string? PRT_SETOR_ORIGEM { get; set; }
        
        public string? PRT_NOME_ASSUNTO { get; set; }
        public string? PRT_DT_ABERTURA { get; set; }
        public string? PRT_DT_POSTAGEM { get; set; }
        public string? PRT_CPFCNJ_PROPRIETARIO { get; set; }
        public string? PRT_NOMEPROPRIETARIO { get; set; }
        public string? PRT_PARECER { get; set; }
        public string? PRT_AIT { get; set; }
        public string? PRT_PLACA { get; set; }
        public string? PRT_SITUACAO { get; set; }
        public string? PRT_ORIGEM { get; set; }
        public string? PRT_ATENDENTE { get; set; }
    }
}
