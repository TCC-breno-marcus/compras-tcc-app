using ComprasTccApp.Backend.Domain;
using ComprasTccApp.Backend.Models.Entities.Items;
using ComprasTccApp.Models.Entities.Categorias;
using ComprasTccApp.Models.Entities.Centros;
using ComprasTccApp.Models.Entities.Departamentos;
using ComprasTccApp.Models.Entities.Itens;
using ComprasTccApp.Models.Entities.Pessoas;
using ComprasTccApp.Models.Entities.Servidores;
using ComprasTccApp.Models.Entities.Solicitacoes;
using ComprasTccApp.Models.Entities.Solicitantes;
using ComprasTccApp.Models.Entities.Status;
using Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ComprasTccApp.Tests.Services;

public class DashboardServiceTests
{
    [Fact]
    public async Task GetDashboardDataAsync_DeveRetornarDashboardComKPIsEGraficos_QuandoHaDadosNoAno()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetDashboardDataAsync_DeveRetornarDashboardComKPIsEGraficos_QuandoHaDadosNoAno));
        await SeedBaseAsync(context);

        var categoriaA = new Categoria
        {
            Id = 1,
            Nome = "Eletronicos",
            Descricao = "Eletronicos",
            IsActive = true,
        };
        var categoriaB = new Categoria
        {
            Id = 2,
            Nome = "Laboratorio",
            Descricao = "Laboratorio",
            IsActive = true,
        };

        var itemA = new Item
        {
            Id = 1,
            Nome = "Mouse",
            CatMat = "100",
            Descricao = "Mouse USB",
            LinkImagem = "mouse.png",
            Especificacao = "Optico",
            PrecoSugerido = 10,
            IsActive = true,
            CategoriaId = categoriaA.Id,
            Categoria = categoriaA,
        };
        var itemB = new Item
        {
            Id = 2,
            Nome = "Teclado",
            CatMat = "200",
            Descricao = "Teclado USB",
            LinkImagem = "teclado.png",
            Especificacao = "Mecanico",
            PrecoSugerido = 20,
            IsActive = true,
            CategoriaId = categoriaA.Id,
            Categoria = categoriaA,
        };
        var itemC = new Item
        {
            Id = 3,
            Nome = "Bequer",
            CatMat = "300",
            Descricao = "Bequer 500ml",
            LinkImagem = "bequer.png",
            Especificacao = "Vidro",
            PrecoSugerido = 30,
            IsActive = true,
            CategoriaId = categoriaB.Id,
            Categoria = categoriaB,
        };

        var deptoComputacao = await CriarSolicitanteAsync(context, 1, "Computacao", "DC");
        var deptoQuimica = await CriarSolicitanteAsync(context, 2, "Quimica", "DQ");

        var statusPendente = await context.StatusSolicitacoes.FindAsync(StatusConsts.Pendente);
        var statusRejeitada = await context.StatusSolicitacoes.FindAsync(StatusConsts.Rejeitada);

        var solicitacaoAtiva1 = new Solicitacao
        {
            Id = 1,
            SolicitanteId = deptoComputacao.Id,
            Solicitante = deptoComputacao,
            StatusId = StatusConsts.Pendente,
            Status = statusPendente!,
            DataCriacao = new DateTime(2026, 3, 10),
        };
        solicitacaoAtiva1.ItemSolicitacao.Add(
            new SolicitacaoItem
            {
                SolicitacaoId = solicitacaoAtiva1.Id,
                Solicitacao = solicitacaoAtiva1,
                ItemId = itemA.Id,
                Item = itemA,
                Quantidade = 2,
                ValorUnitario = 10,
            }
        );
        solicitacaoAtiva1.ItemSolicitacao.Add(
            new SolicitacaoItem
            {
                SolicitacaoId = solicitacaoAtiva1.Id,
                Solicitacao = solicitacaoAtiva1,
                ItemId = itemB.Id,
                Item = itemB,
                Quantidade = 1,
                ValorUnitario = 20,
            }
        );

        var solicitacaoAtiva2 = new Solicitacao
        {
            Id = 2,
            SolicitanteId = deptoQuimica.Id,
            Solicitante = deptoQuimica,
            StatusId = StatusConsts.Aprovada,
            Status = await context.StatusSolicitacoes.FindAsync(StatusConsts.Aprovada) ?? null!,
            DataCriacao = new DateTime(2026, 5, 2),
        };
        solicitacaoAtiva2.ItemSolicitacao.Add(
            new SolicitacaoItem
            {
                SolicitacaoId = solicitacaoAtiva2.Id,
                Solicitacao = solicitacaoAtiva2,
                ItemId = itemA.Id,
                Item = itemA,
                Quantidade = 3,
                ValorUnitario = 10,
            }
        );
        solicitacaoAtiva2.ItemSolicitacao.Add(
            new SolicitacaoItem
            {
                SolicitacaoId = solicitacaoAtiva2.Id,
                Solicitacao = solicitacaoAtiva2,
                ItemId = itemC.Id,
                Item = itemC,
                Quantidade = 5,
                ValorUnitario = 30,
            }
        );

        var solicitacaoInativa = new Solicitacao
        {
            Id = 3,
            SolicitanteId = deptoComputacao.Id,
            Solicitante = deptoComputacao,
            StatusId = StatusConsts.Rejeitada,
            Status = statusRejeitada!,
            DataCriacao = new DateTime(2026, 7, 1),
        };
        solicitacaoInativa.ItemSolicitacao.Add(
            new SolicitacaoItem
            {
                SolicitacaoId = solicitacaoInativa.Id,
                Solicitacao = solicitacaoInativa,
                ItemId = itemB.Id,
                Item = itemB,
                Quantidade = 50,
                ValorUnitario = 20,
            }
        );

        context.Categorias.AddRange(categoriaA, categoriaB);
        context.Items.AddRange(itemA, itemB, itemC);
        context.Solicitacoes.AddRange(solicitacaoAtiva1, solicitacaoAtiva2, solicitacaoInativa);
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = await service.GetDashboardDataAsync(2026);

        // Assert
        resultado.Kpis.TotalSolicitacoes.Should().Be(3);
        resultado.Kpis.ValorTotalEstimado.Should().Be(220);
        resultado.Kpis.TotalItensUnicos.Should().Be(3);
        resultado.Kpis.TotalUnidadesSolicitadas.Should().Be(11);
        resultado.Kpis.TotalDepartamentosSolicitantes.Should().Be(2);
        resultado.Kpis.CustoMedioSolicitacao.Should().Be(110);

        resultado.ValorPorDepartamento.Labels.Should().Contain(["DC", "DQ"]);
        resultado.ValorPorDepartamento.Data.Sum().Should().Be(220);

        resultado.ValorPorCategoria.Labels.Should().Contain(["Eletronicos", "Laboratorio"]);
        resultado.ValorPorCategoria.Data.Sum().Should().Be(220);

        resultado.VisaoGeralStatus.Labels.Should().Contain(["Pendente", "Aprovada", "Rejeitada"]);
        resultado.VisaoGeralStatus.Data.Sum().Should().Be(3);

        resultado.TopItensPorQuantidade.Should().NotBeEmpty();
        resultado.TopItensPorQuantidade.First().Valor.Should().Be(5);
        resultado.TopItensPorQuantidade.Select(i => i.ItemId).Should().Contain([1, 3]);

        resultado.TopItensPorValorTotal.Should().NotBeEmpty();
        resultado.TopItensPorValorTotal.First().ItemId.Should().Be(3);
        resultado.TopItensPorValorTotal.First().Valor.Should().Be(150);
    }

    [Theory]
    [InlineData(2024)]
    [InlineData(2030)]
    public async Task GetDashboardDataAsync_DeveRetornarZeros_QuandoNaoHaSolicitacoesNoAno(int ano)
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(GetDashboardDataAsync_DeveRetornarZeros_QuandoNaoHaSolicitacoesNoAno) + ano
        );
        await SeedBaseAsync(context);
        var service = CriarServico(context);

        // Act
        var resultado = await service.GetDashboardDataAsync(ano);

        // Assert
        resultado.Kpis.TotalSolicitacoes.Should().Be(0);
        resultado.Kpis.ValorTotalEstimado.Should().Be(0);
        resultado.Kpis.TotalItensUnicos.Should().Be(0);
        resultado.Kpis.TotalUnidadesSolicitadas.Should().Be(0);
        resultado.Kpis.TotalDepartamentosSolicitantes.Should().Be(0);
        resultado.Kpis.CustoMedioSolicitacao.Should().Be(0);
        resultado.ValorPorDepartamento.Labels.Should().BeEmpty();
        resultado.ValorPorDepartamento.Data.Should().BeEmpty();
        resultado.ValorPorCategoria.Labels.Should().BeEmpty();
        resultado.ValorPorCategoria.Data.Should().BeEmpty();
        resultado.VisaoGeralStatus.Labels.Should().BeEmpty();
        resultado.VisaoGeralStatus.Data.Should().BeEmpty();
        resultado.TopItensPorQuantidade.Should().BeEmpty();
        resultado.TopItensPorValorTotal.Should().BeEmpty();
    }

    private static DashboardService CriarServico(AppDbContext context)
    {
        var loggerMock = new Mock<ILogger<DepartamentoService>>();
        return new DashboardService(context, loggerMock.Object);
    }

    private static AppDbContext CriarContexto(string nomeBanco)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;
        return new AppDbContext(options);
    }

    private static async Task SeedBaseAsync(AppDbContext context)
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

    private static async Task<Solicitante> CriarSolicitanteAsync(
        AppDbContext context,
        int index,
        string nomeDepto,
        string siglaDepto
    )
    {
        var centro = new Centro
        {
            Id = index,
            Nome = $"Centro {index}",
            Sigla = $"C{index}",
            Email = $"centro{index}@uni.com",
            Telefone = "1133333333",
        };

        var departamento = new Departamento
        {
            Id = index,
            Nome = nomeDepto,
            Sigla = siglaDepto,
            Email = $"{siglaDepto.ToLower()}@uni.com",
            Telefone = "1144444444",
            CentroId = centro.Id,
            Centro = centro,
        };

        var pessoa = new Pessoa
        {
            Id = index,
            Nome = $"Pessoa {index}",
            Email = $"pessoa{index}@uni.com",
            Telefone = "11999999999",
            CPF = (12345678900 + index).ToString(),
            DataAtualizacao = DateTime.UtcNow,
            PasswordHash = "hash",
            Role = "Solicitante",
            IsActive = true,
        };

        var servidor = new Servidor
        {
            Id = index,
            PessoaId = pessoa.Id,
            Pessoa = pessoa,
            IdentificadorInterno = $"TEMP-{index}",
            IsGestor = false,
        };

        var solicitante = new Solicitante
        {
            Id = index,
            ServidorId = servidor.Id,
            Servidor = servidor,
            DepartamentoId = departamento.Id,
            Departamento = departamento,
            DataUltimaSolicitacao = DateTime.UtcNow,
        };

        context.Centros.Add(centro);
        context.Departamentos.Add(departamento);
        context.Pessoas.Add(pessoa);
        context.Servidores.Add(servidor);
        context.Solicitantes.Add(solicitante);
        await context.SaveChangesAsync();
        return solicitante;
    }
}
