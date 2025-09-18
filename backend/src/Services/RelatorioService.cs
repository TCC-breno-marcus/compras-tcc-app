using System.Globalization;
using System.IO;
using System.Text;
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
        private readonly string _imageBaseUrl;

        public RelatorioService(
            AppDbContext context,
            ILogger<RelatorioService> logger,
            IConfiguration configuration
        )
        {
            _context = context;
            _logger = logger;
            _imageBaseUrl = configuration["ImageBaseUrl"] ?? "";
        }

        public async Task<PaginatedResultDto<ItemPorDepartamentoDto>> GetItensPorDepartamentoAsync(
            string? searchTerm,
            string? categoriaNome,
            string? itemsType,
            string? siglaDepartamento,
            string? sortOrder,
            int pageNumber,
            int pageSize
        )
        {
            var itens = await GetItensPorDepartamentoInternalAsync(
                searchTerm,
                categoriaNome,
                itemsType,
                siglaDepartamento,
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
            string? searchTerm = null,
            string? categoriaNome = null,
            string? itemsType = null,
            string? siglaDepartamento = null,
            string? sortOrder = null
        )
        {
            var itens = await GetItensPorDepartamentoInternalAsync(
                searchTerm,
                categoriaNome,
                itemsType,
                siglaDepartamento,
                sortOrder
            );

            return itens.ToList();
        }

        private async Task<IQueryable<ItemPorDepartamentoDto>> GetItensPorDepartamentoInternalAsync(
            string? searchTerm,
            string? categoriaNome,
            string? itemsType,
            string? siglaDepartamento,
            string? sortOrder
        )
        {
            var query = BuildBaseQuery(searchTerm, categoriaNome, itemsType, siglaDepartamento);

            var todosOsItensSolicitados = await query
                .Include(si => si.Item)
                .ThenInclude(i => i.Categoria)
                .Include(si => si.Solicitacao)
                .ThenInclude(s => s.Solicitante)
                .ThenInclude(sol => sol.Departamento)
                .ToListAsync();

            var itensSolicitados = todosOsItensSolicitados
                .GroupBy(si => si.Item.Id)
                .Select(TransformToDto)
                .AsQueryable();

            return ApplySorting(itensSolicitados, sortOrder);
        }

        private IQueryable<SolicitacaoItem> BuildBaseQuery(
            string? searchTerm,
            string? categoriaNome,
            string? itemsType,
            string? siglaDepartamento
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

            if (!string.IsNullOrWhiteSpace(itemsType))
            {
                var categoriasPatrimoniais = new[] { "Mobiliário", "Eletrodomésticos" };
                if (itemsType.Equals("patrimonial", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(si =>
                        categoriasPatrimoniais.Contains(si.Item.Categoria.Nome)
                    );
                }
                else if (itemsType.Equals("geral", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.Where(si =>
                        !categoriasPatrimoniais.Contains(si.Item.Categoria.Nome)
                    );
                }
            }
            if (!string.IsNullOrEmpty(siglaDepartamento))
            {
                query = query.Where(s =>
                    s.Solicitacao.Solicitante.Departamento.Sigla.ToUpper()
                    == siglaDepartamento.ToUpper()
                );
            }

            return query;
        }

        private ItemPorDepartamentoDto TransformToDto(IGrouping<long, SolicitacaoItem> group)
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
                LinkImagem = string.IsNullOrWhiteSpace(itemExemplo.LinkImagem)
                    ? itemExemplo.LinkImagem
                    : $"{_imageBaseUrl}{itemExemplo.LinkImagem}",
                PrecoSugerido = itemExemplo.PrecoSugerido,
                QuantidadeTotalSolicitada = group.Sum(si => si.Quantidade),
                DemandaPorDepartamento = group
                    .GroupBy(si => si.Solicitacao.Solicitante.Departamento)
                    .Select(deptGroup => new DemandaPorDepartamentoDto
                    {
                        Unidade = new UnidadeOrganizacionalDto
                        {
                            Id = deptGroup.Key.Id,
                            Nome = deptGroup.Key.Nome,
                            Sigla = deptGroup.Key.Sigla,
                            Email = deptGroup.Key.Email,
                            Telefone = deptGroup.Key.Telefone,
                            Tipo = "Departamento",
                        },
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
                    .OrderBy(d => d.Unidade.Nome)
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
            string? searchTerm = null,
            string? categoriaNome = null,
            string? itemsType = null,
            string? siglaDepartamento = null
        )
        {
            var itens = await GetAllItensPorDepartamentoAsync(
                searchTerm,
                categoriaNome,
                itemsType,
                siglaDepartamento
            );

            return GerarCsv(itens, itemsType!);
        }

        private static byte[] GerarCsv(List<ItemPorDepartamentoDto> itens, string itemsType)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(true)))
                {
                    using (var csv = new CsvWriter(writer, new CultureInfo("pt-BR")))
                    {
                        // --- PASSO 1: DESCOBRIR TODAS AS COLUNAS DE DEPARTAMENTO ---
                        var todosOsDepartamentos = itens
                            .SelectMany(i => i.DemandaPorDepartamento.Select(d => d.Unidade.Sigla))
                            .Distinct()
                            .OrderBy(sigla => sigla)
                            .ToList();

                        // --- PASSO 2: ESCREVER A LINHA DE CABEÇALHO ---

                        // Escreve as colunas fixas
                        csv.WriteField("CATMAT");
                        csv.WriteField("Item");
                        csv.WriteField("Descrição");
                        csv.WriteField("Especificação");
                        csv.WriteField("Categoria");
                        csv.WriteField("Qtde. Total Solicitada");

                        // Escreve as colunas dinâmicas para cada departamento
                        foreach (var siglaDepto in todosOsDepartamentos)
                        {
                            csv.WriteField($"Qtde {siglaDepto}");
                        }

                        // Adiciona a coluna de Justificativas apenas se for patrimonial
                        bool isPatrimonial = itemsType.Equals(
                            "patrimonial",
                            StringComparison.OrdinalIgnoreCase
                        );
                        if (isPatrimonial)
                        {
                            csv.WriteField("Justificativas");
                        }

                        csv.NextRecord(); // Finaliza a linha de cabeçalho

                        // --- PASSO 3: ESCREVER AS LINHAS DE DADOS ---
                        foreach (var item in itens)
                        {
                            // Escreve os dados das colunas fixas
                            csv.WriteField(item.CatMat);
                            csv.WriteField(item.Nome);
                            csv.WriteField(item.Descricao);
                            csv.WriteField(item.Especificacao);
                            csv.WriteField(item.CategoriaNome);
                            csv.WriteField(item.QuantidadeTotalSolicitada);

                            // Cria um dicionário para busca rápida da quantidade por departamento
                            var demandaDict = item.DemandaPorDepartamento.ToDictionary(
                                d => d.Unidade.Sigla,
                                d => d.QuantidadeTotal
                            );

                            // Escreve os dados nas colunas dinâmicas
                            foreach (var siglaDepto in todosOsDepartamentos)
                            {
                                // Tenta encontrar a quantidade para o depto/coluna atual
                                if (demandaDict.TryGetValue(siglaDepto, out var quantidade))
                                {
                                    csv.WriteField(quantidade);
                                }
                                else
                                {
                                    csv.WriteField(0); // Coloca 0 se o item não foi solicitado por este depto
                                }
                            }

                            if (isPatrimonial)
                            {
                                var justificativas = string.Join(
                                    "; ",
                                    item.DemandaPorDepartamento.Where(d =>
                                            !string.IsNullOrWhiteSpace(d.Justificativa)
                                        )
                                        .Select(d => $"{d.Unidade.Sigla}: {d.Justificativa}") // Adiciona a sigla para contexto
                                        .Distinct()
                                );

                                csv.WriteField(justificativas);
                            }

                            csv.NextRecord(); // Finaliza a linha do item
                        }
                    }
                }
                return memoryStream.ToArray();
            }
        }
    }
}
