using ComprasTccApp.Models.Entities.Solicitacoes;

public interface ISolicitacaoService
{
  Task<SolicitacaoGeral> CreateGeralAsync(CreateSolicitacaoGeralDto dto, long solicitanteId);
  Task<Solicitacao?> GetByIdAsync(long id);
}