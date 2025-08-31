using ComprasTccApp.Models.Entities.Configuracoes;
using Models.Dtos;

namespace Services.Interfaces
{
    public interface IConfiguracaoService
    {
        Task<ConfiguracaoDto> GetConfiguracoesAsync();
        Task UpdateConfiguracoesAsync(UpdateConfiguracaoDto dto);
    }
}
