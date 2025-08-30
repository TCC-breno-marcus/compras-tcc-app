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
        string? sortOrder,
        int pageNumber,
        int pageSize
    );

    Task<PaginatedResultDto<SolicitacaoResultDto>> GetAllAsync(
        long? solicitanteId,
        long? gestorId,
        string? tipo,
        DateTime? dataInicial,
        DateTime? dataFinal,
        string? externalId,
        string? sortOrder,
        int pageNumber,
        int pageSize
    );
}
