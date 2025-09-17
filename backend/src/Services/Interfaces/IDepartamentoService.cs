using Models.Dtos;

namespace Services.Interfaces
{
    public interface IDepartamentoService
    {
        Task<IEnumerable<DepartamentoDto>> GetAllDepartamentosAsync(
            string? nome,
            string? sigla,
            string? siglaCentro
        );

        Task<DepartamentoDto?> GetDepartamentoByIdAsync(long id);
    }
}
