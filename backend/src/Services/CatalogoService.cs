using ComprasTccApp.Models.Dtos;
using Database;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Services
{
    public class CatalogoService : ICatalogoService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<CatalogoService> _logger;

        public CatalogoService
        (
            AppDbContext context,
             ILogger<CatalogoService> logger
        )
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<ItemDto>> GetAllItensAsync()
        {
            _logger.LogInformation("Buscando todos os itens do catálogo no serviço...");

            try
            {
                var itensDoBanco = await _context.Items.ToListAsync();

                var itensDto = itensDoBanco
                    .Select(item => new ItemDto
                    {
                        Id = item.Id,
                        Descricao = item.Descricao,
                        CatMat = item.CatMat,
                        LinkImagem = item.LinkImagem,
                        UnidadeMedida = item.UnidadeMedida,
                        IsActive = item.IsActive,
                    })
                    .ToList();

                _logger.LogInformation("Encontrados {Count} itens.", itensDto.Count);

                return itensDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocorreu um erro ao buscar os itens do banco de dados.");
                throw;
            }
        }
    }
}