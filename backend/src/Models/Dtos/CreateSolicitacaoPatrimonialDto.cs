using System.ComponentModel.DataAnnotations;

public class CreateSolicitacaoPatrimonialDto
{
  [Required]
  public long GestorId { get; set; }

  [Required, MinLength(1)]
  public required List<SolicitacaoPatrimonialItemDto> Itens { get; set; }
}