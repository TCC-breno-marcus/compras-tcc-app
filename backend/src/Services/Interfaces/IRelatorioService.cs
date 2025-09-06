using Models.Dtos;

namespace Services.Interfaces
{
    public interface IRelatorioService
    {
        Task<PaginatedResultDto<ItemPorDepartamentoDto>> GetItensPorDepartamentoAsync(
            string? searchTerm,
            string? categoriaNome,
            string? departamento,
            string? sortOrder,
            int pageNumber,
            int pageSize
        );

        Task<byte[]> GetAllItensPorDepartamentoCsvAsync(
            bool isGeral,
            string? searchTerm,
            string? categoriaNome,
            string? departamento
        );
    }
}
