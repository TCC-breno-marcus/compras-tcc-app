using Database;
using Microsoft.EntityFrameworkCore;
using Models.Dtos;
using Services.Interfaces;

public class DepartamentoService : IDepartamentoService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DepartamentoService> _logger;

    public DepartamentoService(AppDbContext context, ILogger<DepartamentoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<DepartamentoDto>> GetAllDepartamentosAsync(
        string? nome,
        string? sigla,
        string? siglaCentro
    )
    {
        var query = _context.Departamentos.AsNoTracking().Include(d => d.Centro).AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
        {
            query = query.Where(d => d.Nome.ToLower().Contains(nome.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(sigla))
        {
            query = query.Where(d => d.Sigla.ToUpper() == sigla.ToUpper());
        }

        if (!string.IsNullOrWhiteSpace(siglaCentro))
        {
            query = query.Where(d => d.Centro.Sigla.ToUpper() == siglaCentro.ToUpper());
        }

        var departamentos = await query
            .OrderBy(d => d.Nome)
            .Select(d => new DepartamentoDto
            {
                Id = d.Id,
                Nome = d.Nome,
                Sigla = d.Sigla,
                Email = d.Email,
                Telefone = d.Telefone,
                Centro = new CentroDto
                {
                    Id = d.Centro.Id,
                    Nome = d.Centro.Nome,
                    Sigla = d.Centro.Sigla,
                    Email = d.Centro.Email,
                    Telefone = d.Centro.Telefone,
                },
            })
            .ToListAsync();

        return departamentos;
    }

    public async Task<DepartamentoDto?> GetDepartamentoByIdAsync(long id)
    {
        _logger.LogInformation("Iniciando busca de uma departamento pelo ID...");
        _logger.LogWarning("ID {Id}", id);

        try
        {
            var departamento = await _context
                .Departamentos.AsNoTracking()
                .Include(d => d.Centro)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (departamento == null)
            {
                _logger.LogWarning("Departamento com ID {Id} não encontrado.", id);
                return null;
            }

            _logger.LogInformation("Departamento com ID {Id} encontrado.", id);

            return new DepartamentoDto
            {
                Id = departamento.Id,
                Nome = departamento.Nome,
                Sigla = departamento.Sigla,
                Email = departamento.Email,
                Telefone = departamento.Telefone,
                Centro = new CentroDto
                {
                    Id = departamento.Centro.Id,
                    Nome = departamento.Centro.Nome,
                    Sigla = departamento.Centro.Sigla,
                    Email = departamento.Centro.Email,
                    Telefone = departamento.Centro.Telefone,
                },
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar departamento por ID {Id}.", id);
            throw;
        }
    }
}
