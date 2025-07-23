using ComprasTccApp.Models.Entities.Pessoas;

namespace ComprasTccApp.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateJwtToken(Pessoa pessoa);
    }
}