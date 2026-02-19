using Models.Dtos;

namespace Services.Interfaces
{
    public interface IRelatorioService
    {
        Task<PaginatedResultDto<ItemPorDepartamentoDto>> GetItensPorDepartamentoAsync(
            string? searchTerm,
            string? categoriaNome,
            string? itemsType,
            string? siglaDepartamento,
            bool? somenteSolicitacoesAtivas,
            string? sortOrder,
            int pageNumber,
            int pageSize
        );

        Task<byte[]> ExportarItensPorDepartamentoAsync(
            string itemsType,
            string formatoArquivo,
            string? searchTerm,
            string? categoriaNome,
            string? siglaDepartamento,
            bool? somenteSolicitacoesAtivas,
            string? usuarioSolicitante = null,
            DateTimeOffset? dataHoraSolicitacao = null
        );
    }
}
