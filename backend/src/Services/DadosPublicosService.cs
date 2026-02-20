using System.Text.RegularExpressions;
using ComprasTccApp.Backend.Domain;
using ComprasTccApp.Models.Entities.Solicitacoes;
using Database;
using Microsoft.EntityFrameworkCore;
using Models.Dtos;
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
    }
}
