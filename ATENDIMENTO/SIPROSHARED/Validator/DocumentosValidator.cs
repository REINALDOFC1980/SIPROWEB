using FluentValidation;
using SIPROSHARED.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHARED.Validator
{
    public class DocumentosValidator : AbstractValidator<DocumentosModel>
    {

        public DocumentosValidator()
        {

            RuleFor(user => user.Doc_nome)
                .NotEmpty().WithMessage("O nome do documento é obrigatório.");
        }
    }
}
