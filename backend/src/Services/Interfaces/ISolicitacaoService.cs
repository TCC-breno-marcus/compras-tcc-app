using ComprasTccApp.Models.Entities.Solicitacoes;
using Models.Dtos;

public interface ISolicitacaoService
{
    Task<SolicitacaoResultDto> CreateGeralAsync(CreateSolicitacaoGeralDto dto, long pessoaId);
    Task<Solicitacao?> GetByIdAsync(long id);
    Task<SolicitacaoResultDto> CreatePatrimonialAsync(
        CreateSolicitacaoPatrimonialDto dto,
        long pessoaId
    );
}
