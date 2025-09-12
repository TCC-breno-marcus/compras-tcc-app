using ComprasTccApp.Models.Entities.Servidores;
using ComprasTccApp.Models.Entities.Solicitantes;
using Models.Dtos;

namespace ComprasTccApp.Services.Interfaces
{
    public interface IUsuarioService
    {
        Task<PaginatedResultDto<UserProfileDto>> GetAllUsersAsync(
            string? role,
            int pageNumber,
            int pageSize,
            string? sortOrder,
            bool? isActive
        );

        Task<(Servidor servidor, Solicitante solicitante)> GetSolicitanteInfoAsync(long pessoaId);

        Task<bool> InativarUsuarioAsync(long id);

        Task<bool> AtivarUsuarioAsync(long id);

    }
}
