using ComprasTccApp.Models.Dtos;

namespace Services.Interfaces
{
    public interface ICatalogoService
    {
        Task<IEnumerable<ItemDto>> GetAllItensAsync();
    }
}