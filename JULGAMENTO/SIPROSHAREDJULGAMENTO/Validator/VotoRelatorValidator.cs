using FluentValidation;
using SIPROSHAREDJULGAMENTO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDHOMOLOGACAO.Validator
{
    public class VotoRelatorValidator : AbstractValidator<JulgamentoProcessoModel>
    {
        public VotoRelatorValidator()
        {

            RuleFor(user => user.Disjug_Dis_Id)
                .NotEmpty().WithMessage("A ID da distribuição é obrigatório")
                .GreaterThan(0).WithMessage("A origem deve ser um número maior que zero.");

            RuleFor(user => user.Disjug_Relator)
               .NotEmpty().WithMessage("O Relator é obrigatório.");

            RuleFor(user => user.Disjug_Parecer_Relatorio)
                .NotEmpty().WithMessage("O parecer é obrigatório.");

            RuleFor(user => user.Disjug_Resultado)
             .NotEmpty().WithMessage("O resultado é obrigatório.");

            RuleFor(user => user.Disjug_Motivo_Voto)
                .NotEmpty().WithMessage("O motivo do voto é obrigatório.");

            RuleFor(user => user.Disjug_Membro1)
                .NotEmpty().WithMessage("Primeiro membro é obrigatório.");

            RuleFor(user => user.Disjug_Membro2)
           .NotEmpty().WithMessage("Segundo membro é obrigatório.");

        }
    }
}



