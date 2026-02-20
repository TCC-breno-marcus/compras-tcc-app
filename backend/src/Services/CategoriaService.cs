using ComprasTccApp.Models.Entities.Categorias;
using Database;
using Microsoft.EntityFrameworkCore;
using Models.Dtos;
using Services.Interfaces;

namespace Services
{
    public class CategoriaService(AppDbContext context, ILogger<CategoriaService> logger)
        : ICategoriaService
    {
        private readonly AppDbContext _context = context;
        private readonly ILogger<CategoriaService> _logger = logger;

        /// <summary>
        /// Consulta categorias com filtros opcionais por identificador, nome, descrição e status de ativação.
        /// </summary>
        /// <param name="id">Lista de identificadores de categoria para filtro exato.</param>
        /// <param name="nome">Lista de termos para busca parcial no nome da categoria.</param>
        /// <param name="descricao">Termo para busca parcial na descrição da categoria.</param>
        /// <param name="isActive">Filtro de status ativo/inativo.</param>
        /// <returns>Lista de categorias que atendem aos filtros informados.</returns>
        /// <exception cref="Exception">Propaga falhas inesperadas durante a consulta ao banco de dados.</exception>
        public async Task<IEnumerable<CategoriaDto>> GetAllCategoriasAsync(
            List<long> id,
            List<string> nome,
            string? descricao,
            bool? isActive
        )
        {
            _logger.LogInformation("Iniciando busca de categorias com filtros...");

            try
            {
                var query = _context.Categorias.AsQueryable();

                if (id != null && id.Count != 0)
                {
                    query = query.Where(categoria => id.Contains(categoria.Id));
                }

                if (nome != null && nome.Count != 0)
                {
                    var nomesLower = nome.Select(n => n.ToLower()).ToList();
                    query = query.Where(categoria =>
                        nomesLower.Any(termoDeBusca =>
                            categoria.Nome.ToLower().Contains(termoDeBusca)
                        )
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

        /// <summary>
        /// Atualiza os dados de uma categoria existente com base nas propriedades informadas no DTO.
        /// </summary>
        /// <param name="id">Identificador da categoria a ser editada.</param>
        /// <param name="updateDto">Dados de atualização parcial da categoria.</param>
        /// <returns>Categoria atualizada; retorna <see langword="null"/> quando a categoria não existe.</returns>
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

        /// <summary>
        /// Recupera uma categoria específica pelo seu identificador.
        /// </summary>
        /// <param name="id">Identificador da categoria.</param>
        /// <returns>Categoria encontrada; retorna <see langword="null"/> quando inexistente.</returns>
        /// <exception cref="Exception">Propaga falhas inesperadas durante a leitura no banco de dados.</exception>
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

        /// <summary>
        /// Cria uma nova categoria validando a unicidade do nome (normalizado por trim e lowercase).
        /// </summary>
        /// <param name="dto">Dados da categoria a ser criada.</param>
        /// <returns>Categoria criada e persistida.</returns>
        /// <exception cref="InvalidOperationException">Lançada quando já existe categoria com o mesmo nome normalizado.</exception>
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

        /// <summary>
        /// Remove uma categoria quando ela existe e não está associada a itens do catálogo.
        /// </summary>
        /// <param name="id">Identificador da categoria a ser removida.</param>
        /// <returns><see langword="true"/> quando removida; <see langword="false"/> quando não encontrada.</returns>
        /// <exception cref="InvalidOperationException">
        /// Lançada quando a categoria está em uso por itens e não pode ser excluída.
        /// </exception>
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
