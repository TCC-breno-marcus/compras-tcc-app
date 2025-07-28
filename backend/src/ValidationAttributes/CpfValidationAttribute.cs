using System;
using System.ComponentModel.DataAnnotations;

namespace ComprasTccApp.Backend.ValidationAttributes
{
    public class CpfValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }

            var cpf = value.ToString();

            cpf = cpf?.Trim().Replace(".", "").Replace("-", "");

            if (cpf?.Length != 11)
            {
                return new ValidationResult("O CPF deve conter 11 dígitos.");
            }

            bool todosDigitosIguais = true;
            for (int i = 1; i < 11; i++)
            {
                if (cpf[i] != cpf[0])
                {
                    todosDigitosIguais = false;
                    break;
                }
            }
            if (todosDigitosIguais)
            {
                return new ValidationResult("CPF inválido.");
            }

            try
            {
                var tempCpf = cpf.Substring(0, 9);
                var soma = 0;

                for (int i = 0; i < 9; i++)
                    soma += int.Parse(tempCpf[i].ToString()) * (10 - i);

                var resto = soma % 11;
                var digitoVerificador1 = resto < 2 ? 0 : 11 - resto;

                if (digitoVerificador1 != int.Parse(cpf[9].ToString()))
                    return new ValidationResult("CPF inválido.");

                tempCpf = cpf.Substring(0, 10);
                soma = 0;
                for (int i = 0; i < 10; i++)
                    soma += int.Parse(tempCpf[i].ToString()) * (11 - i);

                resto = soma % 11;
                var digitoVerificador2 = resto < 2 ? 0 : 11 - resto;

                if (digitoVerificador2 != int.Parse(cpf[10].ToString()))
                    return new ValidationResult("CPF inválido.");

                return ValidationResult.Success;
            }
            catch (Exception)
            {
                return new ValidationResult("CPF contém caracteres inválidos.");
            }
        }
    }
}