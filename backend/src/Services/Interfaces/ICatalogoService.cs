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
            int pageSize,
            string? sortOrder
        );

        Task ImportarItensAsync(IEnumerable<ItemImportacaoDto> itensParaImportar);

        Task<string> PopularImagensAsync(string caminhoDasImagens);

        Task<ItemDto?> EditarItemAsync(int id, ItemUpdateDto updateDto);
        Task<ItemDto?> GetItemByIdAsync(long id);

        Task<ItemDto> CriarItemAsync(ItemDto dto);

        Task<bool> DeleteItemAsync(long id);

    }
}