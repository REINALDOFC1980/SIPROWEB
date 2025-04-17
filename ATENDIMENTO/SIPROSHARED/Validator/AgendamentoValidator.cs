using FluentValidation;
using SIPROSHARED.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHARED.Validator
{
    public class AgendamentoValidator : AbstractValidator<AgendaModel>
    {

        public AgendamentoValidator()
        {


            //RuleFor(user => user.Age_Abertura)
            //     .NotEmpty().WithMessage("A data de abertura não foi fornecida.")
            //     .Matches(@"^(0[1-9]|[12][0-9]|3[01])/(0[1-9]|1[0-2])/\d{4}$").WithMessage("A data de abertura deve estar no formato dd/MM/yyyy.");

            RuleFor(user => user.Age_Doc_Solicitante)
                .NotEmpty().WithMessage("O CPF/CNPJ não foi fornecido.")
                .Must(doc => doc.Length == 11 || doc.Length == 14)
                .WithMessage("O documento deve ter exatamente 11 (CPF) ou 14 (CNPJ) caracteres.")
                .Matches("^[0-9]{11}$|^[0-9]{14}$")
                .WithMessage("O documento deve conter apenas números.");

            RuleFor(user => user.Age_AIT)
                .NotEmpty().WithMessage("O número do AIT não foi fornecido.")
                .Length(10).WithMessage("O AIT deve ter exatamente 10 caracteres.");

            RuleFor(user => user.Age_Cod_Assunto)
               .NotEmpty().WithMessage("O assunto não foi fornecido.");

            RuleFor(user => user.Age_Cod_Origem)
               .NotEmpty().WithMessage("A origem não foi fornecida.");

            //RuleFor(user => user.Ass_Nome)
            //  .NotEmpty().WithMessage("O Assunto é obrigatório.");

        }
    }
}
