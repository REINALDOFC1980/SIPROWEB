namespace SIPROSHARED.Models
{
    public class AgendaModel
    {
        public int Age_Id { get; set; }
        public string? Age_Dt_Programada   { get; set; }
        public string? Age_AIT  { get; set; }
        public string? Age_Doc_Solicitante { get; set; }
        public string? Age_Nome_Solicitante { get; set; }
        public int Age_Cod_Assunto  { get; set; }
        public int Age_Cod_Geral  { get; set; }
        public int Age_Placa { get; set; }
        public int Age_Cod_Origem  { get; set; }
        public string? Age_Abertura { get; set; }
        public string? Ass_Nome { get; set; }
        public string? Ori_Descricao { get; set; }

        public string? Age_tipo_Solicitante { get; set; }

    }
}
