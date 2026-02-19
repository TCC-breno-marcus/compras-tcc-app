using System.Security.Claims;
using ComprasTccApp.Backend.Domain;
using ComprasTccApp.Backend.Models.Entities.Items;
using ComprasTccApp.Models.Entities.Categorias;
using ComprasTccApp.Models.Entities.Centros;
using ComprasTccApp.Models.Entities.Departamentos;
using ComprasTccApp.Models.Entities.Historicos;
using ComprasTccApp.Models.Entities.Itens;
using ComprasTccApp.Models.Entities.Pessoas;
using ComprasTccApp.Models.Entities.Servidores;
using ComprasTccApp.Models.Entities.Solicitacoes;
using ComprasTccApp.Models.Entities.Solicitantes;
using ComprasTccApp.Models.Entities.Status;
using ComprasTccApp.Services.Interfaces;
using Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Models.Dtos;
using Moq;
using Services.Interfaces;

namespace ComprasTccApp.Tests.Services;

public class SolicitacaoServiceTests
{
    [Fact]
    public async Task CreateGeralAsync_DeveCriarSolicitacaoERetornarDto_QuandoDadosValidos()
    {
        // Arrange
        await using var context = CriarContexto(nameof(CreateGeralAsync_DeveCriarSolicitacaoERetornarDto_QuandoDadosValidos));
        var dados = await SeedDadosBaseAsync(context);

        var configuracaoServiceMock = new Mock<IConfiguracaoService>();
        configuracaoServiceMock
            .Setup(c => c.GetConfiguracoesAsync())
            .ReturnsAsync(CriarConfiguracao(DateTime.UtcNow.AddDays(1)));

        var usuarioServiceMock = new Mock<IUsuarioService>();
        usuarioServiceMock
            .Setup(u => u.GetSolicitanteInfoAsync(dados.Pessoa.Id))
            .ReturnsAsync((dados.Servidor, dados.Solicitante));

        var service = CriarServico(context, configuracaoServiceMock, usuarioServiceMock);
        var dto = new CreateSolicitacaoGeralDto
        {
            JustificativaGeral = "Material para atividades acadêmicas.",
            Itens =
            [
                new SolicitacaoItemDto
                {
                    ItemId = dados.ItemAtivo.Id,
                    Quantidade = 2,
                    ValorUnitario = 50,
                },
            ],
        };

        // Act
        var resultado = await service.CreateGeralAsync(dto, dados.Pessoa.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().BeGreaterThan(0);
        resultado.ExternalId.Should().StartWith("SG-");
        resultado.Status.Id.Should().Be(StatusConsts.Pendente);
        resultado.Itens.Should().ContainSingle();
        resultado.Itens[0].Id.Should().Be(dados.ItemAtivo.Id);

        context.Solicitacoes.Should().ContainSingle();
        context.HistoricoSolicitacoes.Should().ContainSingle();
    }

    [Fact]
    public async Task CreateGeralAsync_DeveLancarInvalidOperationException_QuandoPrazoSubmissaoEncerrado()
    {
        // Arrange
        await using var context = CriarContexto(nameof(CreateGeralAsync_DeveLancarInvalidOperationException_QuandoPrazoSubmissaoEncerrado));
        var configuracaoServiceMock = new Mock<IConfiguracaoService>();
        configuracaoServiceMock
            .Setup(c => c.GetConfiguracoesAsync())
            .ReturnsAsync(CriarConfiguracao(DateTime.UtcNow.AddMinutes(-1)));

        var usuarioServiceMock = new Mock<IUsuarioService>();
        var service = CriarServico(context, configuracaoServiceMock, usuarioServiceMock);

        var dto = new CreateSolicitacaoGeralDto
        {
            JustificativaGeral = "Material para atividades acadêmicas.",
            Itens =
            [
                new SolicitacaoItemDto
                {
                    ItemId = 1,
                    Quantidade = 1,
                    ValorUnitario = 10,
                },
            ],
        };

        // Act
        var act = () => service.CreateGeralAsync(dto, 1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*prazo para a criação*");
        usuarioServiceMock.Verify(u => u.GetSolicitanteInfoAsync(It.IsAny<long>()), Times.Never);
    }

    [Fact]
    public async Task CreatePatrimonialAsync_DeveLancarExcecao_QuandoItemInativo()
    {
        // Arrange
        await using var context = CriarContexto(nameof(CreatePatrimonialAsync_DeveLancarExcecao_QuandoItemInativo));
        var dados = await SeedDadosBaseAsync(context);

        var configuracaoServiceMock = new Mock<IConfiguracaoService>();
        configuracaoServiceMock
            .Setup(c => c.GetConfiguracoesAsync())
            .ReturnsAsync(CriarConfiguracao(DateTime.UtcNow.AddDays(1)));

        var usuarioServiceMock = new Mock<IUsuarioService>();
        usuarioServiceMock
            .Setup(u => u.GetSolicitanteInfoAsync(dados.Pessoa.Id))
            .ReturnsAsync((dados.Servidor, dados.Solicitante));

        var service = CriarServico(context, configuracaoServiceMock, usuarioServiceMock);
        var dto = new CreateSolicitacaoPatrimonialDto
        {
            Itens =
            [
                new SolicitacaoItemDto
                {
                    ItemId = dados.ItemInativo.Id,
                    Quantidade = 1,
                    ValorUnitario = 100,
                    Justificativa = "Reposição",
                },
            ],
        };

        // Act
        var act = () => service.CreatePatrimonialAsync(dto, dados.Pessoa.Id);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("*não existe ou está inativo*");
    }

    [Fact]
    public async Task GetByIdAsync_DeveRetornarNull_QuandoSolicitacaoNaoEncontrada()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetByIdAsync_DeveRetornarNull_QuandoSolicitacaoNaoEncontrada));
        var service = CriarServico(context);

        // Act
        var resultado = await service.GetByIdAsync(999);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_DeveRetornarDtoComKpis_QuandoSolicitacaoExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetByIdAsync_DeveRetornarDtoComKpis_QuandoSolicitacaoExiste));
        var dados = await SeedDadosBaseAsync(context);
        var solicitacao = await CriarSolicitacaoGeralAsync(context, dados);
        var service = CriarServico(context);

        // Act
        var resultado = await service.GetByIdAsync(solicitacao.Id);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(solicitacao.Id);
        resultado.Kpis.Should().NotBeNull();
        resultado.Kpis!.ValorTotalEstimado.Should().Be(150);
        resultado.Kpis.TotalUnidades.Should().Be(3);
        resultado.TopItensPorValor.Should().NotBeNull();
        resultado.TopItensPorValor!.Should().ContainSingle();
    }

    [Theory]
    [InlineData(StatusConsts.Aprovada)]
    [InlineData(StatusConsts.Rejeitada)]
    [InlineData(StatusConsts.Encerrada)]
    public async Task CancelarSolicitacaoAsync_DeveLancarInvalidOperationException_QuandoSolicitacaoFinalizada(
        int statusId
    )
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(CancelarSolicitacaoAsync_DeveLancarInvalidOperationException_QuandoSolicitacaoFinalizada)
                + statusId
        );
        var dados = await SeedDadosBaseAsync(context);
        var solicitacao = await CriarSolicitacaoComStatusAsync(context, dados, statusId);
        var service = CriarServico(context);

        // Act
        var act = () => service.CancelarSolicitacaoAsync(
            solicitacao.Id,
            dados.Pessoa.Id,
            isAdmin: true,
            new CancelarSolicitacaoDto { Observacoes = "Teste" }
        );

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*já foi finalizada*");
    }

    [Fact]
    public async Task AtualizarStatusSolicitacaoAsync_DeveRetornarNull_QuandoSolicitacaoNaoExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(AtualizarStatusSolicitacaoAsync_DeveRetornarNull_QuandoSolicitacaoNaoExiste));
        await SeedStatusAsync(context);
        var service = CriarServico(context);

        // Act
        var resultado = await service.AtualizarStatusSolicitacaoAsync(
            999,
            1,
            new UpdateStatusSolicitacaoDto { NovoStatusId = StatusConsts.Aprovada }
        );

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task AtualizarStatusSolicitacaoAsync_DeveAtualizarStatusERetornarSolicitacao_QuandoSolicitacaoValida()
    {
        // Arrange
        await using var context = CriarContexto(nameof(AtualizarStatusSolicitacaoAsync_DeveAtualizarStatusERetornarSolicitacao_QuandoSolicitacaoValida));
        var dados = await SeedDadosBaseAsync(context);
        var solicitacao = await CriarSolicitacaoComStatusAsync(context, dados, StatusConsts.Pendente);
        var service = CriarServico(context);

        // Act
        var resultado = await service.AtualizarStatusSolicitacaoAsync(
            solicitacao.Id,
            dados.Pessoa.Id,
            new UpdateStatusSolicitacaoDto
            {
                NovoStatusId = StatusConsts.Aprovada,
                Observacoes = "Aprovado para compra.",
            }
        );

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Status.Id.Should().Be(StatusConsts.Aprovada);
        context.HistoricoSolicitacoes.Should().ContainSingle(h =>
            h.SolicitacaoId == solicitacao.Id && h.Acao == ComprasTccApp.Backend.Enums.AcaoHistoricoEnum.MudancaDeStatus
        );
    }

    [Fact]
    public async Task GetHistoricoAsync_DeveRetornarNull_QuandoSolicitacaoNaoExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetHistoricoAsync_DeveRetornarNull_QuandoSolicitacaoNaoExiste));
        var service = CriarServico(context);
        var user = CriarPrincipal(1, "Solicitante");

        // Act
        var resultado = await service.GetHistoricoAsync(999, user);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GetHistoricoAsync_DeveRetornarNull_QuandoSolicitanteNaoForDonoDaSolicitacao()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetHistoricoAsync_DeveRetornarNull_QuandoSolicitanteNaoForDonoDaSolicitacao));
        var dados = await SeedDadosBaseAsync(context);
        var solicitacao = await CriarSolicitacaoComStatusAsync(context, dados, StatusConsts.Pendente);
        context.HistoricoSolicitacoes.Add(
            new HistoricoSolicitacao
            {
                SolicitacaoId = solicitacao.Id,
                PessoaId = dados.Pessoa.Id,
                DataOcorrencia = DateTime.UtcNow,
                Acao = ComprasTccApp.Backend.Enums.AcaoHistoricoEnum.Criacao,
                Detalhes = "Criada",
            }
        );
        await context.SaveChangesAsync();

        var service = CriarServico(context);
        var outroUsuario = CriarPrincipal(9999, "Solicitante");

        // Act
        var resultado = await service.GetHistoricoAsync(solicitacao.Id, outroUsuario);

        // Assert
        resultado.Should().BeNull();
    }

    private static SolicitacaoService CriarServico(
        AppDbContext context,
        Mock<IConfiguracaoService>? configuracaoServiceMock = null,
        Mock<IUsuarioService>? usuarioServiceMock = null
    )
    {
        if (configuracaoServiceMock == null)
        {
            configuracaoServiceMock = new Mock<IConfiguracaoService>();
            configuracaoServiceMock
                .Setup(c => c.GetConfiguracoesAsync())
                .ReturnsAsync(CriarConfiguracao(DateTime.UtcNow.AddDays(1)));
        }

        usuarioServiceMock ??= new Mock<IUsuarioService>();

        var loggerMock = new Mock<ILogger<SolicitacaoService>>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["IMAGE_BASE_URL"] = "https://cdn/" })
            .Build();

        return new SolicitacaoService(
            context,
            loggerMock.Object,
            configuracaoServiceMock.Object,
            usuarioServiceMock.Object,
            configuration
        );
    }

    private static AppDbContext CriarContexto(string nomeBanco)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nomeBanco)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }

    private static ClaimsPrincipal CriarPrincipal(long pessoaId, string role)
    {
        return new ClaimsPrincipal(
            new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, pessoaId.ToString()),
                    new Claim(ClaimTypes.Role, role),
                ],
                "Teste"
            )
        );
    }

    private static ConfiguracaoDto CriarConfiguracao(DateTime? prazoSubmissao)
    {
        return new ConfiguracaoDto
        {
            PrazoSubmissao = prazoSubmissao,
            MaxQuantidadePorItem = 100,
            MaxItensDiferentesPorSolicitacao = 10,
            EmailContatoPrincipal = "contato@universidade.edu",
            EmailParaNotificacoes = "notificacoes@universidade.edu",
        };
    }

    private static async Task SeedStatusAsync(AppDbContext context)
    {
        context.StatusSolicitacoes.AddRange(
            new StatusSolicitacao { Id = StatusConsts.Pendente, Nome = "Pendente" },
            new StatusSolicitacao { Id = StatusConsts.AguardandoAjustes, Nome = "Aguardando Ajustes" },
            new StatusSolicitacao { Id = StatusConsts.Aprovada, Nome = "Aprovada" },
            new StatusSolicitacao { Id = StatusConsts.Rejeitada, Nome = "Rejeitada" },
            new StatusSolicitacao { Id = StatusConsts.Cancelada, Nome = "Cancelada" },
            new StatusSolicitacao { Id = StatusConsts.Encerrada, Nome = "Encerrada" }
        );
        await context.SaveChangesAsync();
    }

    private static async Task<(Pessoa Pessoa, Servidor Servidor, Solicitante Solicitante, Item ItemAtivo, Item ItemInativo)> SeedDadosBaseAsync(
        AppDbContext context
    )
    {
        await SeedStatusAsync(context);

        var centro = new Centro
        {
            Id = 1,
            Nome = "Centro de Tecnologia",
            Sigla = "CT",
            Email = "ct@universidade.edu",
            Telefone = "1133333333",
        };

        var departamento = new Departamento
        {
            Id = 1,
            Nome = "Departamento de Computacao",
            Sigla = "DC",
            Email = "dc@universidade.edu",
            Telefone = "1144444444",
            Centro = centro,
            CentroId = centro.Id,
        };

        var pessoa = new Pessoa
        {
            Id = 1,
            Nome = "Pessoa Teste",
            Email = "pessoa@universidade.edu",
            Telefone = "11999999999",
            CPF = "12345678901",
            DataAtualizacao = DateTime.UtcNow,
            PasswordHash = "hash",
            Role = "Solicitante",
            IsActive = true,
        };

        var servidor = new Servidor
        {
            Id = 1,
            PessoaId = pessoa.Id,
            Pessoa = pessoa,
            IdentificadorInterno = "TEMP-1",
            IsGestor = false,
        };

        var solicitante = new Solicitante
        {
            Id = 1,
            ServidorId = servidor.Id,
            Servidor = servidor,
            DepartamentoId = departamento.Id,
            Departamento = departamento,
            DataUltimaSolicitacao = DateTime.UtcNow,
        };

        var categoria = new Categoria
        {
            Id = 1,
            Nome = "Mobiliário",
            Descricao = "Itens de mobiliário",
            IsActive = true,
        };

        var itemAtivo = new Item
        {
            Id = 1,
            Nome = "Armario",
            CatMat = "CAT-001",
            Descricao = "Armario metálico",
            LinkImagem = "armario.png",
            Especificacao = "Aço",
            PrecoSugerido = 100,
            IsActive = true,
            CategoriaId = categoria.Id,
            Categoria = categoria,
        };

        var itemInativo = new Item
        {
            Id = 2,
            Nome = "Mesa",
            CatMat = "CAT-002",
            Descricao = "Mesa de escritório",
            LinkImagem = "mesa.png",
            Especificacao = "Madeira",
            PrecoSugerido = 200,
            IsActive = false,
            CategoriaId = categoria.Id,
            Categoria = categoria,
        };

        context.Centros.Add(centro);
        context.Departamentos.Add(departamento);
        context.Pessoas.Add(pessoa);
        context.Servidores.Add(servidor);
        context.Solicitantes.Add(solicitante);
        context.Categorias.Add(categoria);
        context.Items.AddRange(itemAtivo, itemInativo);
        await context.SaveChangesAsync();

        return (pessoa, servidor, solicitante, itemAtivo, itemInativo);
    }

    private static async Task<SolicitacaoGeral> CriarSolicitacaoGeralAsync(
        AppDbContext context,
        (Pessoa Pessoa, Servidor Servidor, Solicitante Solicitante, Item ItemAtivo, Item ItemInativo) dados
    )
    {
        var status = await context.StatusSolicitacoes.FirstAsync(s => s.Id == StatusConsts.Pendente);
        var solicitacao = new SolicitacaoGeral
        {
            SolicitanteId = dados.Solicitante.Id,
            Solicitante = dados.Solicitante,
            StatusId = StatusConsts.Pendente,
            Status = status,
            DataCriacao = DateTime.UtcNow,
            JustificativaGeral = "Justificativa geral",
            ExternalId = "SG-2026-0001",
            ItemSolicitacao = [],
        };
        solicitacao.ItemSolicitacao.Add(
            new SolicitacaoItem
            {
                Solicitacao = solicitacao,
                ItemId = dados.ItemAtivo.Id,
                Item = dados.ItemAtivo,
                Quantidade = 3,
                ValorUnitario = 50,
            }
        );

        context.Solicitacoes.Add(solicitacao);
        await context.SaveChangesAsync();
        return solicitacao;
    }

    private static async Task<SolicitacaoGeral> CriarSolicitacaoComStatusAsync(
        AppDbContext context,
        (Pessoa Pessoa, Servidor Servidor, Solicitante Solicitante, Item ItemAtivo, Item ItemInativo) dados,
        int statusId
    )
    {
        var status = await context.StatusSolicitacoes.FirstAsync(s => s.Id == statusId);
        var solicitacao = new SolicitacaoGeral
        {
            SolicitanteId = dados.Solicitante.Id,
            Solicitante = dados.Solicitante,
            StatusId = statusId,
            Status = status,
            DataCriacao = DateTime.UtcNow,
            JustificativaGeral = "Justificativa",
            ExternalId = $"SG-2026-{statusId:D4}",
            ItemSolicitacao = [],
        };
        solicitacao.ItemSolicitacao.Add(
            new SolicitacaoItem
            {
                Solicitacao = solicitacao,
                ItemId = dados.ItemAtivo.Id,
                Item = dados.ItemAtivo,
                Quantidade = 1,
                ValorUnitario = 100,
            }
        );

        context.Solicitacoes.Add(solicitacao);
        await context.SaveChangesAsync();
        return solicitacao;
    }
}
