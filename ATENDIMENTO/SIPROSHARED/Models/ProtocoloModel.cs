using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHARED.Models
{
    public class ProtocoloModel
    {

        //MULTAS
        public string? PRT_AIT { get; set; }
        public string? PRT_AIT_SITUACAO { get; set; }
        public string? PRT_PLACA { get; set; }
        public string? PRT_DT_COMETIMENTO { get; set; }
        public string? PRT_DT_PRAZO { get; set; }
        public string? PRT_RENAVAN { get; set; }


        //PROPRIETÁRIO
        public string? PRT_CPFCNJ_PROPRIETARIO { get; set; }
        public string? PRT_NOMEPROPRIETARIO { get; set; }
        public string? PRT_ENDERECO_PROPRIETARIO { get; set; }
        public string? PRT_CIDADE_PROPRIETARIO { get; set; }


        //CONDUTOR
        public string? PRT_CPF_CONDUTOR { get; set; }
        public string? PRT_NOME_CONDUTOR { get; set; }
        public string? PRT_NUMREGISTRO_CNH { get; set; }
        public string? PRT_MODELO_CNH { get; set; }
        public string? PRT_DT_VALIDADE { get; set; }
        public string? PRT_UF_CNH { get; set; }
        public string? PRT_ENDERECO_CONDUTOR { get; set; }


        //PROTOCOLO
        public string? PRT_NUMERO { get; set; }
        public int PRT_ORIGEM { get; set; }
        public string? PRT_NOME_ORIGEM { get; set; }
        public string? PRT_DT_CADASTRO { get; set; }
        public string? PRT_DT_ABERTURA { get; set; }
        public int PRT_ASSUNTO { get; set; }
        public string? PRT_NOME_ASSUNTO { get; set; }
        public string? PRT_TIPO_SOLICITANTE { get; set; }
        public string? PRT_CPF_SOLICITANTE { get; set; }
        public string? PRT_NOME_SOLICITANTE { get; set; }
        public string? PRT_DT_POSTAGEM { get; set; }
        public string? PRT_NUMERO_GEP { get; set; }
        public string? PRT_ATENDENTE { get; set; }
        public string? PRT_OBSERVACAO { get; set; }
        public int PRT_RESTRICAO { get; set; }
        public string? PRT_RESTRICAO_NOME { get; set; }
        public string? PRT_ACAO { get; set; }
        public string? PRT_SITUACAO { get; set; }


        //ARQUIVAMENTO
        public string? PRT_DT_ARQUIVO { get; set; }
        public string? PRT_USUARIOARQUIVO { get; set; }
        public string? PRT_MOV_USUARIO { get; set; }


        //JULGAMENTO
        public string? PRT_RETORNO_DETRAN { get; set; }
        public string? PRT_DT_JULGAMENTO { get; set; }
        public string? PRT_RESULTADO { get; set; }
        public string? PRT_MOTIVO_RESULTADO { get; set; }
        public string? PRT_LOTE { get; set; }
        public string? PRT_DT_PUBLICACAO { get; set; }
        public string? PRT_PUBLICACAO_DOM { get; set; }



        //RESSARSSIMENTO
        public string? PRT_RESS_BANCO { get; set; }
        public string? PRT_RESS_TIPO { get; set; }
        public string? PRT_RESS_TITULAR { get; set; }
        public string? PRT_RESS_CPF { get; set; }
        public string? PRT_RESS_AGENCIA { get; set; }
        public string? PRT_RESS_CONTA { get; set; }
        public string? PRT_RESS_OPERACAO { get; set; }


        //ESTRANGEIRO
        public string? PRT_CNH_ESTRANGEIRA { get; set; }
        public string? PRT_CNH_ESTRANGEIRA_NOME { get; set; }
        public string? PRT_CNH_ESTRANGEIRA_PAIS { get; set; }


        //OUTROS
        public string? PRT_AGENDAMENTO { get; set; }
        public string? PRT_CONDUTOR_APRESENTADO { get; set; }
        public int PRTDOC_MOVPRO_ID { get; set; }
        

    }
}
