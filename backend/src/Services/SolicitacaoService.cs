using ComprasTccApp.Backend.Extensions;
using ComprasTccApp.Models.Entities.Itens;
using ComprasTccApp.Models.Entities.Servidores;
using ComprasTccApp.Models.Entities.Solicitacoes;
using ComprasTccApp.Models.Entities.Solicitantes;
using Database;
using Microsoft.EntityFrameworkCore;
using Models.Dtos;

public class SolicitacaoService : ISolicitacaoService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SolicitacaoService> _logger;

    public SolicitacaoService(AppDbContext context, ILogger<SolicitacaoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SolicitacaoResultDto> CreateGeralAsync(
        CreateSolicitacaoGeralDto dto,
        long pessoaId
    )
    {
        var (servidor, solicitante) = await GetSolicitanteInfoAsync(pessoaId);

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

            await transaction.CommitAsync();

            var respostaDto = new SolicitacaoResultDto
            {
                Id = novaSolicitacao.Id,
                DataCriacao = novaSolicitacao.DataCriacao,
                JustificativaGeral = novaSolicitacao.JustificativaGeral,
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
                        ItemId = item.ItemId,
                        NomeDoItem = item.Item.Nome,
                        CatMat = item.Item.CatMat,
                        Quantidade = item.Quantidade,
                        ValorUnitario = item.ValorUnitario,
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
        var (servidor, solicitante) = await GetSolicitanteInfoAsync(pessoaId);

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

            await transaction.CommitAsync();

            var respostaDto = new SolicitacaoResultDto
            {
                Id = novaSolicitacao.Id,
                DataCriacao = novaSolicitacao.DataCriacao,
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
                        ItemId = item.ItemId,
                        NomeDoItem = item.Item.Nome,
                        CatMat = item.Item.CatMat,
                        Quantidade = item.Quantidade,
                        ValorUnitario = item.ValorUnitario,
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

    private async Task<(Servidor servidor, Solicitante solicitante)> GetSolicitanteInfoAsync(
        long pessoaId
    )
    {
        var servidor = await _context
            .Servidores.Include(s => s.Pessoa)
            .FirstOrDefaultAsync(s => s.PessoaId == pessoaId);

        if (servidor == null)
            throw new Exception($"Pessoa com ID {pessoaId} não encontrada na tabela Servidor.");

        var solicitante = await _context.Solicitantes.FirstOrDefaultAsync(s =>
            s.ServidorId == servidor.Id
        );

        if (solicitante == null)
            throw new Exception(
                $"Servidor com ID {servidor.Id} não encontrado na tabela Solicitante."
            );

        return (servidor, solicitante);
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
                    ItemId = item.ItemId,
                    NomeDoItem = item.Item.Nome,
                    CatMat = item.Item.CatMat,
                    Quantidade = item.Quantidade,
                    ValorUnitario = item.ValorUnitario,
                    Justificativa = item.Justificativa,
                })
                .ToList(),
        };

        return respostaDto;
    }
}
