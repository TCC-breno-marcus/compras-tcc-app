using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ComprasTccApp.Models.Entities.Status
{
    public class StatusSolicitacao
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public required string Nome { get; set; }

        [StringLength(250)]
        public string? Descricao { get; set; }
    }
}
