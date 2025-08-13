using System.ComponentModel.DataAnnotations;

public class SolicitacaoItemDto
{
    [Required]
    public long ItemId { get; set; }

    [Required, Range(1, double.MaxValue)]
    public decimal Quantidade { get; set; }
    public string? Justificativa { get; set; }
}
