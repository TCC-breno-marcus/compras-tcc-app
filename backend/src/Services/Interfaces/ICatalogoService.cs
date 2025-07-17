using ComprasTccApp.Models.Dtos;

namespace Services.Interfaces
{
    public interface ICatalogoService
    {
        Task<IEnumerable<ItemDto>> GetAllItensAsync();

        Task ImportarItensAsync(IEnumerable<ItemImportacaoDto> itensParaImportar);

        Task<string> PopularImagensAsync(string caminhoDasImagens);
    }
}