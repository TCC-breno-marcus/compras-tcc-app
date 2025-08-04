using System.ComponentModel.DataAnnotations;

public class SolicitacaoPatrimonialItemDto
{
  [Required]
  public long ItemId { get; set; }

  [Required]
  [Range(1, 1000)]
  public decimal Quantidade { get; set; }

  [Required, StringLength(500)]
  public required string Justificativa { get; set; }
}