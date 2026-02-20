using System.Security.Claims;
using ComprasTccApp.Backend.Domain;
using ComprasTccApp.Backend.Enums;
using ComprasTccApp.Backend.Helpers;
using ComprasTccApp.Models.Entities.Historicos;
using ComprasTccApp.Models.Entities.Itens;
using ComprasTccApp.Models.Entities.Solicitacoes;
using ComprasTccApp.Services.Interfaces;
using Database;
using Microsoft.EntityFrameworkCore;
using Models.Dtos;
using Services.Interfaces;

public class SolicitacaoService : ISolicitacaoService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SolicitacaoService> _logger;
    private readonly IConfiguracaoService _configuracaoService;
    private readonly IUsuarioService _usuarioService;
    private readonly string _IMAGE_BASE_URL;

    /// <summary>
    /// Inicializa o serviço responsável pelo ciclo de vida das solicitações de compras.
    /// </summary>
    /// <param name="context">Contexto de dados para consulta e persistência das solicitações.</param>
    /// <param name="logger">Logger para auditoria e rastreio de falhas do serviço.</param>
    /// <param name="configuracaoService">Serviço de configurações usado para validar regras de prazo.</param>
    /// <param name="usuarioService">Serviço de usuários usado para resolver o solicitante autenticado.</param>
    /// <param name="configuration">Configuração da aplicação para obtenção da URL base de imagens.</param>
    public SolicitacaoService(
        AppDbContext context,
        ILogger<SolicitacaoService> logger,
        IConfiguracaoService configuracaoService,
        IUsuarioService usuarioService,
        IConfiguration configuration
    )
    {
        _context = context;
        _logger = logger;
        _configuracaoService = configuracaoService;
        _usuarioService = usuarioService;
        _IMAGE_BASE_URL = configuration["IMAGE_BASE_URL"] ?? "";
    }

    /// <summary>
    /// Cria uma solicitação do tipo geral para o solicitante informado, validando prazo de submissão e integridade dos itens.
    /// </summary>
    /// <param name="dto">Dados da solicitação geral, incluindo justificativa e itens solicitados.</param>
    /// <param name="pessoaId">Identificador da pessoa que está criando a solicitação.</param>
    /// <returns>DTO com os dados completos da solicitação criada.</returns>
    /// <exception cref="InvalidOperationException">Lançada quando o prazo de submissão já encerrou ou quando o status pendente não é encontrado.</exception>
    /// <exception cref="Exception">Lançada quando algum item informado não existe ou está inativo.</exception>
    public async Task<SolicitacaoResultDto> CreateGeralAsync(
        CreateSolicitacaoGeralDto dto,
        long pessoaId
    )
    {
        var configuracoes = await _configuracaoService.GetConfiguracoesAsync();
        var prazoSubmissao = configuracoes.PrazoSubmissao;
        if (prazoSubmissao.HasValue && DateTime.UtcNow > prazoSubmissao.Value)
        {
            string prazoSubmissaoFormatado = prazoSubmissao.Value.ToString("dd/MM/yyyy 'às' HH:mm");
            throw new InvalidOperationException(
                $"O prazo para a criação de solicitações encerrou em {prazoSubmissaoFormatado}."
            );
        }

        var (servidor, solicitante) = await _usuarioService.GetSolicitanteInfoAsync(pessoaId);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var novaSolicitacao = new SolicitacaoGeral
            {
                SolicitanteId = solicitante.Id,
                DataCriacao = DateTime.UtcNow,
                StatusId = StatusConsts.Pendente,
                JustificativaGeral = dto.JustificativaGeral,
            };

            var itensDaSolicitacao = new List<SolicitacaoItem>();

            foreach (var itemDto in dto.Itens)
            {
                var itemDoCatalogo = await _context.Items.FindAsync(itemDto.ItemId);
                if (itemDoCatalogo == null || !itemDoCatalogo.IsActive)
                {
                    throw new Exception(
                        $"Item com ID {itemDto.ItemId} não existe ou está inativo."
                    );
                }

                var solicitacaoItem = new SolicitacaoItem
                {
                    Solicitacao = novaSolicitacao,
                    Item = itemDoCatalogo,
                    ItemId = itemDto.ItemId,
                    Quantidade = itemDto.Quantidade,
                    ValorUnitario = itemDto.ValorUnitario,
                    Justificativa = null,
                };
                itensDaSolicitacao.Add(solicitacaoItem);
            }

            novaSolicitacao.ItemSolicitacao = itensDaSolicitacao;

            await _context.Solicitacoes.AddAsync(novaSolicitacao);
            await _context.SaveChangesAsync();

            novaSolicitacao.ExternalId = GenerateExternalId(novaSolicitacao);

            var historico = new HistoricoSolicitacao
            {
                SolicitacaoId = novaSolicitacao.Id,
                DataOcorrencia = DateTime.UtcNow,
                PessoaId = pessoaId,
                Acao = AcaoHistoricoEnum.Criacao,
                Detalhes = "Solicitação criada.",
                Observacoes = "",
            };

            await _context.HistoricoSolicitacoes.AddAsync(historico);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var status = await _context.StatusSolicitacoes.FindAsync(StatusConsts.Pendente);
            if (status == null)
            {
                throw new InvalidOperationException(
                    $"Status com ID {StatusConsts.Pendente} não encontrado."
                );
            }

            var respostaDto = new SolicitacaoResultDto
            {
                Id = novaSolicitacao.Id,
                DataCriacao = novaSolicitacao.DataCriacao,
                JustificativaGeral = novaSolicitacao.JustificativaGeral,
                ExternalId = novaSolicitacao.ExternalId,
                Status = new StatusSolicitacaoDto
                {
                    Id = status.Id,
                    Nome = status.Nome,
                    Descricao = status.Descricao,
                },
                Solicitante = new SolicitanteDto
                {
                    Id = solicitante.Id,
                    Nome = servidor.Pessoa.Nome,
                    Email = servidor.Pessoa.Email,
                    Unidade =
                        solicitante.Departamento != null
                            ? new UnidadeOrganizacionalDto
                            {
                                Id = solicitante.Departamento.Id,
                                Nome = solicitante.Departamento.Nome,
                                Sigla = solicitante.Departamento.Sigla,
                                Email = solicitante.Departamento.Email,
                                Telefone = solicitante.Departamento.Telefone,
                                Tipo = "Departamento",
                            }
                            : null,
                },
                Itens = novaSolicitacao
                    .ItemSolicitacao.Select(item => new ItemSolicitacaoResultDto
                    {
                        Id = item.ItemId,
                        Nome = item.Item.Nome,
                        CatMat = item.Item.CatMat,
                        Quantidade = item.Quantidade,
                        LinkImagem = string.IsNullOrWhiteSpace(item.Item.LinkImagem)
                            ? item.Item.LinkImagem
                            : $"{_IMAGE_BASE_URL}{item.Item.LinkImagem}",
                        PrecoSugerido = item.ValorUnitario,
                        Justificativa = item.Justificativa,
                    })
                    .ToList(),
            };

            return respostaDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar solicitação geral.");
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Cria uma solicitação do tipo patrimonial para o solicitante informado, validando prazo de submissão e itens ativos.
    /// </summary>
    /// <param name="dto">Dados da solicitação patrimonial com os itens e justificativas por item.</param>
    /// <param name="pessoaId">Identificador da pessoa que está criando a solicitação.</param>
    /// <returns>DTO com os dados completos da solicitação criada.</returns>
    /// <exception cref="InvalidOperationException">Lançada quando o prazo de submissão já encerrou ou quando o status pendente não é encontrado.</exception>
    /// <exception cref="Exception">Lançada quando algum item informado não existe ou está inativo.</exception>
    public async Task<SolicitacaoResultDto> CreatePatrimonialAsync(
        CreateSolicitacaoPatrimonialDto dto,
        long pessoaId
    )
    {
        var configuracoes = await _configuracaoService.GetConfiguracoesAsync();
        var prazoSubmissao = configuracoes.PrazoSubmissao;
        if (prazoSubmissao.HasValue && DateTime.UtcNow > prazoSubmissao.Value)
        {
            string prazoSubmissaoFormatado = prazoSubmissao.Value.ToString("dd/MM/yyyy 'às' HH:mm");
            throw new InvalidOperationException(
                $"O prazo para a criação de solicitações encerrou em {prazoSubmissaoFormatado}."
            );
        }

        var (servidor, solicitante) = await _usuarioService.GetSolicitanteInfoAsync(pessoaId);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var novaSolicitacao = new SolicitacaoPatrimonial
            {
                SolicitanteId = solicitante.Id,
                DataCriacao = DateTime.UtcNow,
                StatusId = StatusConsts.Pendente,
            };

            var itensDaSolicitacao = new List<SolicitacaoItem>();

            foreach (var itemDto in dto.Itens)
            {
                var itemDoCatalogo = await _context.Items.FindAsync(itemDto.ItemId);
                if (itemDoCatalogo == null || !itemDoCatalogo.IsActive)
                {
                    throw new Exception(
                        $"Item com ID {itemDto.ItemId} não existe ou está inativo."
                    );
                }

                var solicitacaoItem = new SolicitacaoItem
                {
                    Solicitacao = novaSolicitacao,
                    Item = itemDoCatalogo,
                    ItemId = itemDto.ItemId,
                    Quantidade = itemDto.Quantidade,
                    ValorUnitario = itemDto.ValorUnitario,
                    Justificativa = itemDto.Justificativa,
                };
                itensDaSolicitacao.Add(solicitacaoItem);
            }

            novaSolicitacao.ItemSolicitacao = itensDaSolicitacao;

            await _context.Solicitacoes.AddAsync(novaSolicitacao);
            await _context.SaveChangesAsync();

            novaSolicitacao.ExternalId = GenerateExternalId(novaSolicitacao);

            var historico = new HistoricoSolicitacao
            {
                SolicitacaoId = novaSolicitacao.Id,
                DataOcorrencia = DateTime.UtcNow,
                PessoaId = pessoaId,
                Acao = AcaoHistoricoEnum.Criacao,
                Detalhes = "Solicitação criada.",
            };

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var status = await _context.StatusSolicitacoes.FindAsync(StatusConsts.Pendente);
            if (status == null)
            {
                throw new InvalidOperationException(
                    $"Status com ID {StatusConsts.Pendente} não encontrado."
                );
            }

            var respostaDto = new SolicitacaoResultDto
            {
                Id = novaSolicitacao.Id,
                DataCriacao = novaSolicitacao.DataCriacao,
                ExternalId = novaSolicitacao.ExternalId,
                Status = new StatusSolicitacaoDto
                {
                    Id = status.Id,
                    Nome = status.Nome,
                    Descricao = status.Descricao,
                },
                JustificativaGeral = null,
                Solicitante = new SolicitanteDto
                {
                    Id = solicitante.Id,
                    Nome = servidor.Pessoa.Nome,
                    Email = servidor.Pessoa.Email,
                    Unidade =
                        solicitante.Departamento != null
                            ? new UnidadeOrganizacionalDto
                            {
                                Id = solicitante.Departamento.Id,
                                Nome = solicitante.Departamento.Nome,
                                Sigla = solicitante.Departamento.Sigla,
                                Email = solicitante.Departamento.Email,
                                Telefone = solicitante.Departamento.Telefone,
                                Tipo = "Departamento",
                            }
                            : null,
                },
                Itens = novaSolicitacao
                    .ItemSolicitacao.Select(item => new ItemSolicitacaoResultDto
                    {
                        Id = item.ItemId,
                        Nome = item.Item.Nome,
                        CatMat = item.Item.CatMat,
                        Quantidade = item.Quantidade,
                        LinkImagem = string.IsNullOrWhiteSpace(item.Item.LinkImagem)
                            ? item.Item.LinkImagem
                            : $"{_IMAGE_BASE_URL}{item.Item.LinkImagem}",
                        PrecoSugerido = item.ValorUnitario,
                        Justificativa = item.Justificativa,
                    })
                    .ToList(),
            };

            return respostaDto;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar solicitação geral.");
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Edita uma solicitação existente aplicando validações de prazo, autorização e consistência dos itens.
    /// </summary>
    /// <param name="id">Identificador da solicitação a ser editada.</param>
    /// <param name="pessoaId">Identificador da pessoa que está executando a edição.</param>
    /// <param name="isAdmin">Indica se o usuário possui perfil administrativo e pode editar qualquer solicitação.</param>
    /// <param name="dto">Dados atualizados da solicitação e da lista de itens.</param>
    /// <returns>DTO atualizado da solicitação editada, ou <c>null</c> quando a solicitação não é encontrada.</returns>
    /// <exception cref="InvalidOperationException">Lançada quando o prazo de submissão está encerrado para o status atual da solicitação.</exception>
    /// <exception cref="UnauthorizedAccessException">Lançada quando um usuário sem perfil administrativo tenta editar solicitação de outro solicitante.</exception>
    /// <exception cref="Exception">Lançada quando algum item incluído não existe ou está inativo.</exception>
    public async Task<SolicitacaoResultDto?> EditarSolicitacaoAsync(
        long id,
        long pessoaId,
        bool isAdmin,
        UpdateSolicitacaoDto dto
    )
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var configuracoes = await _configuracaoService.GetConfiguracoesAsync();
            var prazoSubmissao = configuracoes.PrazoSubmissao;

            var solicitacaoDoBanco = await _context
                .Solicitacoes.Include(s => s.ItemSolicitacao)
                .ThenInclude(si => si.Item)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitacaoDoBanco == null)
                return null;

            if (
                prazoSubmissao.HasValue
                && DateTime.UtcNow > prazoSubmissao.Value
                && solicitacaoDoBanco.StatusId != StatusConsts.AguardandoAjustes
            )
            {
                string prazoSubmissaoFormatado = prazoSubmissao.Value.ToString(
                    "dd/MM/yyyy 'às' HH:mm"
                );
                throw new InvalidOperationException(
                    $"O prazo para a criação de solicitações encerrou em {prazoSubmissaoFormatado}."
                );
            }

            if (!isAdmin)
            {
                var (servidor, solicitante) = await _usuarioService.GetSolicitanteInfoAsync(
                    pessoaId
                );
                if (solicitacaoDoBanco.SolicitanteId != solicitante.Id)
                {
                    throw new UnauthorizedAccessException(
                        "Você não tem permissão para editar esta solicitação."
                    );
                }
            }

            var detalhesDaEdicao = new List<string>();
            var itensAntigosParaAuditoria = solicitacaoDoBanco.ItemSolicitacao.ToList();
            var todosOsItemIds = itensAntigosParaAuditoria
                .Select(i => i.ItemId)
                .Union(dto.Itens.Select(i => i.ItemId))
                .Distinct()
                .ToList();
            var catalogoItens = await _context
                .Items.AsNoTracking()
                .Where(i => todosOsItemIds.Contains(i.Id))
                .ToDictionaryAsync(i => i.Id);
            var detalhesItens = AuditHelper.CompareSolicitacaoItens(
                itensAntigosParaAuditoria,
                dto.Itens,
                catalogoItens
            );
            detalhesDaEdicao.AddRange(detalhesItens);

            if (
                solicitacaoDoBanco is SolicitacaoGeral sg
                && sg.JustificativaGeral != dto.JustificativaGeral
                && dto.JustificativaGeral != null
            )
            {
                detalhesDaEdicao.Add(
                    $"Justificativa geral foi alterada de '{sg.JustificativaGeral ?? "vazio"}' para '{dto.JustificativaGeral}'."
                );
            }

            if (
                solicitacaoDoBanco is SolicitacaoGeral solicitacaoGeral
                && dto.JustificativaGeral != null
            )
            {
                solicitacaoGeral.JustificativaGeral = dto.JustificativaGeral;
            }

            var idsDosItensRecebidos = dto.Itens.Select(i => i.ItemId).ToHashSet();
            var itensAtuaisNoBanco = solicitacaoDoBanco.ItemSolicitacao.ToList();

            var itensParaRemover = itensAtuaisNoBanco
                .Where(i => !idsDosItensRecebidos.Contains(i.ItemId))
                .ToList();
            _context.SolicitacaoItens.RemoveRange(itensParaRemover);

            foreach (var itemDto in dto.Itens)
            {
                var itemExistente = itensAtuaisNoBanco.FirstOrDefault(i =>
                    i.ItemId == itemDto.ItemId
                );

                if (itemExistente != null)
                {
                    itemExistente.Quantidade = itemDto.Quantidade;
                    itemExistente.ValorUnitario = itemDto.ValorUnitario;
                    if (solicitacaoDoBanco is SolicitacaoPatrimonial)
                    {
                        itemExistente.Justificativa = itemDto.Justificativa;
                    }
                }
                else
                {
                    var itemDoCatalogo = await _context.Items.FindAsync(itemDto.ItemId);
                    if (itemDoCatalogo == null || !itemDoCatalogo.IsActive)
                        throw new Exception(
                            $"Item com ID {itemDto.ItemId} não existe ou está inativo."
                        );

                    solicitacaoDoBanco.ItemSolicitacao.Add(
                        new SolicitacaoItem
                        {
                            Solicitacao = solicitacaoDoBanco,
                            Item = itemDoCatalogo,
                            ItemId = itemDto.ItemId,
                            Quantidade = itemDto.Quantidade,
                            ValorUnitario = itemDto.ValorUnitario,
                            Justificativa =
                                (solicitacaoDoBanco is SolicitacaoPatrimonial)
                                    ? itemDto.Justificativa
                                    : null,
                        }
                    );
                }
            }

            if (detalhesDaEdicao.Any())
            {
                var historico = new HistoricoSolicitacao
                {
                    SolicitacaoId = solicitacaoDoBanco.Id,
                    DataOcorrencia = DateTime.UtcNow,
                    PessoaId = pessoaId,
                    Acao = AcaoHistoricoEnum.Edicao,
                    Detalhes = string.Join(" | ", detalhesDaEdicao),
                    Observacoes = null,
                };
                await _context.HistoricoSolicitacoes.AddAsync(historico);

                // Quando o solicitante envia ajustes, a solicitação volta para a fila do gestor.
                if (!isAdmin && solicitacaoDoBanco.StatusId == StatusConsts.AguardandoAjustes)
                {
                    solicitacaoDoBanco.StatusId = StatusConsts.Pendente;

                    var historicoStatus = new HistoricoSolicitacao
                    {
                        SolicitacaoId = solicitacaoDoBanco.Id,
                        DataOcorrencia = DateTime.UtcNow,
                        PessoaId = pessoaId,
                        Acao = AcaoHistoricoEnum.MudancaDeStatus,
                        Detalhes =
                            "Status alterado automaticamente de 'Aguardando Ajustes' para 'Pendente' após edição do solicitante.",
                        Observacoes =
                            "Ajustes enviados pelo solicitante para nova análise do gestor.",
                    };
                    await _context.HistoricoSolicitacoes.AddAsync(historicoStatus);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Erro ao editar solicitação com ID {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Obtém uma solicitação pelo identificador e projeta os dados completos, incluindo itens e indicadores agregados.
    /// </summary>
    /// <param name="id">Identificador da solicitação.</param>
    /// <returns>DTO detalhado da solicitação, ou <c>null</c> quando não encontrada.</returns>
    public async Task<SolicitacaoResultDto?> GetByIdAsync(long id)
    {
        _logger.LogInformation("Buscando solicitação com ID: {Id}", id);

        var solicitacao = await _context
            .Solicitacoes.AsNoTracking()
            .Include(s => s.Solicitante)
            .ThenInclude(sol => sol.Servidor)
            .ThenInclude(serv => serv.Pessoa)
            .Include(s => s.Solicitante)
            .ThenInclude(sol => sol.Departamento)
            .Include(s => s.ItemSolicitacao)
            .ThenInclude(si => si.Item)
            .ThenInclude(i => i.Categoria)
            .Include(s => s.Status)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitacao == null)
        {
            _logger.LogWarning("Solicitação com ID: {Id} não encontrada.", id);
            return null;
        }

        var itens = solicitacao.ItemSolicitacao;

        var kpis = new SolicitacaoKpiDto
        {
            ValorTotalEstimado = itens.Sum(i => i.Quantidade * i.ValorUnitario),
            TotalItensUnicos = itens.Select(i => i.ItemId).Distinct().Count(),
            TotalUnidades = itens.Sum(i => i.Quantidade),
        };

        var valorPorCategoria = itens
            .GroupBy(i => i.Item.Categoria.Nome)
            .Select(g => new { Label = g.Key, Valor = g.Sum(i => i.Quantidade * i.ValorUnitario) })
            .ToList();

        var topItens = itens
            .Select(i => new
            {
                i.ItemId,
                i.Item.Nome,
                i.Item.CatMat,
                ValorTotal = i.Quantidade * i.ValorUnitario,
            })
            .OrderByDescending(x => x.ValorTotal)
            .Take(5) // Top 5 itens mais caros da solicitação
            .Select(x => new DashTopItemDto
            {
                ItemId = x.ItemId,
                Nome = x.Nome,
                CatMat = x.CatMat,
                Valor = x.ValorTotal,
            })
            .ToList();

        var respostaDto = new SolicitacaoResultDto
        {
            Id = solicitacao.Id,
            DataCriacao = solicitacao.DataCriacao,
            JustificativaGeral =
                (solicitacao is SolicitacaoGeral solicitacaoGeral)
                    ? solicitacaoGeral.JustificativaGeral
                    : null,
            ExternalId = solicitacao.ExternalId,
            Status = new StatusSolicitacaoDto
            {
                Id = solicitacao.Status.Id,
                Nome = solicitacao.Status.Nome,
                Descricao = solicitacao.Status.Descricao,
            },
            Solicitante = new SolicitanteDto
            {
                Id = solicitacao.Solicitante.Id,
                Nome = solicitacao.Solicitante.Servidor.Pessoa.Nome,
                Email = solicitacao.Solicitante.Servidor.Pessoa.Email,
                Unidade =
                    solicitacao.Solicitante.Departamento != null
                        ? new UnidadeOrganizacionalDto
                        {
                            Id = solicitacao.Solicitante.Departamento.Id,
                            Nome = solicitacao.Solicitante.Departamento.Nome,
                            Sigla = solicitacao.Solicitante.Departamento.Sigla,
                            Email = solicitacao.Solicitante.Departamento.Email,
                            Telefone = solicitacao.Solicitante.Departamento.Telefone,
                            Tipo = "Departamento",
                        }
                        : null,
            },
            Itens = solicitacao
                .ItemSolicitacao.Select(item => new ItemSolicitacaoResultDto
                {
                    Id = item.ItemId,
                    Nome = item.Item.Nome,
                    CatMat = item.Item.CatMat,
                    Quantidade = item.Quantidade,
                    LinkImagem = string.IsNullOrWhiteSpace(item.Item.LinkImagem)
                        ? item.Item.LinkImagem
                        : $"{_IMAGE_BASE_URL}{item.Item.LinkImagem}",
                    PrecoSugerido = item.ValorUnitario,
                    Justificativa = item.Justificativa,
                })
                .ToList(),
            Kpis = kpis,
            ValorPorCategoria = new ChartDataDto
            {
                Labels = valorPorCategoria.Select(x => x.Label).ToList(),
                Data = valorPorCategoria.Select(x => x.Valor).ToList(),
            },
            TopItensPorValor = topItens,
        };

        return respostaDto;
    }

    /// <summary>
    /// Lista solicitações do solicitante autenticado com filtros, ordenação e paginação.
    /// </summary>
    /// <param name="pessoaId">Identificador da pessoa para resolver o solicitante.</param>
    /// <param name="gestorId">Filtro opcional pelo gestor responsável.</param>
    /// <param name="tipo">Filtro opcional pelo tipo da solicitação (GERAL ou PATRIMONIAL).</param>
    /// <param name="dataInicial">Data inicial opcional para o período de criação.</param>
    /// <param name="dataFinal">Data final opcional para o período de criação.</param>
    /// <param name="externalId">Filtro opcional pelo identificador externo da solicitação.</param>
    /// <param name="statusIds">Filtro opcional por lista de status.</param>
    /// <param name="sortOrder">Ordenação por data de criação: asc ou desc.</param>
    /// <param name="pageNumber">Número da página para paginação.</param>
    /// <param name="pageSize">Quantidade de itens por página.</param>
    /// <returns>Resultado paginado contendo as solicitações que atendem aos filtros.</returns>
    public async Task<PaginatedResultDto<SolicitacaoResultDto>> GetAllBySolicitanteAsync(
        long pessoaId,
        long? gestorId,
        string? tipo,
        DateTime? dataInicial,
        DateTime? dataFinal,
        string? externalId,
        List<int>? statusIds,
        string? sortOrder,
        int pageNumber,
        int pageSize
    )
    {
        var (servidor, solicitante) = await _usuarioService.GetSolicitanteInfoAsync(pessoaId);

        _logger.LogInformation("Buscando solicitações para o solicitante ID: {Id}", solicitante.Id);

        var query = _context
            .Solicitacoes.Where(s => s.SolicitanteId == solicitante.Id)
            .Include(s => s.Solicitante)
            .ThenInclude(sol => sol.Servidor)
            .ThenInclude(serv => serv.Pessoa)
            .Include(s => s.Solicitante)
            .ThenInclude(sol => sol.Departamento)
            .Include(s => s.ItemSolicitacao)
            .ThenInclude(si => si.Item)
            .Include(s => s.Status)
            .AsNoTracking();

        if (gestorId.HasValue)
        {
            query = query.Where(s => s.GestorId == gestorId.Value);
        }
        if (!string.IsNullOrWhiteSpace(tipo))
        {
            query = query.Where(s => EF.Property<string>(s, "TipoSolicitacao") == tipo.ToUpper());
        }
        if (dataInicial.HasValue)
        {
            var inicioPeriodo = dataInicial.Value.ToUniversalTime().Date;
            var fimPeriodo = (dataFinal ?? dataInicial).Value.ToUniversalTime().Date.AddDays(1);
            query = query.Where(s => s.DataCriacao >= inicioPeriodo && s.DataCriacao < fimPeriodo);
        }
        else if (dataFinal.HasValue)
        {
            var fimPeriodo = dataFinal.Value.ToUniversalTime().Date.AddDays(1);
            query = query.Where(s => s.DataCriacao < fimPeriodo);
        }

        if (!string.IsNullOrWhiteSpace(externalId))
        {
            query = query.Where(s => s.ExternalId == externalId);
        }

        if (statusIds != null && statusIds.Any())
        {
            query = query.Where(s => statusIds.Contains(s.StatusId));
        }

        if (sortOrder?.ToLower() == "asc")
        {
            query = query.OrderBy(s => s.DataCriacao);
        }
        else
        {
            query = query.OrderByDescending(s => s.DataCriacao);
        }

        var totalCount = await query.CountAsync();
        var solicitacoesPaginadas = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var listaDeDtos = solicitacoesPaginadas
            .Select(solicitacao => new SolicitacaoResultDto
            {
                Id = solicitacao.Id,
                DataCriacao = solicitacao.DataCriacao,
                JustificativaGeral =
                    (solicitacao is SolicitacaoGeral sg) ? sg.JustificativaGeral : null,
                ExternalId = solicitacao.ExternalId,
                Status = new StatusSolicitacaoDto
                {
                    Id = solicitacao.Status.Id,
                    Nome = solicitacao.Status.Nome,
                    Descricao = solicitacao.Status.Descricao,
                },
                Solicitante = new SolicitanteDto
                {
                    Id = solicitacao.Solicitante.Id,
                    Nome = solicitacao.Solicitante.Servidor.Pessoa.Nome,
                    Email = solicitacao.Solicitante.Servidor.Pessoa.Email,
                    Unidade =
                        solicitacao.Solicitante.Departamento != null
                            ? new UnidadeOrganizacionalDto
                            {
                                Id = solicitacao.Solicitante.Departamento.Id,
                                Nome = solicitacao.Solicitante.Departamento.Nome,
                                Sigla = solicitacao.Solicitante.Departamento.Sigla,
                                Email = solicitacao.Solicitante.Departamento.Email,
                                Telefone = solicitacao.Solicitante.Departamento.Telefone,
                                Tipo = "Departamento",
                            }
                            : null,
                },
                Itens = (
                    (solicitacao is SolicitacaoGeral geral) ? geral.ItemSolicitacao
                    : (solicitacao is SolicitacaoPatrimonial patrimonial)
                        ? patrimonial.ItemSolicitacao
                    : new List<SolicitacaoItem>()
                )
                    .Select(item => new ItemSolicitacaoResultDto
                    {
                        Id = item.ItemId,
                        Nome = item.Item.Nome,
                        CatMat = item.Item.CatMat,
                        Quantidade = item.Quantidade,
                        LinkImagem = string.IsNullOrWhiteSpace(item.Item.LinkImagem)
                            ? item.Item.LinkImagem
                            : $"{_IMAGE_BASE_URL}{item.Item.LinkImagem}",
                        PrecoSugerido = item.ValorUnitario,
                        Justificativa = item.Justificativa,
                    })
                    .ToList(),
            })
            .ToList();

        return new PaginatedResultDto<SolicitacaoResultDto>(
            listaDeDtos,
            totalCount,
            pageNumber,
            pageSize
        );
    }

    private string GenerateExternalId(Solicitacao solicitacao)
    {
        var ano = solicitacao.DataCriacao.ToString("yyyy");
        var idFormatado = solicitacao.Id.ToString("D4");
        string prefixo = solicitacao switch
        {
            SolicitacaoPatrimonial => "SP",
            SolicitacaoGeral => "SG",
            _ => "SO",
        };

        return $"{prefixo}-{ano}-{idFormatado}";
    }

    /// <summary>
    /// Lista solicitações em visão administrativa com filtros, ordenação e paginação.
    /// </summary>
    /// <param name="pessoaId">Filtro opcional por pessoa, resolvendo o solicitante correspondente.</param>
    /// <param name="gestorId">Filtro opcional por gestor responsável.</param>
    /// <param name="tipo">Filtro opcional pelo tipo da solicitação (GERAL ou PATRIMONIAL).</param>
    /// <param name="siglaDepartamento">Filtro opcional pela sigla do departamento do solicitante.</param>
    /// <param name="dataInicial">Data inicial opcional para o período de criação.</param>
    /// <param name="dataFinal">Data final opcional para o período de criação.</param>
    /// <param name="externalId">Filtro opcional pelo identificador externo da solicitação.</param>
    /// <param name="statusIds">Filtro opcional por lista de status.</param>
    /// <param name="sortOrder">Ordenação por data de criação: asc ou desc.</param>
    /// <param name="pageNumber">Número da página para paginação.</param>
    /// <param name="pageSize">Quantidade de itens por página.</param>
    /// <returns>Resultado paginado contendo as solicitações que atendem aos filtros administrativos.</returns>
    public async Task<PaginatedResultDto<SolicitacaoResultDto>> GetAllAsync(
        long? pessoaId,
        long? gestorId,
        string? tipo,
        string? siglaDepartamento,
        DateTime? dataInicial,
        DateTime? dataFinal,
        string? externalId,
        List<int>? statusIds,
        string? sortOrder,
        int pageNumber,
        int pageSize
    )
    {
        _logger.LogInformation("Buscando todas as solicitações com filtros administrativos.");

        var query = _context
            .Solicitacoes.Include(s => s.Solicitante)
            .ThenInclude(sol => sol.Servidor)
            .ThenInclude(serv => serv.Pessoa)
            .Include(s => s.Solicitante)
            .ThenInclude(sol => sol.Departamento)
            .Include(s => s.ItemSolicitacao)
            .ThenInclude(si => si.Item)
            .Include(s => s.Status)
            .AsNoTracking();

        if (pessoaId.HasValue)
        {
            var (servidor, solicitante) = await _usuarioService.GetSolicitanteInfoAsync(
                (long)pessoaId
            );
            query = query.Where(s => s.SolicitanteId == solicitante.Id);
        }

        if (gestorId.HasValue)
        {
            query = query.Where(s => s.GestorId == gestorId.Value);
        }
        if (!string.IsNullOrWhiteSpace(tipo))
        {
            query = query.Where(s => EF.Property<string>(s, "TipoSolicitacao") == tipo.ToUpper());
        }
        if (!string.IsNullOrWhiteSpace(siglaDepartamento))
        {
            query = query.Where(s =>
                s.Solicitante.Departamento.Sigla.ToUpper() == siglaDepartamento.ToUpper()
            );
        }
        if (dataInicial.HasValue)
        {
            var inicioPeriodo = dataInicial.Value.ToUniversalTime().Date;
            var fimPeriodo = (dataFinal ?? dataInicial).Value.ToUniversalTime().Date.AddDays(1);
            query = query.Where(s => s.DataCriacao >= inicioPeriodo && s.DataCriacao < fimPeriodo);
        }
        else if (dataFinal.HasValue)
        {
            var fimPeriodo = dataFinal.Value.ToUniversalTime().Date.AddDays(1);
            query = query.Where(s => s.DataCriacao < fimPeriodo);
        }
        if (!string.IsNullOrWhiteSpace(externalId))
        {
            query = query.Where(s => s.ExternalId == externalId);
        }

        if (statusIds != null && statusIds.Any())
        {
            query = query.Where(s => statusIds.Contains(s.StatusId));
        }

        if (sortOrder?.ToLower() == "asc")
        {
            query = query.OrderBy(s => s.DataCriacao);
        }
        else
        {
            query = query.OrderByDescending(s => s.DataCriacao);
        }

        var totalCount = await query.CountAsync();
        var solicitacoesPaginadas = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var listaDeDtos = solicitacoesPaginadas
            .Select(solicitacao => new SolicitacaoResultDto
            {
                Id = solicitacao.Id,
                DataCriacao = solicitacao.DataCriacao,
                JustificativaGeral =
                    (solicitacao is SolicitacaoGeral sg) ? sg.JustificativaGeral : null,
                ExternalId = solicitacao.ExternalId,
                Status = new StatusSolicitacaoDto
                {
                    Id = solicitacao.Status.Id,
                    Nome = solicitacao.Status.Nome,
                    Descricao = solicitacao.Status.Descricao,
                },
                Solicitante = new SolicitanteDto
                {
                    Id = solicitacao.Solicitante.Id,
                    Nome = solicitacao.Solicitante.Servidor.Pessoa.Nome,
                    Email = solicitacao.Solicitante.Servidor.Pessoa.Email,
                    Unidade =
                        solicitacao.Solicitante.Departamento != null
                            ? new UnidadeOrganizacionalDto
                            {
                                Id = solicitacao.Solicitante.Departamento.Id,
                                Nome = solicitacao.Solicitante.Departamento.Nome,
                                Sigla = solicitacao.Solicitante.Departamento.Sigla,
                                Email = solicitacao.Solicitante.Departamento.Email,
                                Telefone = solicitacao.Solicitante.Departamento.Telefone,
                                Tipo = "Departamento",
                            }
                            : null,
                },
                Itens = (
                    (solicitacao is SolicitacaoGeral geral) ? geral.ItemSolicitacao
                    : (solicitacao is SolicitacaoPatrimonial patrimonial)
                        ? patrimonial.ItemSolicitacao
                    : new List<SolicitacaoItem>()
                )
                    .Select(item => new ItemSolicitacaoResultDto
                    {
                        Id = item.ItemId,
                        Nome = item.Item.Nome,
                        CatMat = item.Item.CatMat,
                        Quantidade = item.Quantidade,
                        LinkImagem = string.IsNullOrWhiteSpace(item.Item.LinkImagem)
                            ? item.Item.LinkImagem
                            : $"{_IMAGE_BASE_URL}{item.Item.LinkImagem}",
                        PrecoSugerido = item.ValorUnitario,
                        Justificativa = item.Justificativa,
                    })
                    .ToList(),
            })
            .ToList();

        return new PaginatedResultDto<SolicitacaoResultDto>(
            listaDeDtos,
            totalCount,
            pageNumber,
            pageSize
        );
    }

    /// <summary>
    /// Cancela uma solicitação existente com validação de autorização e de status finalizado.
    /// </summary>
    /// <param name="id">Identificador da solicitação a ser cancelada.</param>
    /// <param name="pessoaId">Identificador da pessoa que está solicitando o cancelamento.</param>
    /// <param name="isAdmin">Indica se o usuário possui perfil administrativo e pode cancelar qualquer solicitação.</param>
    /// <param name="dto">Dados complementares do cancelamento, incluindo observações.</param>
    /// <returns>DTO da solicitação após cancelamento, ou <c>null</c> quando não encontrada.</returns>
    /// <exception cref="UnauthorizedAccessException">Lançada quando um usuário sem perfil administrativo tenta cancelar solicitação de outro solicitante.</exception>
    /// <exception cref="InvalidOperationException">Lançada quando a solicitação já está finalizada ou já foi cancelada.</exception>
    public async Task<SolicitacaoResultDto?> CancelarSolicitacaoAsync(
        long id,
        long pessoaId,
        bool isAdmin,
        CancelarSolicitacaoDto dto
    )
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var solicitacao = await _context.Solicitacoes.FindAsync(id);
            if (solicitacao == null)
                return null;

            if (!isAdmin)
            {
                var (servidor, solicitante) = await _usuarioService.GetSolicitanteInfoAsync(
                    pessoaId
                );
                if (solicitacao.SolicitanteId != solicitante.Id)
                {
                    throw new UnauthorizedAccessException(
                        "Você não tem permissão para cancelar esta solicitação."
                    );
                }
            }

            if (
                solicitacao.StatusId == StatusConsts.Aprovada
                || solicitacao.StatusId == StatusConsts.Rejeitada
                || solicitacao.StatusId == StatusConsts.Encerrada
            )
            {
                throw new InvalidOperationException(
                    "Não é possível cancelar uma solicitação que já foi finalizada."
                );
            }

            if (solicitacao.StatusId == StatusConsts.Cancelada)
            {
                throw new InvalidOperationException("A solicitação já se encontra CANCELADA.");
            }

            solicitacao.StatusId = StatusConsts.Cancelada;

            var historico = new HistoricoSolicitacao
            {
                SolicitacaoId = solicitacao.Id,
                DataOcorrencia = DateTime.UtcNow,
                PessoaId = pessoaId,
                Acao = AcaoHistoricoEnum.Cancelamento,
                Detalhes = "Solicitação cancelada.",
                Observacoes = dto.Observacoes,
            };

            await _context.HistoricoSolicitacoes.AddAsync(historico);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetByIdAsync(id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Erro ao cancelar solicitação com ID {Id}", id);
            throw;
        }
    }

    /// <summary>
    /// Atualiza o status de uma solicitação, registrando histórico da transição.
    /// </summary>
    /// <param name="id">Identificador da solicitação.</param>
    /// <param name="pessoaId">Identificador da pessoa responsável pela mudança de status.</param>
    /// <param name="dto">Dados da alteração contendo o novo status e observações opcionais.</param>
    /// <returns>DTO atualizado da solicitação, ou <c>null</c> quando a solicitação não é encontrada.</returns>
    /// <exception cref="InvalidOperationException">Lançada quando a solicitação já está em status terminal (Cancelada ou Encerrada).</exception>
    public async Task<SolicitacaoResultDto?> AtualizarStatusSolicitacaoAsync(
        long id,
        long pessoaId,
        UpdateStatusSolicitacaoDto dto
    )
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var solicitacao = await _context.Solicitacoes.FindAsync(id);
            if (solicitacao == null)
            {
                _logger.LogWarning(
                    "Tentativa de mudar status de solicitação inexistente. ID: {Id}",
                    id
                );
                return null;
            }

            var statusAnteriorId = solicitacao.StatusId;
            var statusAnteriorNome = (
                await _context.StatusSolicitacoes.FindAsync(statusAnteriorId)
            )?.Nome;

            if (
                statusAnteriorId == StatusConsts.Cancelada
                || statusAnteriorId == StatusConsts.Encerrada
            )
            {
                throw new InvalidOperationException(
                    $"Não é possível alterar o status de uma solicitação que já foi {statusAnteriorNome}."
                );
            }

            solicitacao.StatusId = dto.NovoStatusId;

            var novoStatusNome = (
                await _context.StatusSolicitacoes.FindAsync(dto.NovoStatusId)
            )?.Nome;

            var historico = new HistoricoSolicitacao
            {
                SolicitacaoId = solicitacao.Id,
                DataOcorrencia = DateTime.UtcNow,
                PessoaId = pessoaId,
                Acao = AcaoHistoricoEnum.MudancaDeStatus,
                Detalhes =
                    $"O status da solicitação foi alterado de '{statusAnteriorNome}' para '{novoStatusNome}'",
                Observacoes = string.IsNullOrWhiteSpace(dto.Observacoes) ? null : dto.Observacoes,
            };

            await _context.HistoricoSolicitacoes.AddAsync(historico);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Status da Solicitação ID {Id} alterado de {StatusAnterior} para {StatusNovo} pelo usuário ID {UsuarioId}",
                id,
                statusAnteriorId,
                dto.NovoStatusId,
                pessoaId
            );

            return await GetByIdAsync(id);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    /// <summary>
    /// Retorna o histórico de uma solicitação respeitando as regras de autorização do usuário autenticado.
    /// </summary>
    /// <param name="solicitacaoId">Identificador da solicitação para consulta do histórico.</param>
    /// <param name="user">Usuário autenticado usado para validar permissões de acesso.</param>
    /// <returns>Lista de eventos do histórico em ordem decrescente de ocorrência, ou <c>null</c> quando a solicitação não existe ou o acesso não é permitido.</returns>
    /// <exception cref="FormatException">Lançada quando o identificador da pessoa no token não pode ser convertido para número.</exception>
    public async Task<List<HistoricoSolicitacaoDto>?> GetHistoricoAsync(
        long solicitacaoId,
        ClaimsPrincipal user
    )
    {
        var pessoaId = long.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        bool isSolicitante = user.IsInRole("Solicitante");

        var solicitacao = await _context
            .Solicitacoes.AsNoTracking()
            .Include(s => s.Solicitante)
            .ThenInclude(sol => sol.Servidor)
            .ThenInclude(serv => serv.Pessoa)
            .FirstOrDefaultAsync(s => s.Id == solicitacaoId);

        if (solicitacao == null)
        {
            _logger.LogWarning("Solicitação com ID: {Id} não encontrada.", solicitacaoId);
            return null;
        }

        if (isSolicitante && solicitacao.Solicitante.Servidor.Pessoa.Id != pessoaId)
        {
            _logger.LogWarning("Tentativa de acesso não autorizada.");
            return null;
        }

        var historico = await _context
            .HistoricoSolicitacoes.AsNoTracking()
            .Where(h => h.SolicitacaoId == solicitacaoId)
            .Include(h => h.Pessoa)
            .OrderByDescending(h => h.DataOcorrencia)
            .Select(h => new HistoricoSolicitacaoDto
            {
                Id = h.Id,
                DataOcorrencia = h.DataOcorrencia,
                Acao = h.Acao.ToString(),
                Detalhes = h.Detalhes,
                Observacoes = h.Observacoes,
                NomePessoa = h.Pessoa.Nome,
            })
            .ToListAsync();

        return historico;
    }

    public async Task<int> ArchiveOldSolicitationsAsync(int anoReferencia, long executorPessoaId)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var toArchive = await _context
                .Solicitacoes.Where(s =>
                    s.DataCriacao.Year < anoReferencia && s.StatusId != StatusConsts.Encerrada
                )
                .ToListAsync();

            if (!toArchive.Any())
            {
                _logger.LogInformation(
                    "Nenhuma solicitação encontrada para arquivamento referente ao ano {Ano}",
                    anoReferencia
                );
                return 0;
            }

            foreach (var s in toArchive)
            {
                var statusAnterior = s.StatusId;
                s.StatusId = StatusConsts.Encerrada;

                var historico = new HistoricoSolicitacao
                {
                    SolicitacaoId = s.Id,
                    DataOcorrencia = DateTime.UtcNow,
                    PessoaId = executorPessoaId,
                    Acao = AcaoHistoricoEnum.MudancaDeStatus,
                    Detalhes =
                        $"Rotina de arquivamento anual: status alterado de {statusAnterior} para {StatusConsts.Encerrada}",
                    Observacoes = "Encerrada automaticamente por rotina anual",
                };

                await _context.HistoricoSolicitacoes.AddAsync(historico);
            }

            var changed = await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Arquivadas {Count} solicitações anteriores a {Ano}",
                toArchive.Count,
                anoReferencia
            );

            return toArchive.Count;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Erro ao arquivar solicitações antigas");
            throw;
        }
    }
}
