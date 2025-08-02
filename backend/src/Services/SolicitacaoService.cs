// Local: Services/SolicitacaoService.cs
using ComprasTccApp.Models.Entities.Itens;
using Database;

public class SolicitacaoService : ISolicitacaoService
{
  private readonly AppDbContext _context;
  private readonly ILogger<SolicitacaoService> _logger;

  public SolicitacaoService(AppDbContext context, ILogger<SolicitacaoService> logger)
  {
    _context = context;
    _logger = logger;
  }

  public async Task<SolicitacaoGeral> CreateGeralAsync(CreateSolicitacaoGeralDto dto, long solicitanteId)
  {
    await using var transaction = await _context.Database.BeginTransactionAsync();
    try
    {
      var novaSolicitacao = new SolicitacaoGeral
      {
        SolicitanteId = solicitanteId,
        GestorId = dto.GestorId,
        DataCriacao = DateTime.UtcNow,
        JustificativaGeral = dto.JustificativaGeral
      };

      foreach (var itemDto in dto.Itens)
      {
        var itemDoCatalogo = await _context.Items.FindAsync(itemDto.ItemId);
        if (itemDoCatalogo == null || !itemDoCatalogo.IsActive)
        {
          throw new Exception($"Item com ID {itemDto.ItemId} não existe ou está inativo.");
        }

        var solicitacaoItem = new SolicitacaoItem
        {
          Solicitacao = novaSolicitacao,
          ItemId = itemDto.ItemId,
          Quantidade = itemDto.Quantidade,
          ValorUnitario = itemDoCatalogo.PrecoSugerido,
          Justificativa = null
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
}