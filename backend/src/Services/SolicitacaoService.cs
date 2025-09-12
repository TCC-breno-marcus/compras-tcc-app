using ComprasTccApp.Backend.Enums;
using ComprasTccApp.Backend.Extensions;
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
    private readonly string _imageBaseUrl;

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
        _imageBaseUrl = configuration["ImageBaseUrl"] ?? "";
    }

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

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var respostaDto = new SolicitacaoResultDto
            {
                Id = novaSolicitacao.Id,
                DataCriacao = novaSolicitacao.DataCriacao,
                JustificativaGeral = novaSolicitacao.JustificativaGeral,
                ExternalId = novaSolicitacao.ExternalId,
                Solicitante = new SolicitanteDto
                {
                    Id = solicitante.Id,
                    Nome = servidor.Pessoa.Nome,
                    Email = servidor.Pessoa.Email,
                    Departamento = solicitante.Unidade.ToFriendlyString(),
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
                            : $"{_imageBaseUrl}{item.Item.LinkImagem}",
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

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            var respostaDto = new SolicitacaoResultDto
            {
                Id = novaSolicitacao.Id,
                DataCriacao = novaSolicitacao.DataCriacao,
                ExternalId = novaSolicitacao.ExternalId,
                JustificativaGeral = null,
                Solicitante = new SolicitanteDto
                {
                    Id = solicitante.Id,
                    Nome = servidor.Pessoa.Nome,
                    Email = servidor.Pessoa.Email,
                    Departamento = solicitante.Unidade.ToFriendlyString(),
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
                            : $"{_imageBaseUrl}{item.Item.LinkImagem}",
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
            if (prazoSubmissao.HasValue && DateTime.UtcNow > prazoSubmissao.Value)
            {
                string prazoSubmissaoFormatado = prazoSubmissao.Value.ToString(
                    "dd/MM/yyyy 'às' HH:mm"
                );
                throw new InvalidOperationException(
                    $"O prazo para a criação de solicitações encerrou em {prazoSubmissaoFormatado}."
                );
            }

            var solicitacaoDoBanco = await _context
                .Solicitacoes.Include(s => s.ItemSolicitacao)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitacaoDoBanco == null)
                return null;

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

    public async Task<SolicitacaoResultDto?> GetByIdAsync(long id)
    {
        _logger.LogInformation("Buscando solicitação com ID: {Id}", id);

        var solicitacao = await _context
            .Solicitacoes.Include(s => s.Solicitante)
            .ThenInclude(sol => sol.Servidor)
            .ThenInclude(serv => serv.Pessoa)
            .Include(s => s.ItemSolicitacao)
            .ThenInclude(si => si.Item)
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitacao == null)
        {
            _logger.LogWarning("Solicitação com ID: {Id} não encontrada.", id);
            return null;
        }

        var respostaDto = new SolicitacaoResultDto
        {
            Id = solicitacao.Id,
            DataCriacao = solicitacao.DataCriacao,
            JustificativaGeral =
                (solicitacao is SolicitacaoGeral solicitacaoGeral)
                    ? solicitacaoGeral.JustificativaGeral
                    : null,
            ExternalId = solicitacao.ExternalId,
            Solicitante = new SolicitanteDto
            {
                Id = solicitacao.Solicitante.Id,
                Nome = solicitacao.Solicitante.Servidor.Pessoa.Nome,
                Email = solicitacao.Solicitante.Servidor.Pessoa.Email,
                Departamento = solicitacao.Solicitante.Unidade.ToFriendlyString(),
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
                        : $"{_imageBaseUrl}{item.Item.LinkImagem}",
                    PrecoSugerido = item.ValorUnitario,
                    Justificativa = item.Justificativa,
                })
                .ToList(),
        };

        return respostaDto;
    }

    public async Task<PaginatedResultDto<SolicitacaoResultDto>> GetAllBySolicitanteAsync(
        long pessoaId,
        long? gestorId,
        string? tipo,
        DateTime? dataInicial,
        DateTime? dataFinal,
        string? externalId,
        string? sortOrder,
        int pageNumber,
        int pageSize
    )
    {
        var (servidor, solicitante) = await _usuarioService.GetSolicitanteInfoAsync(pessoaId);

        _logger.LogInformation("Buscando solicitações para o solicitante ID: {Id}", solicitante.Id);

        var query = _context
            .Solicitacoes.Where(s => s.SolicitanteId == solicitante.Id)
            .Include(s => s.Solicitante.Servidor.Pessoa)
            .Include("ItemSolicitacao.Item")
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
                Solicitante = new SolicitanteDto
                {
                    Id = solicitacao.Solicitante.Id,
                    Nome = solicitacao.Solicitante.Servidor.Pessoa.Nome,
                    Email = solicitacao.Solicitante.Servidor.Pessoa.Email,
                    Departamento = solicitacao.Solicitante.Unidade.ToFriendlyString(),
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
                            : $"{_imageBaseUrl}{item.Item.LinkImagem}",
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

    public async Task<PaginatedResultDto<SolicitacaoResultDto>> GetAllAsync(
        long? pessoaId,
        long? gestorId,
        string? tipo,
        string? unidade,
        DateTime? dataInicial,
        DateTime? dataFinal,
        string? externalId,
        string? sortOrder,
        int pageNumber,
        int pageSize
    )
    {
        _logger.LogInformation("Buscando todas as solicitações com filtros administrativos.");

        var query = _context
            .Solicitacoes.Include(s => s.Solicitante.Servidor.Pessoa)
            .Include("ItemSolicitacao.Item")
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
        if (!string.IsNullOrEmpty(unidade))
        {
            DepartamentoEnum? departamentoEnum = unidade.FromString<DepartamentoEnum>();
            if (departamentoEnum.HasValue)
            {
                query = query.Where(s => s.Solicitante.Unidade == departamentoEnum.Value);
            }
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
                Solicitante = new SolicitanteDto
                {
                    Id = solicitacao.Solicitante.Id,
                    Nome = solicitacao.Solicitante.Servidor.Pessoa.Nome,
                    Email = solicitacao.Solicitante.Servidor.Pessoa.Email,
                    Departamento = solicitacao.Solicitante.Unidade.ToFriendlyString(),
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
                            : $"{_imageBaseUrl}{item.Item.LinkImagem}",
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
}
