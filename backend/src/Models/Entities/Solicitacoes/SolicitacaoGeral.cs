using System.ComponentModel.DataAnnotations;
using ComprasTccApp.Models.Entities.Itens;
using ComprasTccApp.Models.Entities.Solicitacoes;

public class SolicitacaoGeral : Solicitacao
{
    [Required, StringLength(500)]
    public required string JustificativaGeral { get; set; }

    public List<SolicitacaoItem> ItemSolicitacao { get; set; } = new();
}