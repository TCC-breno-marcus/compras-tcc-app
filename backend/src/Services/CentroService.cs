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

    public async Task<List<RelatorioGastosCentroSaidaDto>> GetRelatorioGastosPorCentroAsync(RelatorioGastosCentroFiltroDto filtro)
    {
        var dataFimAjustada = filtro.DataFim.Date.AddDays(1).AddTicks(-1);

        var query = _context.SolicitacaoItens.AsNoTracking();

        query = query.Where(si =>
            si.Solicitacao.DataCriacao >= filtro.DataInicio &&
            si.Solicitacao.DataCriacao <= dataFimAjustada
        );

        if (filtro.StatusId.HasValue)
            query = query.Where(si => si.Solicitacao.StatusId == filtro.StatusId.Value);
        

        var dadosBrutos = await query
            .Select(si => new
            {
                CentroId = si.Solicitacao.Solicitante.Departamento.Centro.Id,
                CentroNome = si.Solicitacao.Solicitante.Departamento.Centro.Nome,
                CentroSigla = si.Solicitacao.Solicitante.Departamento.Centro.Sigla,
                DeptNome = si.Solicitacao.Solicitante.Departamento.Nome,
                SolicitacaoId = si.SolicitacaoId,
                TotalItem = si.Quantidade * si.ValorUnitario
            })
            .ToListAsync();

        var relatorio = dadosBrutos
            .GroupBy(x => new { x.CentroId, x.CentroNome, x.CentroSigla })
            .Select(g => new RelatorioGastosCentroSaidaDto
            {
                CentroId = g.Key.CentroId,
                CentroNome = g.Key.CentroNome,
                CentroSigla = g.Key.CentroSigla,
                QuantidadeSolicitacoes = g.Select(x => x.SolicitacaoId).Distinct().Count(),
                ValorTotalGasto = g.Sum(x => x.TotalItem),
                DepartamentoMaiorGasto = g.GroupBy(x => x.DeptNome)
                                          .Select(deptGroup => new
                                          {
                                              Nome = deptGroup.Key,
                                              Total = deptGroup.Sum(y => y.TotalItem)
                                          })
                                          .OrderByDescending(d => d.Total)
                                          .First().Nome
            })
            .OrderByDescending(r => r.ValorTotalGasto)
            .ToList();

        return relatorio;
    }
}
