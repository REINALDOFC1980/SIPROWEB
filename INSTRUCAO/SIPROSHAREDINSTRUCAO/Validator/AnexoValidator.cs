using FluentValidation;
using SIPROSHARED.Models;
using SIPROSHAREDINSTRUCAO.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDPUBLICACAO.Validator
{
    public class AnexoValidator : AbstractValidator<ProtocoloModel>
    {

        public AnexoValidator()
        {
            RuleFor(x => x.PRT_NUMERO)
                .NotEmpty().WithMessage("Número do protocolo é obrigatório.");

            RuleFor(x => x.PRT_AIT)
                .NotEmpty().WithMessage("Número do AIT é obrigatório.");

            RuleFor(x => x.PRT_ATENDENTE)
                .NotEmpty().WithMessage("Nome do atendente é obrigatório.");

            RuleFor(x => x.PRTDOC_MOVPRO_ID)
                .GreaterThan(0).WithMessage("ID do movimento deve ser maior que zero.");
        }

    }
}
