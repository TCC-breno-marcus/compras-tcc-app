public interface ISolicitacaoService
{
  Task<SolicitacaoGeral> CreateGeralAsync(CreateSolicitacaoGeralDto dto, long solicitanteId);
}