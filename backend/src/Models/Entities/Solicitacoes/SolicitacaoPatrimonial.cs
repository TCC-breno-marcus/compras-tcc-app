using ComprasTccApp.Models.Entities.Itens;
using ComprasTccApp.Models.Entities.Solicitacoes;

public class SolicitacaoPatrimonial : Solicitacao
{
    public List<SolicitacaoItem> ItemSolicitacao { get; set; } = new();
}
