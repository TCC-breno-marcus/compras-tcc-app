using Models.Dtos;

namespace Services.Interfaces
{
    public interface ICentroService
    {
        Task<IEnumerable<CentroDto>> GetAllCentrosAsync(string? nome, string? sigla);

        Task<CentroDto?> GetCentroByIdAsync(long id);
    }
}
