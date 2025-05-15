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
    public class DistribuicaoEspecificaValidator : AbstractValidator<ProtocoloDistribuicaoModel>
    {

        public DistribuicaoEspecificaValidator()
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
