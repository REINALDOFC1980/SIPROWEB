namespace SIPROSHAREDINSTRUCAO.Models
{
    public class ProtocoloInstrucaoModel
    {
        //[JsonProperty("DIS_ID")]
        public int DIS_ID { get; set; }
        public string? DIS_MOV_ID { get; set; }
        public string? DIS_ORIGEM_USUARIO { get; set; }
        public string? DIS_ORIGEM_DATA { get; set; }
        public string? DIS_DESTINO_USUARIO { get; set; }
        public string? DIS_NOME_USUARIO { get; set; }
        public string? DIS_DESTINO_STA_ID { get; set; }
        public string? DIS_DESTINO_STATUS { get; set; }
        public string? DIS_NUMJULGADOS { get; set; }
        public string? DIS_RETORNO { get; set; }
        public string? DIS_DATA_JULGAMENTO { get; set; }
        public string? DIS_PERFIL { get; set; }
        public int DIS_QTD { get; set; }
        public int DIS_PERCENTUAL { get; set; }
        public string? MOVPRO_PARECER_ORIGEM { get; set; }

    }
}

