using ComprasTccApp.Models.Dtos;

namespace Services.Interfaces
{
    public interface ICategoriaService
    {
        Task<IEnumerable<CategoriaDto>> GetAllCategoriasAsync
        (
            long? id,
            string? nome,
            string? descricao,
            bool? isActive
        );

        Task<CategoriaDto?> EditarCategoriaAsync(int id, CategoriaUpdateDto updateDto);
        Task<CategoriaDto?> GetCategoriaByIdAsync(long id);
        Task<CategoriaDto> CriarCategoriaAsync(CategoriaDto dto);
        Task<bool> DeleteCategoriaAsync(long id);

    }
}