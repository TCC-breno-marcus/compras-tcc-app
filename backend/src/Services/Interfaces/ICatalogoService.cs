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
            long? categoriaId,
            string? especificacao,
            bool? isActive,
            string? searchTerm,
            int pageNumber,
            int pageSize,
            string? sortOrder
        );

        Task ImportarItensAsync(IEnumerable<ItemImportacaoDto> itensParaImportar);

        Task<string> PopularImagensAsync(string caminhoDasImagens);

        Task<ItemDto?> EditarItemAsync(int id, ItemUpdateDto updateDto);
        Task<ItemDto?> GetItemByIdAsync(long id);

        Task<ItemDto> CriarItemAsync(CreateItemDto dto);

        Task<bool> DeleteItemAsync(long id);

        Task<IEnumerable<ItemDto>?> GetItensSemelhantesAsync(long id);

        Task<ItemDto?> AtualizarImagemAsync(long id, IFormFile imagem);

        Task<bool> RemoverImagemAsync(long id);
    }
}