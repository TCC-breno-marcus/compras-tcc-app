using ComprasTccApp.Backend.Enums;
using ComprasTccApp.Backend.Extensions;
using Database;
using Microsoft.EntityFrameworkCore;
using Models.Dtos;
using Services.Interfaces;

namespace Services
{
    public class RelatorioService : IRelatorioService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RelatorioService> _logger;

        public RelatorioService(AppDbContext context, ILogger<RelatorioService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PaginatedResultDto<ItemPorDepartamentoDto>> GetItensPorDepartamentoAsync(
            string? searchTerm,
            string? categoriaNome,
            string? departamento,
            string? sortOrder,
            int pageNumber,
            int pageSize
        )
        {
            var query = _context.SolicitacaoItens.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(si =>
                    si.Item.Nome.ToLower().Contains(term)
                    || si.Item.Descricao.ToLower().Contains(term)
                    || si.Item.CatMat.ToLower().Contains(term)
                    || si.Item.Especificacao.ToLower().Contains(term)
                );
            }

            if (!string.IsNullOrWhiteSpace(categoriaNome))
            {
                query = query.Where(si =>
                    si.Item.Categoria.Nome.ToLower().Contains(categoriaNome.ToLower())
                );
            }

            if (!string.IsNullOrEmpty(departamento))
            {
                DepartamentoEnum? departamentoEnum = departamento.FromString<DepartamentoEnum>();
                if (departamentoEnum.HasValue)
                {
                    query = query.Where(s =>
                        s.Solicitacao.Solicitante.Unidade == departamentoEnum.Value
                    );
                }
            }

            var todosOsItensSolicitados = await query
                .Include(si => si.Item)
                .ThenInclude(i => i.Categoria)
                .Include(si => si.Solicitacao)
                .ThenInclude(s => s.Solicitante)
                .ToListAsync();

            var itensSolicitados = todosOsItensSolicitados
                .GroupBy(si => si.Item.Id)
                .Select(group =>
                {
                    var itemExemplo = group.First().Item;

                    return new ItemPorDepartamentoDto
                    {
                        Id = group.Key,
                        Nome = itemExemplo.Nome,
                        CatMat = itemExemplo.CatMat,
                        // Trunca a descrição para no máximo 100 caracteres, adicionando "..." se for maior
                        Descricao =
                            itemExemplo.Descricao.Length > 100
                                ? itemExemplo.Descricao.Substring(0, 100) + "..."
                                : itemExemplo.Descricao,
                        CategoriaNome = itemExemplo.Categoria.Nome,
                        // TODO: o link abaixo deve estar em variável de ambiente
                        LinkImagem = string.IsNullOrWhiteSpace(itemExemplo.LinkImagem)
                            ? itemExemplo.LinkImagem
                            : $"http://localhost:8088/images/{itemExemplo.LinkImagem}",
                        PrecoSugerido = itemExemplo.PrecoSugerido,
                        // Soma a quantidade de todas as ocorrências deste item
                        QuantidadeTotalSolicitada = group.Sum(si => si.Quantidade),
                        // Cria o detalhamento por departamento
                        QuantidadesPorDepartamento = group
                            .GroupBy(si => si.Solicitacao.Solicitante.Unidade) // Sub-agrupamento por departamento
                            .Select(deptGroup => new QuantidadePorDepartamentoDto
                            {
                                Departamento = deptGroup.Key.ToFriendlyString(),
                                QuantidadeTotal = deptGroup.Sum(si => si.Quantidade),
                            })
                            .OrderBy(d => d.Departamento)
                            .ToList(),
                        ValorTotalSolicitado = group.Sum(si => si.Quantidade * si.ValorUnitario),
                        PrecoMedio = group.Average(si => si.ValorUnitario),
                        PrecoMinimo = group.Min(si => si.ValorUnitario),
                        PrecoMaximo = group.Max(si => si.ValorUnitario),
                        NumeroDeSolicitacoes = group
                            .Select(si => si.SolicitacaoId)
                            .Distinct()
                            .Count(), // Nº de Solicitações Diferentes
                    };
                })
                .AsQueryable();

            if (sortOrder?.ToLower() == "desc")
            {
                itensSolicitados = itensSolicitados.OrderByDescending(i => i.Nome);
            }
            else
            {
                itensSolicitados = itensSolicitados.OrderBy(i => i.Nome);
            }

            var totalCount = itensSolicitados.Count();

            var itensPaginados = itensSolicitados
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedResultDto<ItemPorDepartamentoDto>(
                itensPaginados,
                totalCount,
                pageNumber,
                pageSize
            );
        }
    }
}
