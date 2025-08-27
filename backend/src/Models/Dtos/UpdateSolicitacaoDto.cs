using System.ComponentModel.DataAnnotations;

namespace Models.Dtos
{
    public class UpdateSolicitacaoDto
    {
        public string? JustificativaGeral { get; set; }

        [Required, MinLength(1)]
        public required List<SolicitacaoItemDto> Itens { get; set; }
    }
}
