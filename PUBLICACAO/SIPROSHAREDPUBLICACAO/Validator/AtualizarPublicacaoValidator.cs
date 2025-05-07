using FluentValidation;
using SIPROSHAREDPUBLICACAO.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHAREDPUBLICACAO.Validator
{
    public class AtualizarPublicacaoValidator : AbstractValidator<PublicacaoModel>
    {

        public AtualizarPublicacaoValidator()
        {

            RuleFor(user => user.prt_dt_publicacao)
               .NotEmpty().WithMessage("A data da publicação é obirgatória");

            RuleFor(user => user.prt_publicacao_dom)
               .NotEmpty().WithMessage("O número do DOM é obrigatório.");

            RuleFor(user => user.prt_lote)
               .NotEmpty().WithMessage("Número do lote é obrigatório.");

            RuleFor(user => user.prt_usu_publicacao)
               .NotEmpty().WithMessage("Usuario da publicação é Obrigatório.");
        }
    }
}
