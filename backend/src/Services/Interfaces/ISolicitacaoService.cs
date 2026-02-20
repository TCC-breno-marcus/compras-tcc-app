using System.Security.Claims;
using Models.Dtos;

public interface ISolicitacaoService
{
    Task<SolicitacaoResultDto> CreateGeralAsync(CreateSolicitacaoGeralDto dto, long pessoaId);
    Task<SolicitacaoResultDto?> GetByIdAsync(long id);
    Task<SolicitacaoResultDto> CreatePatrimonialAsync(
        CreateSolicitacaoPatrimonialDto dto,
        long pessoaId
    );

    Task<SolicitacaoResultDto?> EditarSolicitacaoAsync(
        long id,
        long pessoaId,
        bool isAdmin,
        UpdateSolicitacaoDto dto
    );
    Task<PaginatedResultDto<SolicitacaoResultDto>> GetAllBySolicitanteAsync(
        long pessoaId,
        long? gestorId,
        string? tipo,
        DateTime? dataInicial,
        DateTime? dataFinal,
        string? externalId,
        List<int>? statusIds,
        string? sortOrder,
        int pageNumber,
        int pageSize
    );

    Task<PaginatedResultDto<SolicitacaoResultDto>> GetAllAsync(
        long? solicitanteId,
        long? gestorId,
        string? tipo,
        string? siglaDepartamento,
        DateTime? dataInicial,
        DateTime? dataFinal,
        string? externalId,
        List<int>? statusIds,
        string? sortOrder,
        int pageNumber,
        int pageSize
    );

    Task<SolicitacaoResultDto?> AtualizarStatusSolicitacaoAsync(
        long id,
        long pessoaId,
        UpdateStatusSolicitacaoDto dto
    );

    Task<SolicitacaoResultDto?> CancelarSolicitacaoAsync(
        long id,
        long pessoaId,
        bool isAdmin,
        CancelarSolicitacaoDto dto
    );

    Task<List<HistoricoSolicitacaoDto>?> GetHistoricoAsync(
        long solicitacaoId,
        ClaimsPrincipal user
    );

    // Arquiva (marca como Encerrada) todas as solicitações com DataCriacao em anos anteriores ao ano informado.
    // Retorna a quantidade de solicitações alteradas.
    Task<int> ArchiveOldSolicitationsAsync(int anoReferencia, long executorPessoaId);
}
