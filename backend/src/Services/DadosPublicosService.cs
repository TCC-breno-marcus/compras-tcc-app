using System.Text.RegularExpressions;
using System.Text;
using System.Globalization;
using ComprasTccApp.Backend.Domain;
using ComprasTccApp.Models.Entities.Solicitacoes;
using Database;
using Microsoft.EntityFrameworkCore;
using Models.Dtos;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using Services.Interfaces;

namespace Services
{
    public class DadosPublicosService : IDadosPublicosService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DadosPublicosService> _logger;

        /// <summary>
        /// Inicializa o serviço público de consulta de dados de solicitações.
        /// </summary>
        /// <param name="context">Contexto de dados usado nas consultas.</param>
        /// <param name="logger">Logger para observabilidade das consultas públicas.</param>
        public DadosPublicosService(AppDbContext context, ILogger<DadosPublicosService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Consulta solicitações com filtros públicos e retorna dados agregados sem exposição de informações sensíveis.
        /// </summary>
        /// <param name="dataInicio">Data inicial de criação da solicitação (inclusive).</param>
        /// <param name="dataFim">Data final de criação da solicitação (inclusive).</param>
        /// <param name="statusId">Filtro por identificador do status.</param>
        /// <param name="statusNome">Filtro parcial por nome do status.</param>
        /// <param name="siglaDepartamento">Filtro por sigla do departamento solicitante.</param>
        /// <param name="categoriaNome">Filtro parcial por nome da categoria do item.</param>
        /// <param name="itemNome">Filtro parcial por nome do item solicitado.</param>
        /// <param name="catMat">Filtro parcial por CATMAT do item.</param>
        /// <param name="itemsType">Tipo da solicitação: geral ou patrimonial.</param>
        /// <param name="valorMinimo">Valor mínimo total da solicitação.</param>
        /// <param name="valorMaximo">Valor máximo total da solicitação.</param>
        /// <param name="somenteSolicitacoesAtivas">Se true, considera apenas status ativos de negócio.</param>
        /// <param name="pageNumber">Número da página (base 1).</param>
        /// <param name="pageSize">Tamanho da página.</param>
        /// <returns>Resultado paginado com métricas e dados mascarados.</returns>
        public async Task<PublicoSolicitacaoConsultaResultDto> ConsultarSolicitacoesAsync(
            DateTime? dataInicio,
            DateTime? dataFim,
            int? statusId,
            string? statusNome,
            string? siglaDepartamento,
            string? categoriaNome,
            string? itemNome,
            string? catMat,
            string? itemsType,
            decimal? valorMinimo,
            decimal? valorMaximo,
            bool? somenteSolicitacoesAtivas,
            int pageNumber,
            int pageSize
        )
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 25 : pageSize;

            IQueryable<Solicitacao> query = _context.Solicitacoes.AsNoTracking();
            query = AplicarFiltros(
                query,
                dataInicio,
                dataFim,
                statusId,
                statusNome,
                siglaDepartamento,
                categoriaNome,
                itemNome,
                catMat,
                itemsType,
                valorMinimo,
                valorMaximo,
                somenteSolicitacoesAtivas
            );

            var totalCount = await query.CountAsync();
            var totalItensSolicitados =
                await query.SelectMany(s => s.ItemSolicitacao).SumAsync(i => (decimal?)i.Quantidade)
                ?? 0m;
            var valorTotalSolicitado =
                await query.SelectMany(s => s.ItemSolicitacao)
                    .SumAsync(i => (decimal?)(i.Quantidade * i.ValorUnitario))
                ?? 0m;

            var solicitacoes = await query
                .Include(s => s.Status)
                .Include(s => s.Solicitante)
                .ThenInclude(sol => sol.Departamento)
                .Include(s => s.Solicitante)
                .ThenInclude(sol => sol.Servidor)
                .ThenInclude(serv => serv.Pessoa)
                .Include(s => s.ItemSolicitacao)
                .ThenInclude(si => si.Item)
                .ThenInclude(i => i.Categoria)
                .OrderByDescending(s => s.DataCriacao)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dados = solicitacoes.Select(MapearSolicitacaoPublica).ToList();
            _logger.LogInformation(
                "Consulta pública de solicitações executada com sucesso. Total filtrado: {Total}. Página: {Page}.",
                totalCount,
                pageNumber
            );

            return new PublicoSolicitacaoConsultaResultDto
            {
                Data = dados,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                TotalItensSolicitados = totalItensSolicitados,
                ValorTotalSolicitado = valorTotalSolicitado,
            };
        }

        /// <summary>
        /// Exporta as solicitações públicas filtradas em CSV, mantendo os dados sensíveis mascarados.
        /// </summary>
        /// <param name="dataInicio">Data inicial de criação da solicitação (inclusive).</param>
        /// <param name="dataFim">Data final de criação da solicitação (inclusive).</param>
        /// <param name="statusId">Filtro por identificador do status.</param>
        /// <param name="statusNome">Filtro parcial por nome do status.</param>
        /// <param name="siglaDepartamento">Filtro por sigla do departamento solicitante.</param>
        /// <param name="categoriaNome">Filtro parcial por nome da categoria do item.</param>
        /// <param name="itemNome">Filtro parcial por nome do item solicitado.</param>
        /// <param name="catMat">Filtro parcial por CATMAT do item.</param>
        /// <param name="itemsType">Tipo da solicitação: geral ou patrimonial.</param>
        /// <param name="valorMinimo">Valor mínimo total da solicitação.</param>
        /// <param name="valorMaximo">Valor máximo total da solicitação.</param>
        /// <param name="somenteSolicitacoesAtivas">Se true, considera apenas status ativos de negócio.</param>
        /// <param name="pageNumber">Número da página (base 1).</param>
        /// <param name="pageSize">Tamanho da página.</param>
        /// <returns>Arquivo CSV serializado em bytes.</returns>
        public async Task<byte[]> ExportarSolicitacoesCsvAsync(
            DateTime? dataInicio,
            DateTime? dataFim,
            int? statusId,
            string? statusNome,
            string? siglaDepartamento,
            string? categoriaNome,
            string? itemNome,
            string? catMat,
            string? itemsType,
            decimal? valorMinimo,
            decimal? valorMaximo,
            bool? somenteSolicitacoesAtivas,
            int pageNumber,
            int pageSize
        )
        {
            var resultado = await ConsultarSolicitacoesAsync(
                dataInicio,
                dataFim,
                statusId,
                statusNome,
                siglaDepartamento,
                categoriaNome,
                itemNome,
                catMat,
                itemsType,
                valorMinimo,
                valorMaximo,
                somenteSolicitacoesAtivas,
                pageNumber,
                pageSize
            );

            var sb = new StringBuilder();
            sb.AppendLine(
                "SolicitacaoId,ExternalId,DataCriacao,TipoSolicitacao,StatusId,StatusNome,SolicitanteNomeMascarado,SolicitanteEmailMascarado,SolicitanteTelefoneMascarado,SolicitanteCpfMascarado,DepartamentoNome,DepartamentoSigla,ValorTotalSolicitacao,JustificativaGeral,ItemId,ItemNome,CatMat,CategoriaNome,Quantidade,ValorUnitario,ValorTotal,Justificativa por Item"
            );

            foreach (var solicitacao in resultado.Data)
            {
                if (solicitacao.Itens.Count == 0)
                {
                    sb.AppendLine(
                        string.Join(
                            ",",
                            CsvEscapar(solicitacao.Id),
                            CsvEscapar(solicitacao.ExternalId),
                            CsvEscapar(solicitacao.DataCriacao.ToString("yyyy-MM-ddTHH:mm:ss")),
                            CsvEscapar(solicitacao.TipoSolicitacao),
                            CsvEscapar(solicitacao.StatusId),
                            CsvEscapar(solicitacao.StatusNome),
                            CsvEscapar(solicitacao.SolicitanteNomeMascarado),
                            CsvEscapar(solicitacao.SolicitanteEmailMascarado),
                            CsvEscapar(solicitacao.SolicitanteTelefoneMascarado),
                            CsvEscapar(solicitacao.SolicitanteCpfMascarado),
                            CsvEscapar(solicitacao.DepartamentoNome),
                            CsvEscapar(solicitacao.DepartamentoSigla),
                            CsvEscapar(solicitacao.ValorTotalSolicitacao),
                            CsvEscapar(solicitacao.JustificativaGeral),
                            CsvEscapar(string.Empty),
                            CsvEscapar(string.Empty),
                            CsvEscapar(string.Empty),
                            CsvEscapar(string.Empty),
                            CsvEscapar(string.Empty),
                            CsvEscapar(string.Empty),
                            CsvEscapar(string.Empty),
                            CsvEscapar(string.Empty)
                        )
                    );
                    continue;
                }

                foreach (var item in solicitacao.Itens)
                {
                    sb.AppendLine(
                        string.Join(
                            ",",
                            CsvEscapar(solicitacao.Id),
                            CsvEscapar(solicitacao.ExternalId),
                            CsvEscapar(solicitacao.DataCriacao.ToString("yyyy-MM-ddTHH:mm:ss")),
                            CsvEscapar(solicitacao.TipoSolicitacao),
                            CsvEscapar(solicitacao.StatusId),
                            CsvEscapar(solicitacao.StatusNome),
                            CsvEscapar(solicitacao.SolicitanteNomeMascarado),
                            CsvEscapar(solicitacao.SolicitanteEmailMascarado),
                            CsvEscapar(solicitacao.SolicitanteTelefoneMascarado),
                            CsvEscapar(solicitacao.SolicitanteCpfMascarado),
                            CsvEscapar(solicitacao.DepartamentoNome),
                            CsvEscapar(solicitacao.DepartamentoSigla),
                            CsvEscapar(solicitacao.ValorTotalSolicitacao),
                            CsvEscapar(solicitacao.JustificativaGeral),
                            CsvEscapar(item.ItemId),
                            CsvEscapar(item.ItemNome),
                            CsvEscapar(item.CatMat),
                            CsvEscapar(item.CategoriaNome),
                            CsvEscapar(item.Quantidade),
                            CsvEscapar(item.ValorUnitario),
                            CsvEscapar(item.ValorTotal),
                            CsvEscapar(item.Justificativa)
                        )
                    );
                }
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        /// <summary>
        /// Exporta as solicitações públicas filtradas em PDF, mantendo os dados sensíveis mascarados.
        /// </summary>
        public async Task<byte[]> ExportarSolicitacoesPdfAsync(
            DateTime? dataInicio,
            DateTime? dataFim,
            int? statusId,
            string? statusNome,
            string? siglaDepartamento,
            string? categoriaNome,
            string? itemNome,
            string? catMat,
            string? itemsType,
            decimal? valorMinimo,
            decimal? valorMaximo,
            bool? somenteSolicitacoesAtivas,
            int pageNumber,
            int pageSize
        )
        {
            var resultado = await ConsultarSolicitacoesAsync(
                dataInicio,
                dataFim,
                statusId,
                statusNome,
                siglaDepartamento,
                categoriaNome,
                itemNome,
                catMat,
                itemsType,
                valorMinimo,
                valorMaximo,
                somenteSolicitacoesAtivas,
                pageNumber,
                pageSize
            );

            PdfFontResolver.EnsureInitialized();
            var cultura = new CultureInfo("pt-BR");
            using var stream = new MemoryStream();
            using var document = new PdfDocument();

            PdfPage page = document.AddPage();
            page.Size = PdfSharpCore.PageSize.A4;
            page.Orientation = PdfSharpCore.PageOrientation.Landscape;

            XGraphics gfx = XGraphics.FromPdfPage(page);
            var fonteTitulo = new XFont("AppSans", 12, XFontStyle.Bold);
            var fonteTexto = new XFont("AppSans", 8, XFontStyle.Regular);
            var fonteCabecalho = new XFont("AppSans", 8, XFontStyle.Bold);

            const double margemEsquerda = 24;
            const double margemDireita = 24;
            const double margemTopo = 20;
            const double margemRodape = 20;
            const double paddingCelula = 3;
            var larguraUtil = page.Width - margemEsquerda - margemDireita;
            var y = margemTopo + 42;
            var numeroPagina = 1;

            var colunas = new List<string>
            {
                "Solicitacao",
                "Data",
                "Tipo",
                "Status",
                "Departamento",
                "Solicitante",
                "Email",
                "Telefone",
                "Valor Solicitacao",
                "Justificativa Geral",
                "Item",
                "CatMat",
                "Categoria",
                "Quantidade",
                "Valor Unitario",
                "Valor Total Item",
                "Justificativa por Item",
            };
            var itemColStart = 10;
            var pesos = new List<double>
            {
                1.15, 0.9, 0.9, 0.95, 0.85, 1.05, 1.2, 1.0, 0.9, 1.25, 1.35, 0.85, 0.95, 0.7, 0.9,
                0.95, 1.25,
            };
            var somaPesos = pesos.Sum();
            var larguras = pesos.Select(p => larguraUtil * (p / somaPesos)).ToList();

            List<string> QuebrarTexto(string? texto, double larguraColuna, XFont fonte, XGraphics grafico)
            {
                if (string.IsNullOrWhiteSpace(texto))
                {
                    return [string.Empty];
                }

                var limite = Math.Max(larguraColuna - (2 * paddingCelula), 10);
                var linhas = new List<string>();
                var palavras = texto.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var linhaAtual = string.Empty;

                foreach (var palavra in palavras)
                {
                    var tentativa = string.IsNullOrEmpty(linhaAtual) ? palavra : $"{linhaAtual} {palavra}";
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
                    foreach (var ch in palavra)
                    {
                        var tentativaTrecho = trecho + ch;
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
                            trecho = ch.ToString();
                        }
                    }
                    linhaAtual = trecho;
                }

                if (!string.IsNullOrEmpty(linhaAtual))
                {
                    linhas.Add(linhaAtual);
                }

                return linhas.Count > 0 ? linhas : [string.Empty];
            }

            void DesenharCabecalhoPagina(PdfPage paginaAtual, XGraphics graficoAtual)
            {
                graficoAtual.DrawString(
                    "Dados Publicos - Solicitacoes",
                    fonteTitulo,
                    XBrushes.Black,
                    new XRect(margemEsquerda, margemTopo, larguraUtil, 16),
                    XStringFormats.TopLeft
                );
                graficoAtual.DrawString(
                    $"Gerado em: {DateTimeOffset.UtcNow:dd/MM/yyyy HH:mm:ss} | Registros: {resultado.TotalCount}",
                    fonteTexto,
                    XBrushes.Black,
                    new XRect(margemEsquerda, margemTopo + 16, larguraUtil, 12),
                    XStringFormats.TopLeft
                );
                graficoAtual.DrawLine(
                    XPens.Gray,
                    margemEsquerda,
                    margemTopo + 34,
                    paginaAtual.Width - margemDireita,
                    margemTopo + 34
                );
            }

            double CalcularAlturaCabecalhoTabela(XGraphics graficoAtual)
            {
                var alturaLinha = graficoAtual.MeasureString("Ag", fonteCabecalho).Height;
                var maxLinhas = 1;
                for (var i = 0; i < colunas.Count; i++)
                {
                    var linhas = QuebrarTexto(colunas[i], larguras[i], fonteCabecalho, graficoAtual);
                    if (linhas.Count > maxLinhas)
                    {
                        maxLinhas = linhas.Count;
                    }
                }

                return Math.Max(20, maxLinhas * alturaLinha + 2 * paddingCelula);
            }

            var alturaCabecalhoTabela = CalcularAlturaCabecalhoTabela(gfx);

            void DesenharCabecalhoTabela(XGraphics graficoAtual, double yCabecalho, double alturaCabecalho)
            {
                var x = margemEsquerda;
                var alturaLinha = graficoAtual.MeasureString("Ag", fonteCabecalho).Height;

                for (var i = 0; i < colunas.Count; i++)
                {
                    graficoAtual.DrawRectangle(
                        XBrushes.LightGray,
                        x,
                        yCabecalho,
                        larguras[i],
                        alturaCabecalho
                    );
                    graficoAtual.DrawRectangle(XPens.Gray, x, yCabecalho, larguras[i], alturaCabecalho);

                    var linhas = QuebrarTexto(colunas[i], larguras[i], fonteCabecalho, graficoAtual);
                    for (var j = 0; j < linhas.Count; j++)
                    {
                        graficoAtual.DrawString(
                            linhas[j],
                            fonteCabecalho,
                            XBrushes.Black,
                            new XPoint(
                                x + paddingCelula,
                                yCabecalho + paddingCelula + alturaLinha * (j + 1) - 1
                            )
                        );
                    }
                    x += larguras[i];
                }
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

            DesenharCabecalhoPagina(page, gfx);
            DesenharCabecalhoTabela(gfx, y, alturaCabecalhoTabela);
            y += alturaCabecalhoTabela;

            var alturaLinhaTexto = gfx.MeasureString("Ag", fonteTexto).Height;

            foreach (var solicitacao in resultado.Data)
            {
                var linhasSolicitacao = solicitacao.Itens.Count == 0
                    ? new List<PublicoSolicitacaoItemDto?> { null }
                    : solicitacao.Itens.Select(i => (PublicoSolicitacaoItemDto?)i).ToList();

                var valoresSolicitacao = new List<string>
                {
                    solicitacao.ExternalId ?? solicitacao.Id.ToString(cultura),
                    solicitacao.DataCriacao.ToString("dd/MM/yyyy", cultura),
                    solicitacao.TipoSolicitacao,
                    solicitacao.StatusNome,
                    solicitacao.DepartamentoSigla,
                    solicitacao.SolicitanteNomeMascarado,
                    solicitacao.SolicitanteEmailMascarado,
                    solicitacao.SolicitanteTelefoneMascarado,
                    solicitacao.ValorTotalSolicitacao.ToString("C", cultura),
                    solicitacao.JustificativaGeral ?? "-",
                };

                var linhasSolicitacaoPorCelula = new List<List<string>>();
                var maxLinhasSolicitacao = 1;
                for (var i = 0; i < itemColStart; i++)
                {
                    var linhas = QuebrarTexto(valoresSolicitacao[i], larguras[i], fonteTexto, gfx);
                    linhasSolicitacaoPorCelula.Add(linhas);
                    if (linhas.Count > maxLinhasSolicitacao)
                    {
                        maxLinhasSolicitacao = linhas.Count;
                    }
                }
                var alturaMinSolicitacao = Math.Max(
                    16,
                    maxLinhasSolicitacao * alturaLinhaTexto + 2 * paddingCelula
                );

                var linhasItemPorLinha = new List<List<List<string>>>();
                var alturasLinhaItens = new List<double>();

                foreach (var item in linhasSolicitacao)
                {
                    var valoresItem = new List<string>
                    {
                        item?.ItemNome ?? "-",
                        item?.CatMat ?? "-",
                        item?.CategoriaNome ?? "-",
                        item?.Quantidade.ToString(cultura) ?? "-",
                        item?.ValorUnitario.ToString("C", cultura) ?? "-",
                        item?.ValorTotal.ToString("C", cultura) ?? "-",
                        item?.Justificativa ?? "-",
                    };

                    var linhasItemPorCelula = new List<List<string>>();
                    var maxLinhas = 1;
                    for (var i = 0; i < valoresItem.Count; i++)
                    {
                        var linhas = QuebrarTexto(
                            valoresItem[i],
                            larguras[itemColStart + i],
                            fonteTexto,
                            gfx
                        );
                        linhasItemPorCelula.Add(linhas);
                        if (linhas.Count > maxLinhas)
                        {
                            maxLinhas = linhas.Count;
                        }
                    }

                    var alturaLinha = Math.Max(16, maxLinhas * alturaLinhaTexto + 2 * paddingCelula);
                    linhasItemPorLinha.Add(linhasItemPorCelula);
                    alturasLinhaItens.Add(alturaLinha);
                }

                var alturaBlocoItens = alturasLinhaItens.Sum();
                var alturaBloco = Math.Max(alturaBlocoItens, alturaMinSolicitacao);
                if (alturaBlocoItens < alturaBloco && alturasLinhaItens.Count > 0)
                {
                    alturasLinhaItens[^1] += alturaBloco - alturaBlocoItens;
                }

                if (y + alturaBloco > page.Height - margemRodape - 12)
                {
                    DesenharRodape(page, gfx, numeroPagina);
                    page = document.AddPage();
                    page.Size = PdfSharpCore.PageSize.A4;
                    page.Orientation = PdfSharpCore.PageOrientation.Landscape;
                    gfx = XGraphics.FromPdfPage(page);
                    numeroPagina++;
                    DesenharCabecalhoPagina(page, gfx);
                    alturaCabecalhoTabela = CalcularAlturaCabecalhoTabela(gfx);
                    y = margemTopo + 42;
                    DesenharCabecalhoTabela(gfx, y, alturaCabecalhoTabela);
                    y += alturaCabecalhoTabela;
                    alturaLinhaTexto = gfx.MeasureString("Ag", fonteTexto).Height;
                }

                var xSolicitacao = margemEsquerda;
                for (var i = 0; i < itemColStart; i++)
                {
                    gfx.DrawRectangle(XPens.LightGray, xSolicitacao, y, larguras[i], alturaBloco);
                    var linhas = linhasSolicitacaoPorCelula[i];
                    for (var j = 0; j < linhas.Count; j++)
                    {
                        gfx.DrawString(
                            linhas[j],
                            fonteTexto,
                            XBrushes.Black,
                            new XPoint(
                                xSolicitacao + paddingCelula,
                                y + paddingCelula + alturaLinhaTexto * (j + 1) - 1
                            )
                        );
                    }
                    xSolicitacao += larguras[i];
                }

                var yItem = y;
                for (var linha = 0; linha < linhasItemPorLinha.Count; linha++)
                {
                    var alturaLinha = alturasLinhaItens[linha];
                    var xItem = margemEsquerda + larguras.Take(itemColStart).Sum();
                    for (var colItem = 0; colItem < linhasItemPorLinha[linha].Count; colItem++)
                    {
                        var larguraCol = larguras[itemColStart + colItem];
                        gfx.DrawRectangle(XPens.LightGray, xItem, yItem, larguraCol, alturaLinha);
                        var linhas = linhasItemPorLinha[linha][colItem];
                        for (var j = 0; j < linhas.Count; j++)
                        {
                            gfx.DrawString(
                                linhas[j],
                                fonteTexto,
                                XBrushes.Black,
                                new XPoint(
                                    xItem + paddingCelula,
                                    yItem + paddingCelula + alturaLinhaTexto * (j + 1) - 1
                                )
                            );
                        }
                        xItem += larguraCol;
                    }
                    yItem += alturaLinha;
                }

                y += alturaBloco;
            }

            DesenharRodape(page, gfx, numeroPagina);
            document.Save(stream, false);
            return stream.ToArray();
        }

        private static IQueryable<Solicitacao> AplicarFiltros(
            IQueryable<Solicitacao> query,
            DateTime? dataInicio,
            DateTime? dataFim,
            int? statusId,
            string? statusNome,
            string? siglaDepartamento,
            string? categoriaNome,
            string? itemNome,
            string? catMat,
            string? itemsType,
            decimal? valorMinimo,
            decimal? valorMaximo,
            bool? somenteSolicitacoesAtivas
        )
        {
            if (dataInicio.HasValue)
            {
                query = query.Where(s => s.DataCriacao >= dataInicio.Value);
            }

            if (dataFim.HasValue)
            {
                query = query.Where(s => s.DataCriacao <= dataFim.Value);
            }

            if (statusId.HasValue)
            {
                query = query.Where(s => s.StatusId == statusId.Value);
            }

            if (!string.IsNullOrWhiteSpace(statusNome))
            {
                var status = statusNome.ToLower();
                query = query.Where(s => s.Status.Nome.ToLower().Contains(status));
            }

            if (!string.IsNullOrWhiteSpace(siglaDepartamento))
            {
                var sigla = siglaDepartamento.ToUpper();
                query = query.Where(s => s.Solicitante.Departamento.Sigla.ToUpper() == sigla);
            }

            if (!string.IsNullOrWhiteSpace(categoriaNome))
            {
                var categoria = categoriaNome.ToLower();
                query = query.Where(s =>
                    s.ItemSolicitacao.Any(si => si.Item.Categoria.Nome.ToLower().Contains(categoria))
                );
            }

            if (!string.IsNullOrWhiteSpace(itemNome))
            {
                var nome = itemNome.ToLower();
                query = query.Where(s =>
                    s.ItemSolicitacao.Any(si => si.Item.Nome.ToLower().Contains(nome))
                );
            }

            if (!string.IsNullOrWhiteSpace(catMat))
            {
                var catmatFiltro = catMat.ToLower();
                query = query.Where(s =>
                    s.ItemSolicitacao.Any(si => si.Item.CatMat.ToLower().Contains(catmatFiltro))
                );
            }

            if (!string.IsNullOrWhiteSpace(itemsType))
            {
                if (itemsType.Equals("geral", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.OfType<SolicitacaoGeral>();
                }
                else if (itemsType.Equals("patrimonial", StringComparison.OrdinalIgnoreCase))
                {
                    query = query.OfType<SolicitacaoPatrimonial>();
                }
            }

            if (valorMinimo.HasValue)
            {
                query = query.Where(s =>
                    s.ItemSolicitacao.Sum(si => si.Quantidade * si.ValorUnitario) >= valorMinimo.Value
                );
            }

            if (valorMaximo.HasValue)
            {
                query = query.Where(s =>
                    s.ItemSolicitacao.Sum(si => si.Quantidade * si.ValorUnitario) <= valorMaximo.Value
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
                query = query.Where(s => statusAtivos.Contains(s.StatusId));
            }

            return query;
        }

        private static PublicoSolicitacaoDto MapearSolicitacaoPublica(Solicitacao solicitacao)
        {
            var pessoa = solicitacao.Solicitante.Servidor.Pessoa;
            var departamento = solicitacao.Solicitante.Departamento;

            return new PublicoSolicitacaoDto
            {
                Id = solicitacao.Id,
                ExternalId = solicitacao.ExternalId,
                DataCriacao = solicitacao.DataCriacao,
                TipoSolicitacao = solicitacao is SolicitacaoGeral ? "GERAL" : "PATRIMONIAL",
                StatusId = solicitacao.StatusId,
                StatusNome = solicitacao.Status.Nome,
                SolicitanteNomeMascarado = MascararNome(pessoa.Nome),
                SolicitanteEmailMascarado = MascararEmail(pessoa.Email),
                SolicitanteTelefoneMascarado = MascararTelefone(pessoa.Telefone),
                SolicitanteCpfMascarado = MascararCpf(pessoa.CPF),
                DepartamentoNome = departamento.Nome,
                DepartamentoSigla = departamento.Sigla,
                ValorTotalSolicitacao = solicitacao.ItemSolicitacao.Sum(i => i.Quantidade * i.ValorUnitario),
                JustificativaGeral = solicitacao is SolicitacaoGeral solicitacaoGeral
                    ? solicitacaoGeral.JustificativaGeral
                    : null,
                Itens = solicitacao
                    .ItemSolicitacao.Select(si => new PublicoSolicitacaoItemDto
                    {
                        ItemId = si.ItemId,
                        ItemNome = si.Item.Nome,
                        CatMat = si.Item.CatMat,
                        CategoriaNome = si.Item.Categoria.Nome,
                        Quantidade = si.Quantidade,
                        ValorUnitario = si.ValorUnitario,
                        ValorTotal = si.Quantidade * si.ValorUnitario,
                        Justificativa = string.IsNullOrWhiteSpace(si.Justificativa)
                            ? null
                            : si.Justificativa,
                    })
                    .OrderBy(i => i.ItemNome)
                    .ToList(),
            };
        }

        private static string MascararNome(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
            {
                return string.Empty;
            }

            var partes = nome.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(
                " ",
                partes.Select(p => p.Length <= 1 ? "*" : $"{p[0]}{new string('*', p.Length - 1)}")
            );
        }

        private static string MascararEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            {
                return "***";
            }

            var partes = email.Split('@');
            var usuario = partes[0];
            var dominio = partes[1];
            if (usuario.Length <= 2)
            {
                return $"**@{dominio}";
            }

            return $"{usuario[..2]}***@{dominio}";
        }

        private static string MascararTelefone(string telefone)
        {
            var digitos = Regex.Replace(telefone ?? string.Empty, "[^0-9]", "");
            if (digitos.Length <= 4)
            {
                return new string('*', digitos.Length);
            }

            return $"{new string('*', digitos.Length - 4)}{digitos[^4..]}";
        }

        private static string MascararCpf(string cpf)
        {
            var digitos = Regex.Replace(cpf ?? string.Empty, "[^0-9]", "");
            if (digitos.Length != 11)
            {
                return "***";
            }

            return $"***.***.***-{digitos[^2..]}";
        }

        private static string CsvEscapar(object? valor)
        {
            var texto = valor?.ToString() ?? string.Empty;
            var textoEscapado = texto.Replace("\"", "\"\"");
            return $"\"{textoEscapado}\"";
        }
    }
}
