using ComprasTccApp.Models.Entities.Solicitacoes;

namespace ComprasTccApp.Models.Entities.Historicos
{
    public class HistoricoSolicitacao : HistoricoBase
    {
        public long SolicitacaoId { get; set; }
        public Solicitacao Solicitacao { get; set; } = null!;
    }
}
