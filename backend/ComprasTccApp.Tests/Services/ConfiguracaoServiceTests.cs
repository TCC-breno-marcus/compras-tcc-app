using ComprasTccApp.Models.Entities.Configuracoes;
using Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.Dtos;
using Moq;

namespace ComprasTccApp.Tests.Services;

public class ConfiguracaoServiceTests
{
    [Fact]
    public async Task GetConfiguracoesAsync_DeveRetornarDefaults_QuandoChavesNaoExistem()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetConfiguracoesAsync_DeveRetornarDefaults_QuandoChavesNaoExistem));
        var service = CriarServico(context);

        // Act
        var resultado = await service.GetConfiguracoesAsync();

        // Assert
        resultado.PrazoSubmissao.Should().BeNull();
        resultado.MaxItensDiferentesPorSolicitacao.Should().Be(99);
        resultado.MaxQuantidadePorItem.Should().Be(9999);
        resultado.EmailContatoPrincipal.Should().Be("nao-configurado@sistema.com");
        resultado.EmailParaNotificacoes.Should().Be("nao-configurado@sistema.com");
    }

    [Fact]
    public async Task GetConfiguracoesAsync_DeveRetornarValoresConvertidos_QuandoChavesValidasExistem()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetConfiguracoesAsync_DeveRetornarValoresConvertidos_QuandoChavesValidasExistem));
        var prazo = new DateTime(2026, 2, 20, 10, 0, 0, DateTimeKind.Utc);
        context.Configuracoes.AddRange(
            new Configuracao { Chave = "PrazoSubmissaoSolicitacoes", Valor = prazo.ToString("o") },
            new Configuracao { Chave = "MaxItensDiferentesPorSolicitacao", Valor = "15" },
            new Configuracao { Chave = "MaxQuantidadePorItem", Valor = "50" },
            new Configuracao { Chave = "EmailContatoPrincipal", Valor = "contato@sistema.com" },
            new Configuracao { Chave = "EmailParaNotificacoes", Valor = "notify@sistema.com" }
        );
        await context.SaveChangesAsync();
        var service = CriarServico(context);

        // Act
        var resultado = await service.GetConfiguracoesAsync();

        // Assert
        resultado.PrazoSubmissao.Should().NotBeNull();
        resultado.PrazoSubmissao!.Value.ToUniversalTime().Should().Be(prazo);
        resultado.MaxItensDiferentesPorSolicitacao.Should().Be(15);
        resultado.MaxQuantidadePorItem.Should().Be(50);
        resultado.EmailContatoPrincipal.Should().Be("contato@sistema.com");
        resultado.EmailParaNotificacoes.Should().Be("notify@sistema.com");
    }

    [Theory]
    [InlineData("abc", "xyz", 99, 9999)]
    [InlineData("", "", 99, 9999)]
    public async Task GetConfiguracoesAsync_DeveAplicarDefaultNumerico_QuandoValoresInvalidos(
        string maxItens,
        string maxQuantidade,
        int esperadoItens,
        int esperadoQuantidade
    )
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetConfiguracoesAsync_DeveAplicarDefaultNumerico_QuandoValoresInvalidos) + maxItens + maxQuantidade);
        context.Configuracoes.AddRange(
            new Configuracao { Chave = "MaxItensDiferentesPorSolicitacao", Valor = maxItens },
            new Configuracao { Chave = "MaxQuantidadePorItem", Valor = maxQuantidade }
        );
        await context.SaveChangesAsync();
        var service = CriarServico(context);

        // Act
        var resultado = await service.GetConfiguracoesAsync();

        // Assert
        resultado.MaxItensDiferentesPorSolicitacao.Should().Be(esperadoItens);
        resultado.MaxQuantidadePorItem.Should().Be(esperadoQuantidade);
    }

    [Fact]
    public async Task UpdateConfiguracoesAsync_DeveInserirChaves_QuandoNaoExistemRegistros()
    {
        // Arrange
        await using var context = CriarContexto(nameof(UpdateConfiguracoesAsync_DeveInserirChaves_QuandoNaoExistemRegistros));
        var service = CriarServico(context);
        var dto = new UpdateConfiguracaoDto
        {
            PrazoSubmissao = new DateTime(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc),
            MaxItensDiferentesPorSolicitacao = 20,
            MaxQuantidadePorItem = 100,
            EmailContatoPrincipal = "contato@tcc.com",
            EmailParaNotificacoes = "notifica@tcc.com",
        };

        // Act
        await service.UpdateConfiguracoesAsync(dto);

        // Assert
        context.Configuracoes.Should().HaveCount(5);
        context.Configuracoes.Should().Contain(c => c.Chave == "MaxItensDiferentesPorSolicitacao" && c.Valor == "20");
        context.Configuracoes.Should().Contain(c => c.Chave == "MaxQuantidadePorItem" && c.Valor == "100");
        context.Configuracoes.Should().Contain(c => c.Chave == "EmailContatoPrincipal" && c.Valor == "contato@tcc.com");
        context.Configuracoes.Should().Contain(c => c.Chave == "EmailParaNotificacoes" && c.Valor == "notifica@tcc.com");
    }

    [Fact]
    public async Task UpdateConfiguracoesAsync_DeveAtualizarSomenteCamposInformados_QuandoAtualizacaoParcial()
    {
        // Arrange
        await using var context = CriarContexto(nameof(UpdateConfiguracoesAsync_DeveAtualizarSomenteCamposInformados_QuandoAtualizacaoParcial));
        context.Configuracoes.AddRange(
            new Configuracao { Chave = "MaxItensDiferentesPorSolicitacao", Valor = "10" },
            new Configuracao { Chave = "MaxQuantidadePorItem", Valor = "30" },
            new Configuracao { Chave = "EmailContatoPrincipal", Valor = "antigo@tcc.com" }
        );
        await context.SaveChangesAsync();
        var service = CriarServico(context);

        var dto = new UpdateConfiguracaoDto
        {
            MaxItensDiferentesPorSolicitacao = 99,
            EmailContatoPrincipal = "novo@tcc.com",
        };

        // Act
        await service.UpdateConfiguracoesAsync(dto);

        // Assert
        context.Configuracoes.Single(c => c.Chave == "MaxItensDiferentesPorSolicitacao").Valor.Should().Be("99");
        context.Configuracoes.Single(c => c.Chave == "EmailContatoPrincipal").Valor.Should().Be("novo@tcc.com");
        context.Configuracoes.Single(c => c.Chave == "MaxQuantidadePorItem").Valor.Should().Be("30");
        context.Configuracoes.Should().NotContain(c => c.Chave == "EmailParaNotificacoes");
    }

    [Fact]
    public async Task UpdateConfiguracoesAsync_DeveIgnorarEmailsVazios_QuandoStringsInvalidas()
    {
        // Arrange
        await using var context = CriarContexto(nameof(UpdateConfiguracoesAsync_DeveIgnorarEmailsVazios_QuandoStringsInvalidas));
        context.Configuracoes.Add(
            new Configuracao { Chave = "EmailContatoPrincipal", Valor = "existente@tcc.com" }
        );
        await context.SaveChangesAsync();
        var service = CriarServico(context);

        var dto = new UpdateConfiguracaoDto
        {
            EmailContatoPrincipal = "",
            EmailParaNotificacoes = null,
        };

        // Act
        await service.UpdateConfiguracoesAsync(dto);

        // Assert
        context.Configuracoes.Should().ContainSingle();
        context.Configuracoes.Single().Valor.Should().Be("existente@tcc.com");
    }

    private static ConfiguracaoService CriarServico(AppDbContext context)
    {
        var loggerMock = new Mock<ILogger<ConfiguracaoService>>();
        return new ConfiguracaoService(context, loggerMock.Object);
    }

    private static AppDbContext CriarContexto(string nomeBanco)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;
        return new AppDbContext(options);
    }
}
