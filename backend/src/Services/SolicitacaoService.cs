using ComprasTccApp.Backend.Extensions;
using ComprasTccApp.Models.Entities.Itens;
using ComprasTccApp.Models.Entities.Solicitacoes;
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
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
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

    public async Task<Solicitacao?> GetByIdAsync(long id)
    {
        _logger.LogInformation("Buscando solicitação com ID: {Id}", id);
        var solicitacao = await _context
            .Solicitacoes.Include("ItemSolicitacao.Item")
            .FirstOrDefaultAsync(s => s.Id == id);

        if (solicitacao == null)
        {
            _logger.LogWarning("Solicitação com ID: {Id} não encontrada.", id);
        }

        return solicitacao;
    }

    public async Task<SolicitacaoPatrimonial> CreatePatrimonialAsync(
        CreateSolicitacaoPatrimonialDto dto,
        long solicitanteId
    )
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var solicitante = await _context.Solicitantes.FindAsync(solicitanteId);
            // var gestor = await _context.Gestores.FindAsync(dto.GestorId);

            if (solicitante == null)
            {
                throw new Exception("Solicitante ou Gestor não encontrado.");
            }

            var novaSolicitacao = new SolicitacaoPatrimonial
            {
                SolicitanteId = solicitanteId,
                Solicitante = solicitante,
                DataCriacao = DateTime.UtcNow,
            };

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
                    ValorUnitario = itemDoCatalogo.PrecoSugerido,
                    Justificativa = itemDto.Justificativa,
                };
                novaSolicitacao.ItemSolicitacao.Add(solicitacaoItem);
            }

            await _context.SolicitacoesPatrimoniais.AddAsync(novaSolicitacao);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return novaSolicitacao;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar solicitação patrimonial.");
            await transaction.RollbackAsync();
            throw;
        }
    }
}
