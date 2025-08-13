using System.ComponentModel.DataAnnotations;

public class CreateSolicitacaoGeralDto
{
    [Required]
    public long GestorId { get; set; }

    [Required, StringLength(500)]
    public required string JustificativaGeral { get; set; }

    [Required, MinLength(1)]
    public required List<SolicitacaoItemDto> Itens { get; set; }
}
