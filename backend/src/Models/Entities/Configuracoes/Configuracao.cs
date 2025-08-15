using System.ComponentModel.DataAnnotations;

namespace ComprasTccApp.Models.Entities.Configuracoes
{
    public class Configuracao
    {
        [Key]
        [StringLength(100)]
        public required string Chave { get; set; }

        [Required]
        public required string Valor { get; set; }
    }
}
