using FluentValidation;
using SIPROSHARED.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHARED.Validator
{
    public class PessoaValidator : AbstractValidator<PessoaModel>
    {

         public PessoaValidator()
         {

            RuleFor(user => user.pes_CPF)
                .NotEmpty().WithMessage("O CPF é obrigatório.")
                .Length(11).WithMessage("O CPF deve ter exatamente 11 caracteres.")
                .Matches("^[0-9]{11}$").WithMessage("O CPF deve conter apenas números.");

            RuleFor(user => user.pes_Nome)
                .NotEmpty().WithMessage("O Nome é obrigatório.")
                .MinimumLength(3).WithMessage("O Nome deve ter pelo menos 3 caracteres.")
                .MaximumLength(100).WithMessage("O Nome deve ter no máximo 100 caracteres.")
                .Matches(@"^[a-zA-ZÀ-ÖØ-öø-ÿ\s]+$").WithMessage("O Nome deve conter apenas letras.");

            RuleFor(user => user.pes_EndLogradouro)
                .NotEmpty().WithMessage("O Logradouro é obrigatório.")
                .MinimumLength(5).WithMessage("O Logradouro  deve ter pelo menos 5 caracteres.")
                .MaximumLength(50).WithMessage("O Logradouro  deve ter no máximo 50 caracteres.");

            RuleFor(user => user.pes_EndNumero)
                .NotEmpty().WithMessage("O Número do Endereço é obrigatório.")
                .Matches(@"^\d+$").WithMessage("O Número do Endereço deve conter apenas números.")
                .MaximumLength(5).WithMessage("O Número do Endereço deve ter no máximo 5 caracteres.");

            RuleFor(user => user.pes_EndComplemento)
                .MaximumLength(50).WithMessage("O Complemento  deve ter no máximo 50 caracteres.");

            RuleFor(user => user.pes_EndBairro)
                .NotEmpty().WithMessage("O Bairro é obrigatório.")
                .MinimumLength(5).WithMessage("O Bairro  deve ter pelo menos 5 caracteres.")
                .MaximumLength(50).WithMessage("O Bairro  deve ter no máximo 50 caracteres.");

            RuleFor(user => user.pes_Municipio)
                .NotEmpty().WithMessage("O Municipio é obrigatório.")
                .MinimumLength(5).WithMessage("O Municipio  deve ter pelo menos 5 caracteres.")
                .MaximumLength(50).WithMessage("O Municipio  deve ter no máximo 50 caracteres.");

            var ufsValidas = new[]
            {
                "AC", "AL", "AP", "AM", "BA", "CE", "DF", "ES", "GO",
                "MA", "MT", "MS", "MG", "PA", "PB", "PR", "PE", "PI",
                "RJ", "RN", "RS", "RO", "RR", "SC", "SP", "SE", "TO"
            };

            RuleFor(user => user.pes_UF)
                .NotEmpty().WithMessage("A UF é obrigatória.")
                .Length(2).WithMessage("A UF deve ter exatamente 2 caracteres.")
                .Matches(@"^[A-Z]{2}$").WithMessage("A UF deve conter apenas letras maiúsculas.")
                .Must(uf => ufsValidas.Contains(uf)).WithMessage("A UF informada é inválida.");

            //RuleFor(user => user.pes_UFCNH)
            //    .NotEmpty().WithMessage("A UF da CNH é obrigatória.")
            //    .Length(2).WithMessage("A UF da CNH deve ter exatamente 2 caracteres.")
            //    .Matches(@"^[A-Z]{2}$").WithMessage("A UF da CNH deve conter apenas letras maiúsculas.")
            //    .Must(uf => ufsValidas.Contains(uf)).WithMessage("A UF da CNH informada é inválida.");

            //RuleFor(user => user.pes_NumRegistroCNH)
            //    .NotEmpty().WithMessage("O número do registro é obrigatório.")
            //    .Length(11).WithMessage("O número do registro deve ter exatamente 11 caracteres.")
            //    .Matches("^[0-9]{11}$").WithMessage("O número do registro deve conter apenas números.");

            //RuleFor(user => user.pes_DT_Validade)
            //    .NotEmpty().WithMessage("A data de validade da CNH é obrigatória.")
            //    .Matches(@"^(0[1-9]|[12][0-9]|3[01])/(0[1-9]|1[0-2])/\d{4}$").WithMessage("A data de validade da CNH deve estar no formato dd/MM/yyyy.");

            RuleFor(user => user.pes_Email)
                .NotEmpty().WithMessage("O E-mail é obrigatório.")
                .EmailAddress().WithMessage("O E-mail informado não é válido.");

            RuleFor(user => user.pes_Celular)
                .NotEmpty().WithMessage("O Celular é obrigatório.")
                .Matches(@"^\(?\d{2}\)?\s?\d{5}-\d{4}$")
                .WithMessage("O número de celular informado não é válido.");

            RuleFor(user => user.pes_Telefone)
                .Matches(@"^\(?\d{2}\)?\s?\d{4}-\d{4}$")
                .WithMessage("O número de celular informado não é válido.");

            //RuleFor(user => user.pes_Pais)
            //    .NotEmpty().WithMessage("O País é obrigatório.");
         }

    }
}
