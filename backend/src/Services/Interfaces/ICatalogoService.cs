using System.Security.Claims;
using Models.Dtos;

namespace Services.Interfaces
{
    public interface ICatalogoService
    {
        Task<PaginatedResultDto<ItemDto>> GetAllItensAsync(
            long? id,
            string? catMat,
            string? nome,
            string? descricao,
            List<long> categoriaId,
            string? especificacao,
            bool? isActive,
            string? searchTerm,
            int pageNumber,
            int pageSize,
            string? sortOrder
        );

        Task ImportarItensAsync(IEnumerable<ItemImportacaoDto> itensParaImportar);

        Task<string> PopularImagensAsync(string caminhoDasImagens);

        Task<ItemDto?> EditarItemAsync(int id, ItemUpdateDto updateDto, ClaimsPrincipal user);
        Task<ItemDto?> GetItemByIdAsync(long id);

        Task<ItemDto> CriarItemAsync(CreateItemDto dto, ClaimsPrincipal user);

        Task<(bool sucesso, string mensagem)> DeleteItemAsync(long id, ClaimsPrincipal user);

        Task<IEnumerable<ItemDto>?> GetItensSemelhantesAsync(long id);

        Task<ItemDto?> AtualizarImagemAsync(long id, IFormFile imagem, ClaimsPrincipal user);

        Task<bool> RemoverImagemAsync(long id, ClaimsPrincipal user);

        Task<List<HistoricoItemDto>?> GetHistoricoItemAsync(long itemId);
    }
}
