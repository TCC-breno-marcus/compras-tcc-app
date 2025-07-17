using ComprasTccApp.Models.Dtos;
using Database;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;
using ComprasTccApp.Backend.Models.Entities.Items;

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
                        Nome = item.Nome,
                        Descricao = item.Descricao,
                        CatMat = item.CatMat,
                        LinkImagem = item.LinkImagem,
                        Especificacao = item.LinkImagem,
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

        public async Task ImportarItensAsync(IEnumerable<ItemImportacaoDto> itensParaImportar)
        {
            var novosItens = itensParaImportar.Select(dto => new Item
            {
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                CatMat = dto.Codigo,
                LinkImagem = dto.LinkImagem,
                Especificacao = dto.Especificacao,
                IsActive = true
            });

            await _context.Items.AddRangeAsync(novosItens);

            await _context.SaveChangesAsync();

            _logger.LogInformation("{Count} itens foram importados com sucesso para o catálogo.", novosItens.Count());
        }
    }
}