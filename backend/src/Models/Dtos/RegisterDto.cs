using System.ComponentModel.DataAnnotations;

namespace ComprasTccApp.Backend.DTOs
{
    public class RegisterDto
    {
        [Required, StringLength(250)]
        public required string Nome { get; set; }

        [Required, StringLength(300), EmailAddress]
        public required string Email { get; set; }

        [Required, StringLength(20)]
        public required string Telefone { get; set; }

        [Required, StringLength(11)]
        public required string CPF { get; set; }

        [Required, MinLength(6), MaxLength(24)]
        public required string Password { get; set; }
    }
}
