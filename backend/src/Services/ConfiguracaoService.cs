using ComprasTccApp.Backend.Extensions;
using ComprasTccApp.Models.Entities.Configuracoes;
using Database;
using Microsoft.EntityFrameworkCore;
using Models.Dtos;
using Services.Interfaces;

public class ConfiguracaoService : IConfiguracaoService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ConfiguracaoService> _logger;
    private const string ChavePrazoSubmissao = "PrazoSubmissaoSolicitacoes";

    public ConfiguracaoService(AppDbContext context, ILogger<ConfiguracaoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DateTime?> GetPrazoSubmissaoAsync()
    {
        var config = await _context.Configuracoes.FindAsync(ChavePrazoSubmissao);
        if (config != null && DateTime.TryParse(config.Valor, out var data))
        {
            return data;
        }
        return null;
    }

    public async Task SetPrazoSubmissaoAsync(DateTime novaData)
    {
        var config = await _context.Configuracoes.FindAsync(ChavePrazoSubmissao);
        if (config == null)
        {
            config = new Configuracao
            {
                Chave = ChavePrazoSubmissao,
                Valor = novaData.ToUniversalTime().ToString("o"),
            };
            await _context.Configuracoes.AddAsync(config);
        }
        else
        {
            config.Valor = novaData.ToUniversalTime().ToString("o");
        }
        config.Valor = novaData.ToUniversalTime().ToString("o");
        await _context.SaveChangesAsync();
    }
}
