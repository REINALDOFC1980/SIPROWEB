using FluentValidation;
using SIPROSHARED.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIPROSHARED.Validator
{
    public class ProtocoloValidator : AbstractValidator<ProtocoloModel>
    {
        public ProtocoloValidator()
        {

            RuleFor(user => user.PRT_ORIGEM)
               .NotEmpty().WithMessage("A origem é obrigatória.")
               .GreaterThan(0).WithMessage("A origem deve ser um número maior que zero.")
               .LessThanOrEqualTo(9999).WithMessage("A origem não pode exceder 9999.");

            RuleFor(user => user.PRT_ASSUNTO)
               .NotEmpty().WithMessage("A origem é obrigatória.")
               .GreaterThan(0).WithMessage("A origem deve ser um número maior que zero.")
               .LessThanOrEqualTo(9999).WithMessage("A origem não pode exceder 9999.");

            RuleFor(user => user.PRT_ATENDENTE)
                .NotEmpty().WithMessage("O usuário do atendimento é obrigatório.")
                .MinimumLength(3).WithMessage("O usuário do atendimento deve ter pelo menos 3 caracteres.")
                .MaximumLength(100).WithMessage("A atendente deve ter no máximo 20 caracteres.");
            
            RuleFor(user => user.PRT_AIT)
               .NotEmpty().WithMessage("O AIT é obrigatório.")
               .Length(10).WithMessage("O AIT deve ter exatamente 10 caracteres.");

            RuleFor(user => user.PRT_TIPO_SOLICITANTE)
               .NotEmpty().WithMessage("O AIT é obrigatório.")
               .MinimumLength(3).WithMessage("A atendente deve ter pelo menos 2 caracteres.")
               .MaximumLength(100).WithMessage("A atendente deve ter no máximo 20 caracteres.");


            RuleFor(user => user.PRT_CPF_SOLICITANTE)
                .NotEmpty().WithMessage("O CPF/CNPJ não foi fornecido.")
                .Must(doc => doc.Length == 11 || doc.Length == 14)
                .WithMessage("O documento deve ter exatamente 11 (CPF) ou 14 (CNPJ) caracteres.")
                .Matches("^[0-9]{11}$|^[0-9]{14}$")
                .WithMessage("O documento deve conter apenas números.");



            RuleFor(user => user.PRT_CPFCNJ_PROPRIETARIO)
               .NotEmpty().WithMessage("O CPF do proprietário é obrigatório.");


            RuleFor(user => user.PRT_CPF_CONDUTOR)
               .NotEmpty().WithMessage("O CPF do condutor é obrigatório.")
               .Length(11).WithMessage("O CPF do solicitante deve ter exatamente 11 caracteres.")
               .Matches("^[0-9]{11}$").WithMessage("O CPF do condutor  deve conter apenas números.")
               .Must(ValidarCpfCnpj).WithMessage("O CPF do condutor  informado é inválido.")
               .When(user => new[] { 1, 39 }.Contains(user.PRT_ASSUNTO));

            RuleFor(user => user.PRT_NUMREGISTRO_CNH)
               .NotEmpty().WithMessage("O número de registro da CNH é obrigatório.")
               .Length(11).WithMessage("O número de registro da CNH deve ter exatamente 11 dígitos.")
               .Matches(@"^\d{11}$").WithMessage("O número de registro da CNH deve conter apenas números.")
               .When(user => new[] { 1, 39 }.Contains(user.PRT_ASSUNTO));

            RuleFor(user => user.PRT_DT_VALIDADE)
               .NotEmpty().WithMessage("A data de validade da CNH é obrigatória.")
               .Must(date => DateTime.TryParse(date, out _)).WithMessage("A data de validade da CNH deve ser válida.")
               .Must(date => DateTime.Parse(date) >= DateTime.Today).WithMessage("A data de validade da CNH deve ser maior ou igual à data atual.")
               .When(user => new[] { 1, 39 }.Contains(user.PRT_ASSUNTO));

            RuleFor(user => user.PRT_PLACA)
               .NotEmpty().WithMessage("A placa é obrigatória.")
               .Length(7).WithMessage("A placa deve ter exatamente 7 caracteres.")
               .Matches(@"^[A-Z]{3}[0-9][A-Z][0-9]{2}$|^[A-Z]{3}[0-9]{4}$").WithMessage("A placa deve estar no formato válido (AAA1A11 ou AAA1111).");

             RuleFor(user => user.PRT_DT_COMETIMENTO)
                .NotEmpty().WithMessage("A data do cometimento é obrigatória.")
                .Matches(@"^(0[1-9]|[12][0-9]|3[01])/(0[1-9]|1[0-2])/\d{4} ([01][0-9]|2[0-3]):[0-5][0-9]$")
                .WithMessage("A data do cometimento deve estar no formato dd/MM/yyyy HH:mm.");

            RuleFor(user => user.PRT_DT_PRAZO)
               .NotEmpty().WithMessage("A data do prazo é obrigatória.")
               .Matches(@"^(0[1-9]|[12][0-9]|3[01])/(0[1-9]|1[0-2])/\d{4}$").WithMessage("A data do cometimento deve estar no formato dd/MM/yyyy.");

            RuleFor(user => user.PRT_RESTRICAO)
                .NotEmpty().WithMessage("A restrição é obrigatória.")
                .GreaterThan(0).WithMessage("A restrição deve ser maior que 0.");

            RuleFor(user => user.PRT_AIT_SITUACAO)
                .NotEmpty().WithMessage("A situação do AIT é obrigatória.")
                .MinimumLength(3).WithMessage("A atendente deve ter pelo menos 2 caracteres.")
                .MaximumLength(100).WithMessage("A atendente deve ter no máximo 20 caracteres.");
        }

        //private bool IsValidRenavam(string renavam)
        //{
        //    if (string.IsNullOrWhiteSpace(renavam) || renavam.Length != 11)
        //        return false;

        //    // Pega os 10 primeiros números
        //    var baseRenavam = renavam.Substring(0, 10);

        //    // Converte para array de inteiros
        //    var numeros = baseRenavam.Select(c => int.Parse(c.ToString())).ToArray();

        //    // Multiplicadores de validação
        //    int[] multiplicadores = { 2, 3, 4, 5, 6, 7, 8, 9 };

        //    // Cálculo do dígito verificador
        //    int soma = 0;
        //    for (int i = 0; i < numeros.Length; i++)
        //    {
        //        soma += numeros[(numeros.Length - 1) - i] * multiplicadores[i % multiplicadores.Length];
        //    }

        //    int resto = soma % 11;
        //    int digitoVerificador = (resto == 0 || resto == 1) ? 0 : 11 - resto;

        //    // Compara o dígito calculado com o último número do RENAVAM
        //    return digitoVerificador == int.Parse(renavam[10].ToString());
        //}

        private bool ValidarCpfCnpj(string cpfCnpj)
        {
            if (string.IsNullOrEmpty(cpfCnpj))
                return false;

            cpfCnpj = cpfCnpj.Replace(".", "").Replace("-", "").Replace("/", "");

            if (cpfCnpj.Length == 11)
                return ValidarCpf(cpfCnpj);

            if (cpfCnpj.Length == 14)
                return ValidarCnpj(cpfCnpj);

            return false;
        }

        private bool ValidarCpf(string cpf)
        {
            if (cpf.Length != 11 || cpf.All(c => c == cpf[0]))
                return false;

            int[] multiplicador1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCpf = cpf.Substring(0, 9);
            int soma = tempCpf.Select((t, i) => int.Parse(t.ToString()) * multiplicador1[i]).Sum();
            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            tempCpf += digito1;
            soma = tempCpf.Select((t, i) => int.Parse(t.ToString()) * multiplicador2[i]).Sum();
            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            return cpf.EndsWith(digito1.ToString() + digito2.ToString());
        }

        private bool ValidarCnpj(string cnpj)
        {
            if (cnpj.Length != 14 || cnpj.All(c => c == cnpj[0]))
                return false;

            int[] multiplicador1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            string tempCnpj = cnpj.Substring(0, 12);
            int soma = tempCnpj.Select((t, i) => int.Parse(t.ToString()) * multiplicador1[i]).Sum();
            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            tempCnpj += digito1;
            soma = tempCnpj.Select((t, i) => int.Parse(t.ToString()) * multiplicador2[i]).Sum();
            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            return cnpj.EndsWith(digito1.ToString() + digito2.ToString());
        }
    }
}
