using FluentValidation;
using SIPROSHAREDJULGAMENTO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDJULGAMENTO.Validator
{
    public class InstrucaoValidator : AbstractValidator<InstrucaoProcessoModel>
    {
        public InstrucaoValidator() {

            RuleFor(user => user.INSPRO_Dis_id)
                .NotEmpty().WithMessage("A ID da excluião é obrigatório")
                .GreaterThan(0).WithMessage("A origem deve ser um número maior que zero.");

            RuleFor(user => user.INSPRO_Setor_destino)
               .NotEmpty().WithMessage("O Setor é obrigatório.");

            RuleFor(user => user.INSPRO_Parecer)
                .NotEmpty().WithMessage("O Parecer é obrigatório.");
        }
    }
}
