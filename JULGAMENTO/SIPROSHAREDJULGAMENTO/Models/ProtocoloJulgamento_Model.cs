namespace SIPROSHAREDJULGAMENTO.Models
{
    public class ProtocoloJulgamento_Model
    {
        public string? PRT_NUMERO { get; set; }
        public int PRT_COD_ORIGEM { get; set; }
        public string? PRT_NOME_ORIGEM { get; set; }
        
        public string? PRT_OBSERVACAO { get; set; }
        public string? PRT_NOME_ASSUNTO { get; set; }         
        public string? PRT_DT_ABERTURA { get; set; }
        public string? PRT_DT_POSTAGEM { get; set; }
        public string? PRT_CPFCNJ_PROPRIETARIO { get; set; }
        public string? PRT_NOMEPROPRIETARIO { get; set; }
        public string? PRT_AIT { get; set; }

        public string? PRT_RESTRICAO { get; set; }
        public string? PRT_RESTRICAO_NOME { get; set; }
        public string? PRT_PLACA { get; set; }
        public string? PRT_SITUACAO { get; set; }
        public int DIS_ID { get; set; }

    }
}


     