using System.ComponentModel.DataAnnotations;

namespace Models.Dtos
{
    public class CancelarSolicitacaoDto
    {
        [StringLength(500)]
        public string? Observacoes { get; set; }
    }
}
