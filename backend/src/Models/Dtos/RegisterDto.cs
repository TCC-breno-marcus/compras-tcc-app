using System.ComponentModel.DataAnnotations;
using ComprasTccApp.Backend.ValidationAttributes;

namespace Models.Dtos
{
    public class RegisterDto
    {
        [Required, StringLength(250)]
        public required string Nome { get; set; }

        [Required, StringLength(300), EmailAddress]
        public required string Email { get; set; }

        [Required, StringLength(20)]
        public required string Telefone { get; set; }

        [Required(ErrorMessage = "O CPF é obrigatório.")]
        [StringLength(
            11,
            MinimumLength = 11,
            ErrorMessage = "O CPF deve conter exatamente 11 dígitos."
        )]
        [CpfValidation(ErrorMessage = "O CPF informado é inválido.")]
        public required string CPF { get; set; }

        [Required, MinLength(6), MaxLength(24)]
        public required string Password { get; set; }

        [Required]
        public required string Departamento { get; set; }
    }
}
