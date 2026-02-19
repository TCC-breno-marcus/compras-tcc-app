using Database;
using Microsoft.EntityFrameworkCore;
using Models.Dtos;
using Services.Interfaces;

public class CentroService : ICentroService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DepartamentoService> _logger;

    public CentroService(AppDbContext context, ILogger<DepartamentoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Consulta centros com filtros opcionais por nome e sigla, retornando o resultado ordenado por nome.
    /// </summary>
    /// <param name="nome">Trecho do nome do centro para busca parcial case-insensitive.</param>
    /// <param name="sigla">Sigla exata do centro para comparação case-insensitive.</param>
    /// <returns>Lista de centros que atendem aos filtros aplicados.</returns>
    public async Task<IEnumerable<CentroDto>> GetAllCentrosAsync(string? nome, string? sigla)
    {
        var query = _context.Centros.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
        {
            query = query.Where(c => c.Nome.ToLower().Contains(nome.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(sigla))
        {
            query = query.Where(c => c.Sigla.ToUpper() == sigla.ToUpper());
        }

        var centros = await query
            .OrderBy(c => c.Nome)
            .Select(c => new CentroDto
            {
                Id = c.Id,
                Nome = c.Nome,
                Sigla = c.Sigla,
                Email = c.Email,
                Telefone = c.Telefone,
            })
            .ToListAsync();

        return centros;
    }

    /// <summary>
    /// Recupera um centro pelo identificador.
    /// </summary>
    /// <param name="id">Identificador do centro.</param>
    /// <returns>DTO do centro encontrado; retorna <see langword="null"/> quando inexistente.</returns>
    /// <exception cref="Exception">Propaga falhas inesperadas durante a consulta no banco de dados.</exception>
    public async Task<CentroDto?> GetCentroByIdAsync(long id)
    {
        _logger.LogInformation("Iniciando busca de uma centro pelo ID...");
        _logger.LogWarning("ID {Id}", id);

        try
        {
            var centro = await _context.Centros.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);

            if (centro == null)
            {
                _logger.LogWarning("Centro com ID {Id} não encontrado.", id);
                return null;
            }

            _logger.LogInformation("Centro com ID {Id} encontrado.", id);

            return new CentroDto
            {
                Id = centro.Id,
                Nome = centro.Nome,
                Sigla = centro.Sigla,
                Email = centro.Email,
                Telefone = centro.Telefone,
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar centro por ID {Id}.", id);
            throw;
        }
    }
}
