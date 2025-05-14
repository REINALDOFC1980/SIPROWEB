using FluentValidation;
using SIPROSHARED.Models;
using SIPROSHAREDJULGAMENTO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDJULGAMENTO.Validator
{
    public class AnexarDocValidator : AbstractValidator<ProtocoloModel>
    {
        public AnexarDocValidator() {

            RuleFor(user => user.PRT_NUMERO)
                .NotEmpty().WithMessage("A ID da excluião é obrigatório");

            RuleFor(user => user.PRT_AIT)
               .NotEmpty().WithMessage("O Numero do processo é obrigatório.");

            RuleFor(user => user.PRT_ATENDENTE)
                .NotEmpty().WithMessage("O usaurio é obrigatório.");

        }
    }
}
