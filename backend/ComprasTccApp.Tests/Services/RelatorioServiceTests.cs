using System.Text;
using ClosedXML.Excel;
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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Models.Dtos;
using Moq;
using Services;

namespace ComprasTccApp.Tests.Services;

public class RelatorioServiceTests
{
    [Fact]
    public async Task GetItensPorDepartamentoAsync_DeveRetornarPaginado_QuandoDadosValidos()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetItensPorDepartamentoAsync_DeveRetornarPaginado_QuandoDadosValidos));
        await SeedRelatorioAsync(context);
        var service = CriarServico(context);

        // Act
        var resultado = await service.GetItensPorDepartamentoAsync(
            searchTerm: null,
            categoriaNome: null,
            itemsType: null,
            siglaDepartamento: null,
            somenteSolicitacoesAtivas: null,
            sortOrder: "asc",
            pageNumber: 1,
            pageSize: 1
        );

        // Assert
        resultado.TotalCount.Should().Be(2);
        resultado.Data.Should().ContainSingle();
        resultado.Data[0].Nome.Should().Be("Armario");
    }

    [Fact]
    public async Task GetItensPorDepartamentoAsync_DeveDesconsiderarRejeitadas_QuandoSomenteSolicitacoesAtivasTrue()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetItensPorDepartamentoAsync_DeveDesconsiderarRejeitadas_QuandoSomenteSolicitacoesAtivasTrue));
        await SeedRelatorioAsync(context);
        var service = CriarServico(context);

        // Act
        var resultado = await service.GetItensPorDepartamentoAsync(
            searchTerm: "Armario",
            categoriaNome: null,
            itemsType: null,
            siglaDepartamento: null,
            somenteSolicitacoesAtivas: true,
            sortOrder: null,
            pageNumber: 1,
            pageSize: 10
        );

        // Assert
        resultado.Data.Should().ContainSingle();
        var item = resultado.Data[0];
        item.QuantidadeTotalSolicitada.Should().Be(2);
        item.ValorTotalSolicitado.Should().Be(200);
    }

    [Theory]
    [InlineData("patrimonial", "Armario")]
    [InlineData("geral", "Resistor")]
    public async Task GetItensPorDepartamentoAsync_DeveFiltrarPorTipo_QuandoItemsTypeInformado(
        string itemsType,
        string nomeEsperado
    )
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetItensPorDepartamentoAsync_DeveFiltrarPorTipo_QuandoItemsTypeInformado) + itemsType);
        await SeedRelatorioAsync(context);
        var service = CriarServico(context);

        // Act
        var resultado = await service.GetItensPorDepartamentoAsync(
            searchTerm: null,
            categoriaNome: null,
            itemsType: itemsType,
            siglaDepartamento: null,
            somenteSolicitacoesAtivas: null,
            sortOrder: null,
            pageNumber: 1,
            pageSize: 10
        );

        // Assert
        resultado.Data.Should().ContainSingle();
        resultado.Data[0].Nome.Should().Be(nomeEsperado);
    }

    [Fact]
    public async Task ExportarItensPorDepartamentoAsync_DeveGerarCsv_QuandoFormatoCsv()
    {
        // Arrange
        await using var context = CriarContexto(nameof(ExportarItensPorDepartamentoAsync_DeveGerarCsv_QuandoFormatoCsv));
        await SeedRelatorioAsync(context);
        var service = CriarServico(context);

        // Act
        var bytes = await service.ExportarItensPorDepartamentoAsync(
            itemsType: "geral",
            formatoArquivo: "csv",
            searchTerm: null,
            categoriaNome: null,
            siglaDepartamento: null,
            somenteSolicitacoesAtivas: true
        );
        var csv = Encoding.UTF8.GetString(bytes);

        // Assert
        bytes.Should().NotBeEmpty();
        csv.Should().Contain("CATMAT");
        csv.Should().Contain("Resistor");
    }

    [Fact]
    public async Task ExportarItensPorDepartamentoAsync_DeveGerarExcel_QuandoFormatoExcel()
    {
        // Arrange
        await using var context = CriarContexto(nameof(ExportarItensPorDepartamentoAsync_DeveGerarExcel_QuandoFormatoExcel));
        await SeedRelatorioAsync(context);
        var service = CriarServico(context);

        // Act
        var bytes = await service.ExportarItensPorDepartamentoAsync(
            itemsType: "patrimonial",
            formatoArquivo: "excel",
            searchTerm: null,
            categoriaNome: null,
            siglaDepartamento: null,
            somenteSolicitacoesAtivas: true
        );

        // Assert
        bytes.Should().NotBeEmpty();
        bytes[0].Should().Be((byte)'P');
        bytes[1].Should().Be((byte)'K');
    }

    [Fact]
    public async Task ExportarItensPorDepartamentoAsync_DeveGerarPdf_QuandoFormatoPdf()
    {
        // Arrange
        await using var context = CriarContexto(nameof(ExportarItensPorDepartamentoAsync_DeveGerarPdf_QuandoFormatoPdf));
        await SeedRelatorioAsync(context);
        var service = CriarServico(context);

        // Act
        var bytes = await service.ExportarItensPorDepartamentoAsync(
            itemsType: "patrimonial",
            formatoArquivo: "pdf",
            searchTerm: null,
            categoriaNome: null,
            siglaDepartamento: null,
            somenteSolicitacoesAtivas: true,
            usuarioSolicitante: "Usuario Teste",
            dataHoraSolicitacao: new DateTimeOffset(2026, 2, 19, 21, 0, 0, TimeSpan.Zero)
        );

        // Assert
        bytes.Should().NotBeEmpty();
        bytes[0].Should().Be((byte)'%');
        bytes[1].Should().Be((byte)'P');
        bytes[2].Should().Be((byte)'D');
        bytes[3].Should().Be((byte)'F');
    }

    [Fact]
    public void GerarExcel_DeveIncluirColunaJustificativa_QuandoIsPatrimonialTrue()
    {
        // Arrange
        var service = CriarServico(CriarContexto(nameof(GerarExcel_DeveIncluirColunaJustificativa_QuandoIsPatrimonialTrue)));
        var insights = new List<ItemPorDepartamentoDto>
        {
            new()
            {
                Id = 1,
                Nome = "Armario",
                CatMat = "P100",
                Descricao = "Armario de laboratorio",
                Especificacao = "Metal",
                CategoriaNome = "Mobiliário",
                LinkImagem = "https://cdn/armario.png",
                PrecoSugerido = 100,
                QuantidadeTotalSolicitada = 2,
                ValorTotalSolicitado = 200,
                PrecoMedio = 100,
                PrecoMinimo = 100,
                PrecoMaximo = 100,
                NumeroDeSolicitacoes = 1,
                DemandaPorDepartamento =
                [
                    new()
                    {
                        Unidade = new UnidadeOrganizacionalDto
                        {
                            Id = 1,
                            Nome = "Computacao",
                            Sigla = "DC",
                            Email = "dc@uni.com",
                            Telefone = "1133",
                            Tipo = "Departamento",
                        },
                        QuantidadeTotal = 2,
                        Justificativa = "Necessario para laboratorio",
                    },
                ],
            },
        };

        // Act
        var bytes = service.GerarExcel(insights, isPatrimonial: true);
        using var stream = new MemoryStream(bytes);
        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet("Relatório Detalhado");

        // Assert
        worksheet.Cell(1, 13).GetString().Should().Be("Justificativa");
    }

    private static RelatorioService CriarServico(AppDbContext context)
    {
        var loggerMock = new Mock<ILogger<RelatorioService>>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["IMAGE_BASE_URL"] = "https://cdn/" })
            .Build();

        return new RelatorioService(context, loggerMock.Object, configuration);
    }

    private static AppDbContext CriarContexto(string nomeBanco)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;
        return new AppDbContext(options);
    }

    private static async Task SeedRelatorioAsync(AppDbContext context)
    {
        context.StatusSolicitacoes.AddRange(
            new StatusSolicitacao { Id = StatusConsts.Pendente, Nome = "Pendente" },
            new StatusSolicitacao { Id = StatusConsts.Aprovada, Nome = "Aprovada" },
            new StatusSolicitacao { Id = StatusConsts.Rejeitada, Nome = "Rejeitada" }
        );

        var centro = new Centro
        {
            Id = 1,
            Nome = "Centro de Tecnologia",
            Sigla = "CT",
            Email = "ct@uni.com",
            Telefone = "1133333333",
        };
        var deptoComputacao = new Departamento
        {
            Id = 1,
            Nome = "Computacao",
            Sigla = "DC",
            Email = "dc@uni.com",
            Telefone = "1144444444",
            CentroId = 1,
            Centro = centro,
        };
        var deptoQuimica = new Departamento
        {
            Id = 2,
            Nome = "Quimica",
            Sigla = "DQ",
            Email = "dq@uni.com",
            Telefone = "1155555555",
            CentroId = 1,
            Centro = centro,
        };

        var pessoa1 = new Pessoa
        {
            Id = 1,
            Nome = "Pessoa 1",
            Email = "p1@uni.com",
            Telefone = "11999999991",
            CPF = "12345678901",
            DataAtualizacao = DateTime.UtcNow,
            PasswordHash = "hash",
            Role = "Solicitante",
            IsActive = true,
        };
        var pessoa2 = new Pessoa
        {
            Id = 2,
            Nome = "Pessoa 2",
            Email = "p2@uni.com",
            Telefone = "11999999992",
            CPF = "12345678902",
            DataAtualizacao = DateTime.UtcNow,
            PasswordHash = "hash",
            Role = "Solicitante",
            IsActive = true,
        };

        var servidor1 = new Servidor
        {
            Id = 1,
            PessoaId = 1,
            Pessoa = pessoa1,
            IdentificadorInterno = "TEMP-1",
            IsGestor = false,
        };
        var servidor2 = new Servidor
        {
            Id = 2,
            PessoaId = 2,
            Pessoa = pessoa2,
            IdentificadorInterno = "TEMP-2",
            IsGestor = false,
        };

        var solicitante1 = new Solicitante
        {
            Id = 1,
            ServidorId = 1,
            Servidor = servidor1,
            DepartamentoId = 1,
            Departamento = deptoComputacao,
            DataUltimaSolicitacao = DateTime.UtcNow,
        };
        var solicitante2 = new Solicitante
        {
            Id = 2,
            ServidorId = 2,
            Servidor = servidor2,
            DepartamentoId = 2,
            Departamento = deptoQuimica,
            DataUltimaSolicitacao = DateTime.UtcNow,
        };

        var categoriaPatrimonial = new Categoria
        {
            Id = 1,
            Nome = "Mobiliário",
            Descricao = "Patrimonial",
            IsActive = true,
        };
        var categoriaGeral = new Categoria
        {
            Id = 2,
            Nome = "Componentes Eletrônicos",
            Descricao = "Geral",
            IsActive = true,
        };

        var itemPatrimonial = new Item
        {
            Id = 1,
            Nome = "Armario",
            CatMat = "P100",
            Descricao = "Armario de laboratorio",
            LinkImagem = "armario.png",
            Especificacao = "Metal",
            PrecoSugerido = 100,
            IsActive = true,
            CategoriaId = 1,
            Categoria = categoriaPatrimonial,
        };
        var itemGeral = new Item
        {
            Id = 2,
            Nome = "Resistor",
            CatMat = "G200",
            Descricao = "Resistor 10k",
            LinkImagem = "resistor.png",
            Especificacao = "10k",
            PrecoSugerido = 5,
            IsActive = true,
            CategoriaId = 2,
            Categoria = categoriaGeral,
        };

        var solicitacaoPatrimonialAtiva = new SolicitacaoPatrimonial
        {
            Id = 1,
            SolicitanteId = 1,
            Solicitante = solicitante1,
            StatusId = StatusConsts.Pendente,
            Status = context.StatusSolicitacoes.Local.First(s => s.Id == StatusConsts.Pendente),
            DataCriacao = new DateTime(2026, 1, 10),
        };

        var solicitacaoPatrimonialRejeitada = new SolicitacaoPatrimonial
        {
            Id = 2,
            SolicitanteId = 2,
            Solicitante = solicitante2,
            StatusId = StatusConsts.Rejeitada,
            Status = context.StatusSolicitacoes.Local.First(s => s.Id == StatusConsts.Rejeitada),
            DataCriacao = new DateTime(2026, 2, 10),
        };

        var solicitacaoGeralAtiva = new SolicitacaoGeral
        {
            Id = 3,
            SolicitanteId = 1,
            Solicitante = solicitante1,
            StatusId = StatusConsts.Aprovada,
            Status = context.StatusSolicitacoes.Local.First(s => s.Id == StatusConsts.Aprovada),
            DataCriacao = new DateTime(2026, 3, 10),
            JustificativaGeral = "Material para aula",
        };

        context.Centros.Add(centro);
        context.Departamentos.AddRange(deptoComputacao, deptoQuimica);
        context.Pessoas.AddRange(pessoa1, pessoa2);
        context.Servidores.AddRange(servidor1, servidor2);
        context.Solicitantes.AddRange(solicitante1, solicitante2);
        context.Categorias.AddRange(categoriaPatrimonial, categoriaGeral);
        context.Items.AddRange(itemPatrimonial, itemGeral);
        context.Solicitacoes.AddRange(
            solicitacaoPatrimonialAtiva,
            solicitacaoPatrimonialRejeitada,
            solicitacaoGeralAtiva
        );

        context.SolicitacaoItens.AddRange(
            new SolicitacaoItem
            {
                SolicitacaoId = 1,
                Solicitacao = solicitacaoPatrimonialAtiva,
                ItemId = 1,
                Item = itemPatrimonial,
                Quantidade = 2,
                ValorUnitario = 100,
                Justificativa = "Necessario para laboratorio",
            },
            new SolicitacaoItem
            {
                SolicitacaoId = 2,
                Solicitacao = solicitacaoPatrimonialRejeitada,
                ItemId = 1,
                Item = itemPatrimonial,
                Quantidade = 1,
                ValorUnitario = 100,
                Justificativa = "Nao aprovado",
            },
            new SolicitacaoItem
            {
                SolicitacaoId = 3,
                Solicitacao = solicitacaoGeralAtiva,
                ItemId = 2,
                Item = itemGeral,
                Quantidade = 4,
                ValorUnitario = 5,
                Justificativa = "Nao deve aparecer",
            }
        );

        await context.SaveChangesAsync();
    }
}
