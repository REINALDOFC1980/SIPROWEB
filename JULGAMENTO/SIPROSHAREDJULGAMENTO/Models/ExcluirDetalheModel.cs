﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDJULGAMENTO.Models
{
    public class ExcluirDetalheModel
    {
        public string? MovPro_Prt_Numero { get; set; }
        public int MovPro_id { get; set; }
        public string? MovPro_Situacao { get; set; }
        public string? MovPro_Acao { get; set; }
        public string? Disjug_Relator { get; set; }
        public string? Disjug_Resultado { get; set; }
        public string? Disjul_tipo { get; set; }
        public string? Disjug_Resultado_Data { get; set; }
        public string? Disjug_Parecer_Relatorio { get; set; }
        public string? Disjug_Homologador { get; set; }
        public string? Disjug_Tipo { get; set; }
        public int Disjul_SetSub_Id { get; set; }
        public string? Disjul_Usuario { get; set; }
    }
}
