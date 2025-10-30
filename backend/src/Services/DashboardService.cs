using ComprasTccApp.Backend.Domain;
using Database;
using Microsoft.EntityFrameworkCore;
using Models.Dtos;
using Services.Interfaces;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;
    private readonly ILogger<DepartamentoService> _logger;

    public DashboardService(AppDbContext context, ILogger<DepartamentoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DashResultDto> GetDashboardDataAsync(int ano)
    {
        _logger.LogInformation("Iniciando geração de dashs e KPIs para o gestor...");

        try
        {
            var todasAsSolicitacoesDoAno = await _context
                .Solicitacoes.AsNoTracking()
                .Where(s => s.DataCriacao.Year == ano)
                .Include(s => s.Solicitante.Departamento)
                .Include(s => s.ItemSolicitacao)
                .ThenInclude(i => i.Item.Categoria)
                .Include(s => s.Status)
                .ToListAsync();

            var statusAtivos = new[]
            {
                StatusConsts.Pendente,
                StatusConsts.AguardandoAjustes,
                StatusConsts.Aprovada,
            };

            var solicitacoesAtivas = todasAsSolicitacoesDoAno
                .Where(s => statusAtivos.Contains(s.StatusId))
                .ToList();

            var todosOsItensSolicitadosAtivos = solicitacoesAtivas.SelectMany(s =>
                s.ItemSolicitacao
            );

            var kpis = new DashKPIsDto
            {
                TotalSolicitacoes = todasAsSolicitacoesDoAno.Count,
                ValorTotalEstimado = todosOsItensSolicitadosAtivos.Sum(i =>
                    i.Quantidade * i.ValorUnitario
                ),
                TotalItensUnicos = todosOsItensSolicitadosAtivos
                    .Select(i => i.ItemId)
                    .Distinct()
                    .Count(),
                TotalUnidadesSolicitadas = todosOsItensSolicitadosAtivos.Sum(i => i.Quantidade),
                TotalDepartamentosSolicitantes = solicitacoesAtivas
                    .Select(s => s.Solicitante.DepartamentoId)
                    .Distinct()
                    .Count(),
                CustoMedioSolicitacao =
                    (solicitacoesAtivas.Count > 0)
                        ? todosOsItensSolicitadosAtivos.Sum(i => i.Quantidade * i.ValorUnitario)
                            / solicitacoesAtivas.Count
                        : 0,
            };

            var valorPorDepto = solicitacoesAtivas
                .GroupBy(s => s.Solicitante.Departamento.Sigla)
                .Select(g => new
                {
                    Label = g.Key,
                    Valor = g.SelectMany(s => s.ItemSolicitacao)
                        .Sum(i => i.Quantidade * i.ValorUnitario),
                })
                .OrderByDescending(x => x.Valor)
                .ToList();

            var valorPorCategoria = todosOsItensSolicitadosAtivos
                .GroupBy(i => i.Item.Categoria.Nome)
                .Select(g => new
                {
                    Label = g.Key,
                    Valor = g.Sum(i => i.Quantidade * i.ValorUnitario),
                })
                .OrderByDescending(x => x.Valor)
                .ToList();

            var visaoGeralStatus = todasAsSolicitacoesDoAno
                .GroupBy(s => s.Status.Nome)
                .Select(g => new { Label = g.Key, Valor = (decimal)g.Count() })
                .ToList();

            // 6. Calcula os Top 10
            var topItensPorQuantidade = todosOsItensSolicitadosAtivos
                .GroupBy(i => i.Item.Id)
                .Select(g =>
                {
                    var itemExemplo = g.First().Item;
                    return new DashTopItemDto
                    {
                        ItemId = g.Key,
                        Nome = itemExemplo.Nome,
                        CatMat = itemExemplo.CatMat,
                        Valor = g.Sum(i => i.Quantidade),
                    };
                })
                .OrderByDescending(x => x.Valor)
                .Take(10)
                .ToList();

            var topItensPorValorTotal = todosOsItensSolicitadosAtivos
                .GroupBy(i => i.Item.Id)
                .Select(g =>
                {
                    var itemExemplo = g.First().Item;
                    return new DashTopItemDto
                    {
                        ItemId = g.Key,
                        Nome = itemExemplo.Nome,
                        CatMat = itemExemplo.CatMat,
                        Valor = g.Sum(i => i.Quantidade * i.ValorUnitario),
                    };
                })
                .OrderByDescending(x => x.Valor)
                .Take(10)
                .ToList();

            // 7. Monta o DTO de Resposta
            var dashboard = new DashResultDto
            {
                Kpis = kpis,
                ValorPorDepartamento = new ChartDataDto
                {
                    Labels = valorPorDepto.Select(x => x.Label).ToList(),
                    Data = valorPorDepto.Select(x => x.Valor).ToList(),
                },
                ValorPorCategoria = new ChartDataDto
                {
                    Labels = valorPorCategoria.Select(x => x.Label).ToList(),
                    Data = valorPorCategoria.Select(x => x.Valor).ToList(),
                },
                VisaoGeralStatus = new ChartDataDto
                {
                    Labels = visaoGeralStatus.Select(x => x.Label).ToList(),
                    Data = visaoGeralStatus.Select(x => x.Valor).ToList(),
                },
                TopItensPorQuantidade = topItensPorQuantidade,
                TopItensPorValorTotal = topItensPorValorTotal,
            };

            return dashboard;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar dashboards.");
            throw;
        }
    }
}
