using FluentValidation;
using SIPROSHAREDJULGAMENTO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDHOMOLOGACAO.Validator
{
    public class VotoMembroValidator : AbstractValidator<JulgamentoProcessoModel>
    {
        public VotoMembroValidator()
        {

            RuleFor(user => user.Disjug_Dis_Id)
                .NotEmpty().WithMessage("A ID da distribuição é obrigatório")
                .GreaterThan(0).WithMessage("A origem deve ser um número maior que zero.");

            RuleFor(user => user.Disjug_Relator)
               .NotEmpty().WithMessage("O Relator é obrigatório.");
          
        }
    }
}



