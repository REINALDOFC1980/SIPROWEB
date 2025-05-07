using FluentValidation;
using SIPROSHAREDJULGAMENTO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDJULGAMENTO.Validator
{
    public class ExcluirVotoValidator : AbstractValidator<ExcluirDetalheModel>
    {
        public ExcluirVotoValidator() {

            RuleFor(user => user.MovPro_id)
                .NotEmpty().WithMessage("A ID da excluião é obrigatório")
                .GreaterThan(0).WithMessage("A origem deve ser um número maior que zero.");

            RuleFor(user => user.MovPro_Prt_Numero)
               .NotEmpty().WithMessage("O Numero do processo é obrigatório.");

            RuleFor(user => user.Disjul_Usuario)
                .NotEmpty().WithMessage("O usaurio é obrigatório.");
        }
    }
}
