using ComprasTccApp.Models.Dtos;

namespace Services.Interfaces
{
    public interface ICategoriaService
    {
        Task<IEnumerable<CategoriaDto>> GetAllCategoriasAsync(
            List<long> ids,
            List<string> nomes,
            string? descricao,
            bool? isActive
        );

        Task<CategoriaDto?> EditarCategoriaAsync(int id, CategoriaUpdateDto updateDto);
        Task<CategoriaDto?> GetCategoriaByIdAsync(long id);
        Task<CategoriaDto> CriarCategoriaAsync(CategoriaDto dto);
        Task<bool> DeleteCategoriaAsync(long id);
    }
}
