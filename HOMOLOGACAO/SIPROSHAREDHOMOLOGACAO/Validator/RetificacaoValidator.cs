using FluentValidation;
using SIPROSHAREDHOMOLOGACAO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDHOMOLOGACAO.Validator
{
    public class RetificacaoValidator : AbstractValidator<RetificacaoModel>
    {
        public RetificacaoValidator()
        {
            RuleFor(user => user.MOVPRO_ID)
               .NotEmpty().WithMessage("codigo da movimentação é obrigatória.")
               .GreaterThan(0).WithMessage("A origem deve ser um número maior que zero.");

            RuleFor(user => user.MOVPRO_PARECER_ORIGEM)
               .NotEmpty().WithMessage("Parecer é obrigatório.");

            RuleFor(user => user.MOVPRO_USUARIO_ORIGEM)
               .NotEmpty().WithMessage("Parecer é obrigatório.");

        }

    }
}
       
