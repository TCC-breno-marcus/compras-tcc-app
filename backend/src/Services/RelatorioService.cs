using System.Globalization;
using System.IO;
using System.IO;
using System.Text;
using ComprasTccApp.Backend.Enums;
using ComprasTccApp.Backend.Extensions;
using ComprasTccApp.Models.Entities.Itens;
using CsvHelper;
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
            var itens = await GetItensPorDepartamentoInternalAsync(
                null,
                searchTerm,
                categoriaNome,
                departamento,
                sortOrder
            );

            var totalCount = itens.Count();
            var itensPaginados = itens.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new PaginatedResultDto<ItemPorDepartamentoDto>(
                itensPaginados,
                totalCount,
                pageNumber,
                pageSize
            );
        }

        public async Task<List<ItemPorDepartamentoDto>> GetAllItensPorDepartamentoAsync(
            bool isGeral,
            string? searchTerm = null,
            string? categoriaNome = null,
            string? departamento = null,
            string? sortOrder = null
        )
        {
            var itens = await GetItensPorDepartamentoInternalAsync(
                isGeral,
                searchTerm,
                categoriaNome,
                departamento,
                sortOrder
            );

            return itens.ToList();
        }

        private async Task<IQueryable<ItemPorDepartamentoDto>> GetItensPorDepartamentoInternalAsync(
            bool? isGeral,
            string? searchTerm,
            string? categoriaNome,
            string? departamento,
            string? sortOrder
        )
        {
            var query = BuildBaseQuery(isGeral, searchTerm, categoriaNome, departamento);

            var todosOsItensSolicitados = await query
                .Include(si => si.Item)
                .ThenInclude(i => i.Categoria)
                .Include(si => si.Solicitacao)
                .ThenInclude(s => s.Solicitante)
                .ToListAsync();

            var itensSolicitados = todosOsItensSolicitados
                .GroupBy(si => si.Item.Id)
                .Select(TransformToDto)
                .AsQueryable();

            return ApplySorting(itensSolicitados, sortOrder);
        }

        private IQueryable<SolicitacaoItem> BuildBaseQuery(
            bool? isGeral,
            string? searchTerm,
            string? categoriaNome,
            string? departamento
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

            if (isGeral.HasValue)
            {
                if (isGeral.Value)
                {
                    query = query.Where(si => si.Solicitacao is SolicitacaoGeral);
                }
                else
                {
                    query = query.Where(si => si.Solicitacao is SolicitacaoPatrimonial);
                }
            }

            return query;
        }

        private static ItemPorDepartamentoDto TransformToDto(IGrouping<long, SolicitacaoItem> group)
        {
            var itemExemplo = group.First().Item;

            return new ItemPorDepartamentoDto
            {
                Id = group.Key,
                Nome = itemExemplo.Nome,
                CatMat = itemExemplo.CatMat,
                Descricao = itemExemplo.Descricao,
                Especificacao = itemExemplo.Especificacao,
                CategoriaNome = itemExemplo.Categoria.Nome,
                // TODO: o link abaixo deve estar em variável de ambiente
                LinkImagem = string.IsNullOrWhiteSpace(itemExemplo.LinkImagem)
                    ? itemExemplo.LinkImagem
                    : $"http://localhost:8088/images/{itemExemplo.LinkImagem}",
                PrecoSugerido = itemExemplo.PrecoSugerido,
                QuantidadeTotalSolicitada = group.Sum(si => si.Quantidade),
                DemandaPorDepartamento = group
                    .GroupBy(si => si.Solicitacao.Solicitante.Unidade)
                    .Select(deptGroup => new DemandaPorDepartamentoDto
                    {
                        Departamento = deptGroup.Key.ToFriendlyString(),
                        QuantidadeTotal = deptGroup.Sum(si => si.Quantidade),
                        Justificativa =
                            deptGroup.First().Solicitacao is SolicitacaoGeral
                                ? string.Empty
                                : string.Join(
                                    "; ",
                                    deptGroup
                                        .Where(si => !string.IsNullOrWhiteSpace(si.Justificativa))
                                        .Select(si => si.Justificativa)
                                        .Distinct()
                                ),
                    })
                    .OrderBy(d => d.Departamento)
                    .ToList(),
                ValorTotalSolicitado = group.Sum(si => si.Quantidade * si.ValorUnitario),
                PrecoMedio = group.Average(si => si.ValorUnitario),
                PrecoMinimo = group.Min(si => si.ValorUnitario),
                PrecoMaximo = group.Max(si => si.ValorUnitario),
                NumeroDeSolicitacoes = group.Select(si => si.SolicitacaoId).Distinct().Count(),
            };
        }

        private static IQueryable<ItemPorDepartamentoDto> ApplySorting(
            IQueryable<ItemPorDepartamentoDto> query,
            string? sortOrder
        )
        {
            return sortOrder?.ToLower() == "desc"
                ? query.OrderByDescending(i => i.Nome)
                : query.OrderBy(i => i.Nome);
        }

        public async Task<byte[]> GetAllItensPorDepartamentoCsvAsync(
            bool isGeral,
            string? searchTerm = null,
            string? categoriaNome = null,
            string? departamento = null
        )
        {
            var itens = await GetAllItensPorDepartamentoAsync(
                isGeral,
                searchTerm,
                categoriaNome,
                departamento
            );

            return GerarCsv(itens);
        }

        private static byte[] GerarCsv(List<ItemPorDepartamentoDto> itens)
        {
            var recordsParaCsv = itens.SelectMany(item =>
                item.DemandaPorDepartamento.Select(depto => new
                {
                    CATMAT = item.CatMat,
                    Item = item.Nome,
                    Descricao = item.Descricao,
                    Especificacao = item.Especificacao,
                    Categoria = item.CategoriaNome,
                    Departamento = depto.Departamento,
                    QuantidadeSolicitada = depto.QuantidadeTotal,
                    Justificativa = depto.Justificativa,
                    PrecoMedioItem = item.PrecoMedio,
                    NumeroTotalDeSolicitacoes = item.NumeroDeSolicitacoes,
                })
            );

            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(true)))
                {
                    using (var csv = new CsvWriter(writer, new CultureInfo("pt-BR")))
                    {
                        csv.WriteRecords(recordsParaCsv);
                    }
                }

                return memoryStream.ToArray();
            }
        }
    }
}
