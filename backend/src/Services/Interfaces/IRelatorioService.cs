using Models.Dtos;

namespace Services.Interfaces
{
    public interface IRelatorioService
    {
        Task<PaginatedResultDto<ItemPorDepartamentoDto>> GetItensPorDepartamentoAsync(
            int pageNumber,
            int pageSize,
            string? sortOrder
        );
    }
}
