using FluentValidation;
using SIPROSHAREDINSTRUCAO.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDPUBLICACAO.Validator
{
    public class EncaminharInstrucaoValidator : AbstractValidator<InstrucaoModel>
    {

        public EncaminharInstrucaoValidator()
        {
            RuleFor(user => user.INSPRO_Dis_id)
               .NotEmpty().WithMessage("Código da distribuição é obrigatório.")
               .GreaterThan(0).WithMessage("Código da distribuição deve ser um número maior que zero.");

            RuleFor(user => user.INSPRO_Usuario_origem)
               .NotEmpty().WithMessage("O campo usuário origem é obrigatório");


            RuleFor(user => user.INSPRO_Parecer)
                .NotEmpty().WithMessage("O campo parecer é obrigatório");

        }
      
    }
}
