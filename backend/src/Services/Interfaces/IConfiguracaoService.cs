using ComprasTccApp.Models.Entities.Configuracoes;
using Models.Dtos;

namespace Services.Interfaces
{
    public interface IConfiguracaoService
    {
        Task<DateTime?> GetPrazoSubmissaoAsync();
        Task SetPrazoSubmissaoAsync(DateTime novaData);
    }
}
