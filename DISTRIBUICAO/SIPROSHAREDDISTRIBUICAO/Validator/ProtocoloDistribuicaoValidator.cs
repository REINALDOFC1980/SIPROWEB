using FluentValidation;
using SIPROSHARED.Models;
using SIPROSHAREDDISTRIBUICAO.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHARED.Validator
{
    public class ProtocoloDistribuicaoValidator : AbstractValidator<ProtocoloDistribuicaoModel>
    {

        public ProtocoloDistribuicaoValidator()
        {
            RuleFor(user => user.DIS_ORIGEM_USUARIO)
               .NotEmpty().WithMessage("O campo usuário origem é obrigatório");

            RuleFor(user => user.DIS_DESTINO_USUARIO)
               .NotEmpty().WithMessage("O campo usuário destino é obrigatório");

            RuleFor(user => user.DIS_MOV_ID)
               .NotEmpty().WithMessage("Código da movimentação é obrigatório.")
               .GreaterThan(0).WithMessage("Código da movimentação deve ser um número maior que zero.");
        }

    }
}
