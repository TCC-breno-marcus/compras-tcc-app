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

    private static class Keys
    {
        public const string PrazoSubmissao = "PrazoSubmissaoSolicitacoes";
        public const string MaxQuantidade = "MaxQuantidadePorItem";
        public const string MaxItens = "MaxItensDiferentesPorSolicitacao";
        public const string EmailContato = "EmailContatoPrincipal";
        public const string EmailNotificacoes = "EmailParaNotificacoes";
    }

    public ConfiguracaoService(AppDbContext context, ILogger<ConfiguracaoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Recupera as configurações funcionais do sistema convertendo os valores persistidos para o DTO tipado.
    /// Aplica valores padrão quando chaves não existem ou possuem formato inválido.
    /// </summary>
    /// <returns>Objeto com as configurações de submissão, limites e emails de contato/notificação.</returns>
    /// <exception cref="Exception">Propaga falhas inesperadas durante a leitura do banco de dados.</exception>
    public async Task<ConfiguracaoDto> GetConfiguracoesAsync()
    {
        var configs = await _context.Configuracoes.ToDictionaryAsync(c => c.Chave, c => c.Valor);

        var dto = new ConfiguracaoDto
        {
            PrazoSubmissao =
                configs.TryGetValue(Keys.PrazoSubmissao, out string? prazoStr)
                && DateTime.TryParse(prazoStr, out var prazo)
                    ? prazo
                    : null,
            MaxItensDiferentesPorSolicitacao =
                configs.TryGetValue(Keys.MaxItens, out string? maxItensStr)
                && int.TryParse(maxItensStr, out var maxItens)
                    ? maxItens
                    : 99, // Default

            MaxQuantidadePorItem =
                configs.TryGetValue(Keys.MaxQuantidade, out string? maxQuantidadeStr)
                && int.TryParse(maxQuantidadeStr, out var maxQuantidade)
                    ? maxQuantidade
                    : 9999, // Default

            EmailContatoPrincipal = configs.GetValueOrDefault(
                Keys.EmailContato,
                "nao-configurado@sistema.com"
            ),
            EmailParaNotificacoes = configs.GetValueOrDefault(
                Keys.EmailNotificacoes,
                "nao-configurado@sistema.com"
            ),
        };

        return dto;
    }

    /// <summary>
    /// Atualiza as configurações informadas, realizando upsert por chave para cada campo preenchido no DTO.
    /// Campos nulos ou vazios não são alterados.
    /// </summary>
    /// <param name="dto">Dados de atualização parcial das configurações do sistema.</param>
    /// <returns>Uma <see cref="Task"/> que representa a conclusão da operação.</returns>
    /// <exception cref="Exception">Propaga falhas inesperadas durante persistência no banco de dados.</exception>
    public async Task UpdateConfiguracoesAsync(UpdateConfiguracaoDto dto)
    {
        var chavesParaAtualizar = new List<string>
        {
            Keys.PrazoSubmissao,
            Keys.MaxItens,
            Keys.MaxQuantidade,
            Keys.EmailContato,
            Keys.EmailNotificacoes,
        };

        var configsDoBanco = await _context
            .Configuracoes.Where(c => chavesParaAtualizar.Contains(c.Chave))
            .ToDictionaryAsync(c => c.Chave);

        if (dto.PrazoSubmissao.HasValue)
        {
            var valor = dto.PrazoSubmissao.Value.ToUniversalTime().ToString("o");
            if (configsDoBanco.TryGetValue(Keys.PrazoSubmissao, out var config))
            {
                config.Valor = valor;
            }
            else
            {
                await _context.Configuracoes.AddAsync(
                    new Configuracao { Chave = Keys.PrazoSubmissao, Valor = valor }
                );
            }
        }

        if (dto.MaxItensDiferentesPorSolicitacao.HasValue)
        {
            var valor = dto.MaxItensDiferentesPorSolicitacao.Value.ToString();
            if (configsDoBanco.TryGetValue(Keys.MaxItens, out var config))
            {
                config.Valor = valor;
            }
            else
            {
                await _context.Configuracoes.AddAsync(
                    new Configuracao { Chave = Keys.MaxItens, Valor = valor }
                );
            }
        }

        if (dto.MaxQuantidadePorItem.HasValue)
        {
            var valor = dto.MaxQuantidadePorItem.Value.ToString();
            if (configsDoBanco.TryGetValue(Keys.MaxQuantidade, out var config))
            {
                config.Valor = valor;
            }
            else
            {
                await _context.Configuracoes.AddAsync(
                    new Configuracao { Chave = Keys.MaxQuantidade, Valor = valor }
                );
            }
        }

        if (!string.IsNullOrEmpty(dto.EmailContatoPrincipal))
        {
            var valor = dto.EmailContatoPrincipal.ToString();
            if (configsDoBanco.TryGetValue(Keys.EmailContato, out var config))
            {
                config.Valor = valor;
            }
            else
            {
                await _context.Configuracoes.AddAsync(
                    new Configuracao { Chave = Keys.EmailContato, Valor = valor }
                );
            }
        }

        if (!string.IsNullOrEmpty(dto.EmailParaNotificacoes))
        {
            var valor = dto.EmailParaNotificacoes.ToString();
            if (configsDoBanco.TryGetValue(Keys.EmailNotificacoes, out var config))
            {
                config.Valor = valor;
            }
            else
            {
                await _context.Configuracoes.AddAsync(
                    new Configuracao { Chave = Keys.EmailNotificacoes, Valor = valor }
                );
            }
        }

        await _context.SaveChangesAsync();
    }
}
