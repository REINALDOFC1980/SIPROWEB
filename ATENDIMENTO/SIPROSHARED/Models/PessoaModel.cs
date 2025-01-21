namespace SIPROSHARED.Models
{
    public class PessoaModel
    {         
        public int     pes_ID               { get; set; }
        public string? pes_CPF              { get; set; }
        public string? pes_RG               { get; set; }
        public string? pes_CNPJ             { get; set; }
        public string? pes_Tipo             { get; set; }
        public string? pes_Nome             { get; set; }
       // public string? pes_NomeFantasia     { get; set; }

        public string? pes_EndLogradouro    { get; set; }
        public string? pes_EndNumero        { get; set; }
        public string? pes_EndComplemento   { get; set; }
        public string? pes_EndBairro        { get; set; }
        public string? pes_Municipio        { get; set; }
        public string? pes_UF               { get; set; }
        public string? pes_EndCEP           { get; set; }
        public string? pes_UFCNH            { get; set; }
        public string? pes_NumRegistroCNH   { get; set; }
        public string? pes_DT_Validade      { get; set; }
        public string? pes_Email            { get; set; }
        public string? pes_Celular          { get; set; }
        public string? pes_Telefone         { get; set; }
        public string? pes_DataCadastro     { get; set; }
        public string? pes_Pais             { get; set; }
        public string? pes_Estrangeiro      { get; set; }

    }
}
