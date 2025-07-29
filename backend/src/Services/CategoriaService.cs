using ComprasTccApp.Models.Dtos;
using ComprasTccApp.Models.Entities.Categorias;
using Database;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Services
{
    public class CategoriaService(AppDbContext context, ILogger<CategoriaService> logger)
        : ICategoriaService
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<CategoriaService> _logger = logger;

        public async Task<IEnumerable<CategoriaDto>> GetAllCategoriasAsync(
            long? id,
            string? nome,
            string? descricao,
            bool? isActive
        )
        {
            _logger.LogInformation("Iniciando busca de categorias com filtros...");

            try
            {
                var query = _context.Categorias.AsQueryable();

                if (id.HasValue)
                {
                    query = query.Where(categoria => categoria.Id == id.Value);
                }

                if (!string.IsNullOrWhiteSpace(nome))
                {
                    query = query.Where(categoria =>
                        categoria.Nome.ToLower().Contains(nome.ToLower())
                    );
                }
                if (!string.IsNullOrWhiteSpace(descricao))
                {
                    query = query.Where(categoria =>
                        categoria.Descricao.ToLower().Contains(descricao.ToLower())
                    );
                }

                if (isActive.HasValue)
                {
                    query = query.Where(categoria => categoria.IsActive == isActive.Value);
                }

                var categoriasDto = await query
                    .Select(categoria => new CategoriaDto
                    {
                        Id = categoria.Id,
                        Nome = categoria.Nome,
                        Descricao = categoria.Descricao,
                        IsActive = categoria.IsActive,
                    })
                    .ToListAsync();

                _logger.LogInformation(
                    "Busca concluída. Retornando {Count} categorias.",
                    categoriasDto.Count
                );

                return categoriasDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ocorreu um erro ao buscar as categorias do banco de dados com filtros."
                );
                throw;
            }
        }

        public async Task<CategoriaDto?> EditarCategoriaAsync(int id, CategoriaUpdateDto updateDto)
        {
            var categoriaDoBanco = await _context.Categorias.FirstOrDefaultAsync(i => i.Id == id);

            if (categoriaDoBanco == null)
            {
                _logger.LogWarning("Tentativa de editar categoria com ID {Id} não encontrada.", id);
                return null;
            }

            if (!string.IsNullOrEmpty(updateDto.Nome))
                categoriaDoBanco.Nome = updateDto.Nome;

            if (!string.IsNullOrEmpty(updateDto.Descricao))
                categoriaDoBanco.Descricao = updateDto.Descricao;

            if (updateDto.IsActive.HasValue)
                categoriaDoBanco.IsActive = updateDto.IsActive.Value;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Categoria com ID {Id} atualizada.", id);

            return new CategoriaDto
            {
                Id = categoriaDoBanco.Id,
                Nome = categoriaDoBanco.Nome,
                Descricao = categoriaDoBanco.Descricao,
                IsActive = categoriaDoBanco.IsActive,
            };
        }

        public async Task<CategoriaDto?> GetCategoriaByIdAsync(long id)
        {
            _logger.LogInformation("Iniciando busca de uma categoria pelo ID...");
            _logger.LogWarning("ID {Id}", id);

            try
            {
                var categoria = await _context
                    .Categorias.AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Id == id);

                if (categoria == null)
                {
                    _logger.LogWarning("Categoria com ID {Id} não encontrada.", id);
                    return null;
                }

                _logger.LogInformation("Categoria com ID {Id} encontrada.", id);

                return new CategoriaDto
                {
                    Id = categoria.Id,
                    Nome = categoria.Nome,
                    Descricao = categoria.Descricao,
                    IsActive = categoria.IsActive,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar categoria por ID {Id}.", id);
                throw;
            }
        }

        public async Task<CategoriaDto> CriarCategoriaAsync(CategoriaDto dto)
        {
            var nomeNormalizado = dto.Nome.Trim().ToLower();

            var categoriaExistente = await _context.Categorias.AnyAsync(categoria =>
                categoria.Nome.ToLower() == nomeNormalizado
            );

            if (categoriaExistente)
            {
                throw new InvalidOperationException(
                    $"Já existe uma categoria cadastrada com o nome {dto.Nome}."
                );
            }

            var novaCategoria = new Categoria
            {
                Nome = dto.Nome.Trim(),
                Descricao = dto.Descricao,
                IsActive = true,
            };

            await _context.Categorias.AddAsync(novaCategoria);
            await _context.SaveChangesAsync();

            return new CategoriaDto
            {
                Id = novaCategoria.Id,
                Nome = novaCategoria.Nome,
                Descricao = novaCategoria.Descricao,
                IsActive = novaCategoria.IsActive,
            };
        }

        public async Task<bool> DeleteCategoriaAsync(long id)
        {
            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria == null)
            {
                _logger.LogWarning(
                    "Tentativa de deletar categoria com ID {Id}, mas não foi encontrada.",
                    id
                );
                return false;
            }

            var isCategoriaEmUso = await _context.Items.AnyAsync(item => item.CategoriaId == id);

            if (isCategoriaEmUso)
            {
                _logger.LogWarning(
                    "Tentativa de deletar categoria com ID {Id} que está em uso.",
                    id
                );
                throw new InvalidOperationException(
                    "Esta categoria não pode ser excluída pois está sendo utilizada por um ou mais itens."
                );
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Categoria com ID {Id} foi deletada com sucesso.", id);
            return true;
        }
    }
}
