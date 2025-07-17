using ComprasTccApp.Models.Dtos;

namespace Services.Interfaces
{
    public interface ICatalogoService
    {
        Task<PaginatedResultDto<ItemDto>> GetAllItensAsync
        (
            long? id,
            string? catMat,
            string? nome,
            string? descricao,
            string? especificacao,
            bool? isActive,
            int pageNumber,
            int pageSize
        );

        Task ImportarItensAsync(IEnumerable<ItemImportacaoDto> itensParaImportar);

        Task<string> PopularImagensAsync(string caminhoDasImagens);
    }
}