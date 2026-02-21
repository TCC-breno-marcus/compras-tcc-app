using Models.Dtos;

namespace Services.Interfaces
{
    public interface IDadosPublicosService
    {
        Task<PublicoSolicitacaoConsultaResultDto> ConsultarSolicitacoesAsync(
            DateTime? dataInicio,
            DateTime? dataFim,
            int? statusId,
            string? statusNome,
            string? siglaDepartamento,
            string? categoriaNome,
            string? itemNome,
            string? catMat,
            string? itemsType,
            decimal? valorMinimo,
            decimal? valorMaximo,
            bool? somenteSolicitacoesAtivas,
            int pageNumber,
            int pageSize
        );

        Task<byte[]> ExportarSolicitacoesCsvAsync(
            DateTime? dataInicio,
            DateTime? dataFim,
            int? statusId,
            string? statusNome,
            string? siglaDepartamento,
            string? categoriaNome,
            string? itemNome,
            string? catMat,
            string? itemsType,
            decimal? valorMinimo,
            decimal? valorMaximo,
            bool? somenteSolicitacoesAtivas,
            int pageNumber,
            int pageSize
        );

        Task<byte[]> ExportarSolicitacoesPdfAsync(
            DateTime? dataInicio,
            DateTime? dataFim,
            int? statusId,
            string? statusNome,
            string? siglaDepartamento,
            string? categoriaNome,
            string? itemNome,
            string? catMat,
            string? itemsType,
            decimal? valorMinimo,
            decimal? valorMaximo,
            bool? somenteSolicitacoesAtivas,
            int pageNumber,
            int pageSize
        );
    }
}
