using System.ComponentModel.DataAnnotations;
using ComprasTccApp.Models.Entities.Servidores;

namespace ComprasTccApp.Models.Entities.Pessoas
{
    public class Pessoa
    {
        public long Id { get; set; }

        [Required, StringLength(120)]
        public required string Nome { get; set; }

        [Required, StringLength(120)]
        public required string Email { get; set; }

        [Required, StringLength(20)]
        public required string Telefone { get; set; }

        [Required, StringLength(20)]
        public required string CPF { get; set; }

        [Required]
        public DateTime DataAtualizacao { get; set; }

        [Required]
        public required string PasswordHash { get; set; }

        [Required, StringLength(50)]
        public required string Role { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        public Servidor? Servidor { get; set; }
    }
}
