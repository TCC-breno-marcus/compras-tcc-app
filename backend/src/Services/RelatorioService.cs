using System.Globalization;
using System.IO;
using System.Text;
using ClosedXML.Excel;
using ComprasTccApp.Backend.Domain;
using ComprasTccApp.Models.Entities.Itens;
using CsvHelper;
using Database;
using Microsoft.EntityFrameworkCore;
using Models.Dtos;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Services.Interfaces;

namespace Services
{
    public class RelatorioService : IRelatorioService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RelatorioService> _logger;
        private readonly string _IMAGE_BASE_URL;

        private static string NormalizeDepartmentSigla(string? sigla) =>
            (sigla ?? string.Empty).Trim().ToUpperInvariant();

        public RelatorioService(
            AppDbContext context,
            ILogger<RelatorioService> logger,
            IConfiguration configuration
        )
        {
            _context = context;
            _logger = logger;
            _IMAGE_BASE_URL = configuration["IMAGE_BASE_URL"] ?? "";
        }

        /// <summary>
        /// Retorna relatório paginado de itens agrupados por produto com consolidação de demanda por departamento.
        /// </summary>
        /// <param name="searchTerm">Termo livre para busca em nome, descrição, CATMAT e especificação.</param>
        /// <param name="categoriaNome">Filtro parcial por nome da categoria.</param>
        /// <param name="itemsType">Tipo de item: patrimonial ou geral.</param>
        /// <param name="siglaDepartamento">Sigla exata do departamento solicitante.</param>
        /// <param name="somenteSolicitacoesAtivas">Indica se considera apenas solicitações em status ativos.</param>
        /// <param name="sortOrder">Ordenação por nome do item: asc ou desc.</param>
        /// <param name="pageNumber">Número da página (base 1).</param>
        /// <param name="pageSize">Quantidade de registros por página.</param>
        /// <returns>Resultado paginado com itens agregados e métricas de demanda.</returns>
        /// <exception cref="Exception">Propaga falhas inesperadas durante consulta e agregação dos dados.</exception>
        public async Task<PaginatedResultDto<ItemPorDepartamentoDto>> GetItensPorDepartamentoAsync(
            string? searchTerm,
            string? categoriaNome,
            string? itemsType,
            string? siglaDepartamento,
            bool? somenteSolicitacoesAtivas,
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
                somenteSolicitacoesAtivas,
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

        /// <summary>
        /// Retorna relatório completo de itens por departamento sem paginação, aplicando os mesmos filtros de negócio.
        /// </summary>
        /// <param name="searchTerm">Termo livre para busca em nome, descrição, CATMAT e especificação.</param>
        /// <param name="categoriaNome">Filtro parcial por nome da categoria.</param>
        /// <param name="itemsType">Tipo de item: patrimonial ou geral.</param>
        /// <param name="siglaDepartamento">Sigla exata do departamento solicitante.</param>
        /// <param name="somenteSolicitacoesAtivas">Indica se considera apenas solicitações em status ativos.</param>
        /// <param name="sortOrder">Ordenação por nome do item: asc ou desc.</param>
        /// <returns>Lista completa de itens agregados por produto.</returns>
        /// <exception cref="Exception">Propaga falhas inesperadas durante consulta e agregação dos dados.</exception>
        public async Task<List<ItemPorDepartamentoDto>> GetAllItensPorDepartamentoAsync(
            string? searchTerm = null,
            string? categoriaNome = null,
            string? itemsType = null,
            string? siglaDepartamento = null,
            bool? somenteSolicitacoesAtivas = false,
            string? sortOrder = null
        )
        {
            var itens = await GetItensPorDepartamentoInternalAsync(
                searchTerm,
                categoriaNome,
                itemsType,
                siglaDepartamento,
                somenteSolicitacoesAtivas,
                sortOrder
            );

            return itens.ToList();
        }

        private async Task<IQueryable<ItemPorDepartamentoDto>> GetItensPorDepartamentoInternalAsync(
            string? searchTerm,
            string? categoriaNome,
            string? itemsType,
            string? siglaDepartamento,
            bool? somenteSolicitacoesAtivas,
            string? sortOrder
        )
        {
            var query = BuildBaseQuery(
                searchTerm,
                categoriaNome,
                itemsType,
                siglaDepartamento,
                somenteSolicitacoesAtivas
            );

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
            string? siglaDepartamento,
            bool? somenteSolicitacoesAtivas
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

            if (somenteSolicitacoesAtivas.HasValue && somenteSolicitacoesAtivas.Value)
            {
                var statusAtivos = new[]
                {
                    StatusConsts.Pendente,
                    StatusConsts.AguardandoAjustes,
                    StatusConsts.Aprovada,
                };
                query = query.Where(s => statusAtivos.Contains(s.Solicitacao.StatusId));
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
                    : $"{_IMAGE_BASE_URL}{itemExemplo.LinkImagem}",
                PrecoSugerido = itemExemplo.PrecoSugerido,
                QuantidadeTotalSolicitada = group.Sum(si => si.Quantidade),
                DemandaPorDepartamento = group
                    // Agrupa por chave estável para evitar duplicação com AsNoTracking.
                    .GroupBy(si => si.Solicitacao.Solicitante.Departamento.Id)
                    .Select(deptGroup => new DemandaPorDepartamentoDto
                    {
                        Unidade = new UnidadeOrganizacionalDto
                        {
                            Id = deptGroup.Key,
                            Nome = deptGroup.First().Solicitacao.Solicitante.Departamento.Nome,
                            Sigla = NormalizeDepartmentSigla(
                                deptGroup.First().Solicitacao.Solicitante.Departamento.Sigla
                            ),
                            Email = deptGroup.First().Solicitacao.Solicitante.Departamento.Email,
                            Telefone = deptGroup
                                .First()
                                .Solicitacao.Solicitante.Departamento.Telefone,
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

        /// <summary>
        /// Exporta o relatório de itens por departamento em CSV, Excel ou PDF conforme o formato solicitado.
        /// </summary>
        /// <param name="itemsType">Tipo de item: patrimonial ou geral.</param>
        /// <param name="formatoArquivo">Formato de saída: csv ou xlsx.</param>
        /// <param name="searchTerm">Termo livre para busca em nome, descrição, CATMAT e especificação.</param>
        /// <param name="categoriaNome">Filtro parcial por nome da categoria.</param>
        /// <param name="siglaDepartamento">Sigla exata do departamento solicitante.</param>
        /// <param name="somenteSolicitacoesAtivas">Indica se considera apenas solicitações em status ativos.</param>
        /// <param name="usuarioSolicitante">Nome do usuário que solicitou a exportação.</param>
        /// <param name="dataHoraSolicitacao">Data/hora da solicitação da exportação.</param>
        /// <returns>Conteúdo binário do arquivo exportado.</returns>
        /// <exception cref="Exception">Propaga falhas inesperadas durante geração do arquivo.</exception>
        public async Task<byte[]> ExportarItensPorDepartamentoAsync(
            string itemsType,
            string formatoArquivo,
            string? searchTerm = null,
            string? categoriaNome = null,
            string? siglaDepartamento = null,
            bool? somenteSolicitacoesAtivas = false,
            string? usuarioSolicitante = null,
            DateTimeOffset? dataHoraSolicitacao = null
        )
        {
            var itens = await GetAllItensPorDepartamentoAsync(
                searchTerm,
                categoriaNome,
                itemsType,
                siglaDepartamento,
                somenteSolicitacoesAtivas
            );

            if (formatoArquivo.Equals("csv", StringComparison.OrdinalIgnoreCase))
            {
                return GerarCsv(itens, itemsType);
            }
            else if (formatoArquivo.Equals("excel", StringComparison.OrdinalIgnoreCase))
            {
                bool isPatrimonial = itemsType == "patrimonial";
                return GerarExcel(itens, isPatrimonial);
            }

            return GerarPdf(itens, itemsType, usuarioSolicitante, dataHoraSolicitacao);
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
                            .SelectMany(i =>
                                i.DemandaPorDepartamento.Select(d =>
                                    NormalizeDepartmentSigla(d.Unidade.Sigla)
                                )
                            )
                            .Where(sigla => !string.IsNullOrWhiteSpace(sigla))
                            .Distinct()
                            .OrderBy(sigla => sigla)
                            .ToList();

                        // --- PASSO 2: ESCREVER A LINHA DE CABEÇALHO ---

                        // Escreve as colunas fixas
                        csv.WriteField("CATMAT");
                        csv.WriteField("Nome");
                        csv.WriteField("Descrição");
                        csv.WriteField("Especificação");
                        csv.WriteField("Categoria");
                        csv.WriteField("Preço Mínimo");
                        csv.WriteField("Preço Máximo");
                        csv.WriteField("Preço Médio");
                        csv.WriteField("Valor Total");
                        csv.WriteField("Qtde. Total");

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
                            csv.WriteField("Justificativa(s)");
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
                            csv.WriteField(item.PrecoMinimo);
                            csv.WriteField(item.PrecoMaximo);
                            csv.WriteField(item.PrecoMedio);
                            csv.WriteField(item.ValorTotalSolicitado);
                            csv.WriteField(item.QuantidadeTotalSolicitada);

                            // Cria um dicionário para busca rápida da quantidade por departamento
                            var demandaDict = item
                                .DemandaPorDepartamento.GroupBy(d =>
                                    NormalizeDepartmentSigla(d.Unidade.Sigla)
                                )
                                .ToDictionary(g => g.Key, g => g.Sum(d => d.QuantidadeTotal));

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
                                        .Select(d =>
                                            $"{NormalizeDepartmentSigla(d.Unidade.Sigla)}: {d.Justificativa}"
                                        ) // Adiciona a sigla para contexto
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

        private static byte[] GerarPdf(
            List<ItemPorDepartamentoDto> itens,
            string itemsType,
            string? usuarioSolicitante,
            DateTimeOffset? dataHoraSolicitacao
        )
        {
            PdfFontResolver.EnsureInitialized();
            var isPatrimonial = itemsType.Equals("patrimonial", StringComparison.OrdinalIgnoreCase);
            var cultura = new CultureInfo("pt-BR");
            var usuario = string.IsNullOrWhiteSpace(usuarioSolicitante)
                ? "Usuario nao identificado"
                : usuarioSolicitante;
            var dataGeracao = dataHoraSolicitacao ?? DateTimeOffset.Now;

            using var stream = new MemoryStream();
            using var document = new PdfDocument();

            PdfPage page = document.AddPage();
            page.Size = PdfSharpCore.PageSize.A4;
            page.Orientation = PdfSharpCore.PageOrientation.Landscape;

            XGraphics gfx = XGraphics.FromPdfPage(page);
            var fonteTitulo = new XFont("AppSans", 14, XFontStyle.Bold);
            var fonteTexto = new XFont("AppSans", 8, XFontStyle.Regular);
            var fonteCabecalhoTabela = new XFont("AppSans", 8, XFontStyle.Bold);

            const double margemEsquerda = 30;
            const double margemDireita = 30;
            const double margemTopo = 25;
            const double margemRodape = 25;
            const double paddingCelula = 3;
            double larguraUtil = page.Width - margemEsquerda - margemDireita;
            double y = margemTopo + 50;
            int numeroPagina = 1;

            var cabecalhos = new List<string>
            {
                "CATMAT",
                "Nome",
                "Descricao",
                "Especificacao",
                "Categoria",
                "Preco Min.",
                "Preco Max.",
                "Preco Medio",
                "Valor Total",
                "Qtde. Total",
                "Departamento",
                "Qtde. Solicitada",
            };
            if (isPatrimonial)
            {
                cabecalhos.Add("Justificativa(s)");
            }

            var pesosColunas = new List<double>
            {
                1.0,
                1.6,
                1.9,
                1.9,
                1.2,
                1.0,
                1.0,
                1.0,
                1.2,
                0.9,
                1.1,
                1.0,
            };
            if (isPatrimonial)
            {
                pesosColunas.Add(2.2);
            }

            var somaPesos = pesosColunas.Sum();
            var largurasColunas = pesosColunas.Select(p => larguraUtil * (p / somaPesos)).ToList();

            List<string> QuebrarTexto(string? texto, double larguraColuna, XFont fonte, XGraphics grafico)
            {
                if (string.IsNullOrWhiteSpace(texto))
                {
                    return [string.Empty];
                }

                var limite = Math.Max(larguraColuna - 2 * paddingCelula, 10);
                var linhas = new List<string>();
                var paragrafos = texto.Replace("\r", string.Empty).Split('\n');

                foreach (var paragrafo in paragrafos)
                {
                    var palavras = paragrafo.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (palavras.Length == 0)
                    {
                        linhas.Add(string.Empty);
                        continue;
                    }

                    var linhaAtual = string.Empty;
                    foreach (var palavra in palavras)
                    {
                        var tentativa = string.IsNullOrEmpty(linhaAtual)
                            ? palavra
                            : $"{linhaAtual} {palavra}";
                        if (grafico.MeasureString(tentativa, fonte).Width <= limite)
                        {
                            linhaAtual = tentativa;
                            continue;
                        }

                        if (!string.IsNullOrEmpty(linhaAtual))
                        {
                            linhas.Add(linhaAtual);
                        }

                        if (grafico.MeasureString(palavra, fonte).Width <= limite)
                        {
                            linhaAtual = palavra;
                            continue;
                        }

                        var trecho = string.Empty;
                        foreach (var caractere in palavra)
                        {
                            var tentativaTrecho = trecho + caractere;
                            if (
                                grafico.MeasureString(tentativaTrecho, fonte).Width <= limite
                                || string.IsNullOrEmpty(trecho)
                            )
                            {
                                trecho = tentativaTrecho;
                            }
                            else
                            {
                                linhas.Add(trecho);
                                trecho = caractere.ToString();
                            }
                        }

                        linhaAtual = trecho;
                    }

                    if (!string.IsNullOrEmpty(linhaAtual))
                    {
                        linhas.Add(linhaAtual);
                    }
                }

                return linhas.Count == 0 ? [string.Empty] : linhas;
            }

            void DesenharCabecalho(PdfPage paginaAtual, XGraphics graficoAtual)
            {
                graficoAtual.DrawString(
                    "Relatorio de Itens por Departamento",
                    fonteTitulo,
                    XBrushes.Black,
                    new XRect(margemEsquerda, margemTopo, larguraUtil, 18),
                    XStringFormats.TopLeft
                );
                graficoAtual.DrawString(
                    $"Tipo: {itemsType} | Usuario: {usuario} | Gerado em: {dataGeracao:dd/MM/yyyy HH:mm:ss}",
                    fonteTexto,
                    XBrushes.Black,
                    new XRect(margemEsquerda, margemTopo + 20, larguraUtil, 16),
                    XStringFormats.TopLeft
                );
                graficoAtual.DrawLine(
                    XPens.Gray,
                    margemEsquerda,
                    margemTopo + 40,
                    paginaAtual.Width - margemDireita,
                    margemTopo + 40
                );
            }

            void DesenharRodape(PdfPage paginaAtual, XGraphics graficoAtual, int pagina)
            {
                graficoAtual.DrawLine(
                    XPens.LightGray,
                    margemEsquerda,
                    paginaAtual.Height - margemRodape - 8,
                    paginaAtual.Width - margemDireita,
                    paginaAtual.Height - margemRodape - 8
                );
                graficoAtual.DrawString(
                    $"Pagina {pagina}",
                    fonteTexto,
                    XBrushes.Gray,
                    new XRect(margemEsquerda, paginaAtual.Height - margemRodape, larguraUtil, 12),
                    XStringFormats.TopRight
                );
            }

            double CalcularAlturaCabecalho(XGraphics graficoAtual)
            {
                var alturaLinhaCabecalho = graficoAtual.MeasureString("Ag", fonteCabecalhoTabela).Height;
                var maxLinhasCabecalho = 1;

                for (int i = 0; i < cabecalhos.Count; i++)
                {
                    var linhasCabecalho = QuebrarTexto(
                        cabecalhos[i],
                        largurasColunas[i],
                        fonteCabecalhoTabela,
                        graficoAtual
                    );
                    if (linhasCabecalho.Count > maxLinhasCabecalho)
                    {
                        maxLinhasCabecalho = linhasCabecalho.Count;
                    }
                }

                return Math.Max(20, maxLinhasCabecalho * alturaLinhaCabecalho + 2 * paddingCelula);
            }

            var alturaCabecalhoTabela = CalcularAlturaCabecalho(gfx);

            void DesenharCabecalhoTabela(
                PdfPage paginaAtual,
                XGraphics graficoAtual,
                double yCabecalho,
                double alturaCabecalhoAtual
            )
            {
                double x = margemEsquerda;
                var alturaLinhaCabecalho = graficoAtual.MeasureString("Ag", fonteCabecalhoTabela).Height;
                for (int i = 0; i < cabecalhos.Count; i++)
                {
                    var larguraColuna = largurasColunas[i];
                    graficoAtual.DrawRectangle(
                        XBrushes.LightGray,
                        x,
                        yCabecalho,
                        larguraColuna,
                        alturaCabecalhoAtual
                    );
                    graficoAtual.DrawRectangle(
                        XPens.Gray,
                        x,
                        yCabecalho,
                        larguraColuna,
                        alturaCabecalhoAtual
                    );
                    var linhasCabecalho = QuebrarTexto(
                        cabecalhos[i],
                        larguraColuna,
                        fonteCabecalhoTabela,
                        graficoAtual
                    );
                    for (int j = 0; j < linhasCabecalho.Count; j++)
                    {
                        graficoAtual.DrawString(
                            linhasCabecalho[j],
                            fonteCabecalhoTabela,
                            XBrushes.Black,
                            new XPoint(
                                x + paddingCelula,
                                yCabecalho + paddingCelula + alturaLinhaCabecalho * (j + 1) - 1
                            )
                        );
                    }
                    x += larguraColuna;
                }
            }

            DesenharCabecalho(page, gfx);
            DesenharCabecalhoTabela(page, gfx, y, alturaCabecalhoTabela);
            y += alturaCabecalhoTabela;

            var alturaLinhaTexto = gfx.MeasureString("Ag", fonteTexto).Height;

            foreach (var item in itens)
            {
                var demandasOrdenadas = item
                    .DemandaPorDepartamento.GroupBy(d => NormalizeDepartmentSigla(d.Unidade.Sigla))
                    .Select(g => new DemandaPorDepartamentoDto
                    {
                        Unidade = new UnidadeOrganizacionalDto
                        {
                            Id = g.First().Unidade.Id,
                            Nome = g.First().Unidade.Nome,
                            Sigla = g.Key,
                            Email = g.First().Unidade.Email,
                            Telefone = g.First().Unidade.Telefone,
                            Tipo = g.First().Unidade.Tipo,
                        },
                        QuantidadeTotal = g.Sum(x => x.QuantidadeTotal),
                        Justificativa = string.Join(
                            "; ",
                            g.Select(x => x.Justificativa)
                                .Where(j => !string.IsNullOrWhiteSpace(j))
                                .Distinct()
                        ),
                    })
                    .OrderBy(d => d.Unidade.Nome)
                    .ToList();

                if (demandasOrdenadas.Count == 0)
                {
                    demandasOrdenadas.Add(
                        new DemandaPorDepartamentoDto
                        {
                            Unidade = new UnidadeOrganizacionalDto
                            {
                                Sigla = "-",
                                Nome = "-",
                                Tipo = "Departamento",
                            },
                            QuantidadeTotal = 0,
                            Justificativa = string.Empty,
                        }
                    );
                }

                var valoresItem = new List<string>
                {
                    item.CatMat ?? string.Empty,
                    item.Nome ?? string.Empty,
                    item.Descricao ?? string.Empty,
                    item.Especificacao ?? string.Empty,
                    item.CategoriaNome ?? string.Empty,
                    item.PrecoMinimo.ToString("C", cultura),
                    item.PrecoMaximo.ToString("C", cultura),
                    item.PrecoMedio.ToString("C", cultura),
                    item.ValorTotalSolicitado.ToString("C", cultura),
                    item.QuantidadeTotalSolicitada.ToString(cultura),
                };

                var deptColStart = 10;
                var deptRowHeight = new List<double>();
                var deptRowsLines = new List<List<List<string>>>();

                foreach (var depto in demandasOrdenadas)
                {
                    var valoresDepto = new List<string>
                    {
                        depto.Unidade.Sigla ?? "-",
                        depto.QuantidadeTotal.ToString(cultura),
                    };
                    if (isPatrimonial)
                    {
                        valoresDepto.Add(depto.Justificativa ?? string.Empty);
                    }

                    var linhasCelulaDepto = new List<List<string>>();
                    var maxLinhasDepto = 1;

                    for (int i = 0; i < valoresDepto.Count; i++)
                    {
                        var colIndex = deptColStart + i;
                        var linhas = QuebrarTexto(
                            valoresDepto[i],
                            largurasColunas[colIndex],
                            fonteTexto,
                            gfx
                        );
                        linhasCelulaDepto.Add(linhas);
                        if (linhas.Count > maxLinhasDepto)
                        {
                            maxLinhasDepto = linhas.Count;
                        }
                    }

                    deptRowsLines.Add(linhasCelulaDepto);
                    deptRowHeight.Add(Math.Max(16, maxLinhasDepto * alturaLinhaTexto + 2 * paddingCelula));
                }

                var alturaBlocoDepto = deptRowHeight.Sum();

                var linhasItemPorCelula = new List<List<string>>();
                var maxLinhasItem = 1;
                for (int i = 0; i < valoresItem.Count; i++)
                {
                    var linhas = QuebrarTexto(valoresItem[i], largurasColunas[i], fonteTexto, gfx);
                    linhasItemPorCelula.Add(linhas);
                    if (linhas.Count > maxLinhasItem)
                    {
                        maxLinhasItem = linhas.Count;
                    }
                }

                var alturaMinimaItem = Math.Max(16, maxLinhasItem * alturaLinhaTexto + 2 * paddingCelula);
                var alturaBlocoItem = Math.Max(alturaBlocoDepto, alturaMinimaItem);
                if (alturaBlocoDepto < alturaBlocoItem && deptRowHeight.Count > 0)
                {
                    deptRowHeight[^1] += (alturaBlocoItem - alturaBlocoDepto);
                }

                if (y + alturaBlocoItem > page.Height - margemRodape - 12)
                {
                    DesenharRodape(page, gfx, numeroPagina);
                    page = document.AddPage();
                    page.Size = PdfSharpCore.PageSize.A4;
                    page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                    gfx = XGraphics.FromPdfPage(page);
                    numeroPagina++;
                    DesenharCabecalho(page, gfx);
                    y = margemTopo + 50;
                    alturaCabecalhoTabela = CalcularAlturaCabecalho(gfx);
                    DesenharCabecalhoTabela(page, gfx, y, alturaCabecalhoTabela);
                    y += alturaCabecalhoTabela;
                    alturaLinhaTexto = gfx.MeasureString("Ag", fonteTexto).Height;
                }

                var xItem = margemEsquerda;
                for (int i = 0; i < valoresItem.Count; i++)
                {
                    var larguraColuna = largurasColunas[i];
                    gfx.DrawRectangle(XPens.LightGray, xItem, y, larguraColuna, alturaBlocoItem);

                    var linhas = linhasItemPorCelula[i];
                    for (int j = 0; j < linhas.Count; j++)
                    {
                        gfx.DrawString(
                            linhas[j],
                            fonteTexto,
                            XBrushes.Black,
                            new XPoint(
                                xItem + paddingCelula,
                                y + paddingCelula + alturaLinhaTexto * (j + 1) - 1
                            )
                        );
                    }

                    xItem += larguraColuna;
                }

                var yDepto = y;
                for (int linhaDepto = 0; linhaDepto < demandasOrdenadas.Count; linhaDepto++)
                {
                    var alturaLinhaDepto = deptRowHeight[linhaDepto];
                    var xDepto = margemEsquerda + largurasColunas.Take(deptColStart).Sum();

                    for (int i = 0; i < deptRowsLines[linhaDepto].Count; i++)
                    {
                        var colIndex = deptColStart + i;
                        var larguraColuna = largurasColunas[colIndex];
                        gfx.DrawRectangle(
                            XPens.LightGray,
                            xDepto,
                            yDepto,
                            larguraColuna,
                            alturaLinhaDepto
                        );

                        var linhas = deptRowsLines[linhaDepto][i];
                        for (int j = 0; j < linhas.Count; j++)
                        {
                            gfx.DrawString(
                                linhas[j],
                                fonteTexto,
                                XBrushes.Black,
                                new XPoint(
                                    xDepto + paddingCelula,
                                    yDepto + paddingCelula + alturaLinhaTexto * (j + 1) - 1
                                )
                            );
                        }
                        xDepto += larguraColuna;
                    }

                    yDepto += alturaLinhaDepto;
                }

                y += alturaBlocoItem;
            }

            DesenharRodape(page, gfx, numeroPagina);
            document.Save(stream, false);
            return stream.ToArray();
        }

        /// <summary>
        /// Gera arquivo Excel detalhado do relatório de itens por departamento, incluindo justificativa para itens patrimoniais.
        /// </summary>
        /// <param name="insights">Dados agregados do relatório a serem exportados.</param>
        /// <param name="isPatrimonial">Indica se deve incluir coluna de justificativa patrimonial.</param>
        /// <returns>Conteúdo binário do arquivo Excel (.xlsx).</returns>
        /// <exception cref="Exception">Propaga falhas inesperadas durante montagem e serialização da planilha.</exception>
        public byte[] GerarExcel(List<ItemPorDepartamentoDto> insights, bool isPatrimonial)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Relatório Detalhado");
                int linhaAtual = 1;

                worksheet.Cell(linhaAtual, 1).Value = "CATMAT";
                worksheet.Cell(linhaAtual, 2).Value = "Nome";
                worksheet.Cell(linhaAtual, 3).Value = "Descrição";
                worksheet.Cell(linhaAtual, 4).Value = "Especificação";
                worksheet.Cell(linhaAtual, 5).Value = "Categoria";
                worksheet.Cell(linhaAtual, 6).Value = "Preço Mínimo";
                worksheet.Cell(linhaAtual, 7).Value = "Preço Máximo";
                worksheet.Cell(linhaAtual, 8).Value = "Preço Médio";
                worksheet.Cell(linhaAtual, 9).Value = "Valor Total";
                worksheet.Cell(linhaAtual, 10).Value = "Qtde. Total";

                worksheet.Cell(linhaAtual, 11).Value = "Departamento";
                worksheet.Cell(linhaAtual, 12).Value = "Qtde. Solicitada";
                if (isPatrimonial)
                {
                    worksheet.Cell(linhaAtual, 13).Value = "Justificativa";
                }

                var headerRange = worksheet.Range(1, 1, 1, isPatrimonial ? 13 : 12);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F81BD");
                headerRange.Style.Font.FontColor = XLColor.White;

                linhaAtual++;

                foreach (var item in insights)
                {
                    int primeiraLinhaDoItem = linhaAtual;
                    var demandasOrdenadas = item
                        .DemandaPorDepartamento.GroupBy(d =>
                            NormalizeDepartmentSigla(d.Unidade.Sigla)
                        )
                        .Select(g => new DemandaPorDepartamentoDto
                        {
                            Unidade = new UnidadeOrganizacionalDto
                            {
                                Id = g.First().Unidade.Id,
                                Nome = g.First().Unidade.Nome,
                                Sigla = g.Key,
                                Email = g.First().Unidade.Email,
                                Telefone = g.First().Unidade.Telefone,
                                Tipo = g.First().Unidade.Tipo,
                            },
                            QuantidadeTotal = g.Sum(x => x.QuantidadeTotal),
                            Justificativa = string.Join(
                                "; ",
                                g.Select(x => x.Justificativa)
                                    .Where(j => !string.IsNullOrWhiteSpace(j))
                                    .Distinct()
                            ),
                        })
                        .OrderBy(d => d.Unidade.Nome)
                        .ToList();

                    int totalDeptosParaItem = demandasOrdenadas.Count;

                    bool primeiraIteracao = true;
                    foreach (var depto in demandasOrdenadas)
                    {
                        // Os dados do item só são escritos na primeira linha do grupo
                        if (primeiraIteracao)
                        {
                            worksheet.Cell(linhaAtual, 1).Value = item.CatMat;
                            worksheet.Cell(linhaAtual, 2).Value = item.Nome;
                            worksheet.Cell(linhaAtual, 3).Value = item.Descricao;
                            worksheet.Cell(linhaAtual, 4).Value = item.Especificacao;
                            worksheet.Cell(linhaAtual, 5).Value = item.CategoriaNome;
                            worksheet.Cell(linhaAtual, 6).Value = item.PrecoMinimo;
                            worksheet.Cell(linhaAtual, 7).Value = item.PrecoMaximo;
                            worksheet.Cell(linhaAtual, 8).Value = item.PrecoMedio;
                            worksheet.Cell(linhaAtual, 9).Value = item.ValorTotalSolicitado;
                            worksheet.Cell(linhaAtual, 10).Value = item.QuantidadeTotalSolicitada;
                            primeiraIteracao = false;
                        }

                        // Os dados do departamento são escritos para cada linha
                        worksheet.Cell(linhaAtual, 11).Value = depto.Unidade.Sigla;
                        worksheet.Cell(linhaAtual, 12).Value = depto.QuantidadeTotal;
                        if (isPatrimonial)
                        {
                            worksheet.Cell(linhaAtual, 13).Value = depto.Justificativa;
                        }

                        linhaAtual++;
                    }

                    // --- PASSO 3: MESCLAR AS CÉLULAS DO ITEM ---
                    // Se um item teve mais de um departamento, mescla as células verticalmente
                    if (totalDeptosParaItem > 1)
                    {
                        int ultimaLinhaDoItem = linhaAtual - 1;
                        for (int col = 1; col <= 10; col++) // Mescla as 10 primeiras colunas
                        {
                            var rangeParaMesclar = worksheet.Range(
                                primeiraLinhaDoItem,
                                col,
                                ultimaLinhaDoItem,
                                col
                            );
                            rangeParaMesclar.Merge();
                            rangeParaMesclar.Style.Alignment.SetVertical(
                                XLAlignmentVerticalValues.Center
                            );
                        }
                    }
                }

                // --- PASSO 4: FORMATAÇÃO FINAL ---
                worksheet.Columns().AdjustToContents();
                worksheet.SheetView.FreezeRows(1);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    return stream.ToArray();
                }
            }
        }

        public async Task<List<RelatorioItemSaidaDto>> GetRelatorioItensPorDepartamentoAsync(
            RelatorioItensFiltroDto filtro
        )
        {
            var query = BuildBaseQuery(
                filtro.SearchTerm,
                filtro.CategoriaNome,
                filtro.ItemsType,
                filtro.SiglaDepartamento,
                true
            );

            var dataFimAjustada = filtro.DataFim.Date.AddDays(1).AddTicks(-1);

            query = query.Where(si =>
                si.Solicitacao.DataCriacao >= filtro.DataInicio
                && si.Solicitacao.DataCriacao <= dataFimAjustada
            );

            var itensFiltrados = await query
                .Include(si => si.Item)
                .ThenInclude(i => i.Categoria)
                .Select(si => new
                {
                    si.Item,
                    si.Quantidade,
                    si.ValorUnitario,
                })
                .ToListAsync();

            var relatorio = itensFiltrados
                .GroupBy(x => x.Item.Id)
                .Select(g =>
                {
                    var itemInfo = g.First().Item;
                    return new RelatorioItemSaidaDto
                    {
                        ItemId = itemInfo.Id,
                        Nome = itemInfo.Nome,
                        CatMat = itemInfo.CatMat,
                        UnidadeMedida = itemInfo.Especificacao,
                        Categoria = itemInfo.Categoria.Nome,

                        QuantidadeSolicitada = (int)g.Sum(x => x.Quantidade),
                        ValorMedioUnitario = g.Average(x => x.ValorUnitario),
                        ValorTotalGasto = g.Sum(x => x.Quantidade * x.ValorUnitario),
                    };
                })
                .AsQueryable();

            relatorio =
                filtro.SortOrder?.ToLower() == "desc"
                    ? relatorio.OrderByDescending(x => x.QuantidadeSolicitada)
                    : relatorio.OrderBy(x => x.Nome);

            return relatorio.ToList();
        }
    }
}
