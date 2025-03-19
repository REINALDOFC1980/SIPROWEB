using FluentValidation;
using SIPROSHAREDHOMOLOGACAO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDHOMOLOGACAO.Validator
{
    public class HomologarValidator :AbstractValidator<JulgamentoModel>
    {
        public HomologarValidator()
        {

            RuleFor(user => user.MovPro_id)
                .NotEmpty().WithMessage("A Id do assunto é obrigatório.")
                .GreaterThan(0).WithMessage("A origem deve ser um número maior que zero.");

            RuleFor(user => user.MovPro_Prt_Numero)
               .NotEmpty().WithMessage("O Processo é obrigatório.");

            RuleFor(user => user.Disjug_Homologador)
                .NotEmpty().WithMessage("O resultado é obrigatório.");

            RuleFor(user => user.Disjug_Parecer_Relatorio)
                .NotEmpty().WithMessage("O parecer é obrigatório.");

            RuleFor(user => user.Disjul_SetSub_Id)
                .NotEmpty().WithMessage("O ID do setor é obrigatório.");

        }
    }
}



