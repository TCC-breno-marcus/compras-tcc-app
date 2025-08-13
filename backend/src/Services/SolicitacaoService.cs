using ComprasTccApp.Models.Entities.Itens;
using ComprasTccApp.Models.Entities.Solicitacoes;
using Database;
using Microsoft.EntityFrameworkCore;

public class SolicitacaoService : ISolicitacaoService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SolicitacaoService> _logger;

    public SolicitacaoService(AppDbContext context, ILogger<SolicitacaoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SolicitacaoGeral> CreateGeralAsync(
        CreateSolicitacaoGeralDto dto,
        long solicitanteId
    )
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var solicitante = await _context.Solicitantes.FindAsync(solicitanteId);
            // var gestor = await _context.Gestores.FindAsync(dto.GestorId);

            if (solicitante == null)
                throw new Exception($"Solicitante com ID {solicitanteId} não encontrado.");
            // if (gestor == null)
            //     throw new Exception($"Gestor com ID {dto.GestorId} não encontrado.");

            var novaSolicitacao = new SolicitacaoGeral
            {
                SolicitanteId = solicitanteId,
                // GestorId = dto.GestorId,
                Solicitante = solicitante,
                // Gestor = gestor,
                DataCriacao = DateTime.UtcNow,
                JustificativaGeral = dto.JustificativaGeral,
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
                    Justificativa = null,
                };
                novaSolicitacao.ItemSolicitacao.Add(solicitacaoItem);
            }

            await _context.SolicitacoesGerais.AddAsync(novaSolicitacao);
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
            return novaSolicitacao;
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
                // GestorId = dto.GestorId,
                Solicitante = solicitante,
                // Gestor = gestor,
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
