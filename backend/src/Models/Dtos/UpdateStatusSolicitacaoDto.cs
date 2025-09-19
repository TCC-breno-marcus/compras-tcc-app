using System.ComponentModel.DataAnnotations;

namespace Models.Dtos
{
    public class UpdateStatusSolicitacaoDto
    {
        [Required]
        public int NovoStatusId { get; set; }

        [StringLength(500)]
        public string? Observacoes { get; set; }
    }
}
