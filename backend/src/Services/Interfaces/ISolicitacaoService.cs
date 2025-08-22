using ComprasTccApp.Models.Entities.Solicitacoes;
using Models.Dtos;

public interface ISolicitacaoService
{
    Task<SolicitacaoResultDto> CreateGeralAsync(CreateSolicitacaoGeralDto dto, long pessoaId);
    Task<SolicitacaoResultDto?> GetByIdAsync(long id);
    Task<SolicitacaoResultDto> CreatePatrimonialAsync(
        CreateSolicitacaoPatrimonialDto dto,
        long pessoaId
    );
    Task<PaginatedResultDto<SolicitacaoResultDto>> GetAllBySolicitanteAsync(
        long pessoaId,
        int pageNumber,
        int pageSize
    );
}
