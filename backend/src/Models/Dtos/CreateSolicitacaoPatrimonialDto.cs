using System.ComponentModel.DataAnnotations;

namespace Models.Dtos
{
    public class CreateSolicitacaoPatrimonialDto
    {
        public long? GestorId { get; set; }

        [Required, MinLength(1)]
        public required List<SolicitacaoPatrimonialItemDto> Itens { get; set; }
    }
}
