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
using Services;
using System.Text;

namespace ComprasTccApp.Tests.Services;

public class DadosPublicosServiceTests
{
    [Fact]
    public async Task ConsultarSolicitacoesAsync_DeveRetornarDadosMascarados_QuandoConsultaPublica()
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(ConsultarSolicitacoesAsync_DeveRetornarDadosMascarados_QuandoConsultaPublica)
        );
        await SeedDadosAsync(context);
        var service = CriarServico(context);

        // Act
        var resultado = await service.ConsultarSolicitacoesAsync(
            dataInicio: null,
            dataFim: null,
            statusId: null,
            statusNome: null,
            siglaDepartamento: null,
            categoriaNome: null,
            itemNome: null,
            catMat: null,
            itemsType: null,
            valorMinimo: null,
            valorMaximo: null,
            somenteSolicitacoesAtivas: null,
            pageNumber: 1,
            pageSize: 10
        );

        // Assert
        resultado.TotalCount.Should().Be(2);
        resultado.Data.Should().NotBeEmpty();

        var solicitacao = resultado.Data.First();
        solicitacao.SolicitanteEmailMascarado.Should().NotBe("ana.silva@universidade.edu");
        solicitacao.SolicitanteTelefoneMascarado.Should().NotBe("11999998888");
        solicitacao.SolicitanteCpfMascarado.Should().MatchRegex(@"\*\*\*\.\*\*\*\.\*\*\*-\d{2}");
    }

    [Fact]
    public async Task ConsultarSolicitacoesAsync_DeveFiltrarPorTipoStatusEDepartamento_QuandoFiltrosInformados()
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(ConsultarSolicitacoesAsync_DeveFiltrarPorTipoStatusEDepartamento_QuandoFiltrosInformados)
        );
        await SeedDadosAsync(context);
        var service = CriarServico(context);

        // Act
        var resultado = await service.ConsultarSolicitacoesAsync(
            dataInicio: new DateTime(2026, 1, 1),
            dataFim: new DateTime(2026, 2, 1),
            statusId: StatusConsts.Pendente,
            statusNome: "pendente",
            siglaDepartamento: "DC",
            categoriaNome: "Mobiliário",
            itemNome: "Armario",
            catMat: "P100",
            itemsType: "patrimonial",
            valorMinimo: 100m,
            valorMaximo: 500m,
            somenteSolicitacoesAtivas: true,
            pageNumber: 1,
            pageSize: 10
        );

        // Assert
        resultado.TotalCount.Should().Be(1);
        resultado.Data.Should().ContainSingle();
        resultado.Data[0].TipoSolicitacao.Should().Be("PATRIMONIAL");
        resultado.Data[0].StatusId.Should().Be(StatusConsts.Pendente);
        resultado.Data[0].DepartamentoSigla.Should().Be("DC");
        resultado.Data[0].Itens.Should().ContainSingle(i => i.ItemNome == "Armario");
    }

    [Fact]
    public async Task ExportarSolicitacoesCsvAsync_DeveGerarCsvComDadosMascarados_QuandoConsultaPublica()
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(ExportarSolicitacoesCsvAsync_DeveGerarCsvComDadosMascarados_QuandoConsultaPublica)
        );
        await SeedDadosAsync(context);
        var service = CriarServico(context);

        // Act
        var bytes = await service.ExportarSolicitacoesCsvAsync(
            dataInicio: null,
            dataFim: null,
            statusId: null,
            statusNome: null,
            siglaDepartamento: null,
            categoriaNome: null,
            itemNome: null,
            catMat: null,
            itemsType: null,
            valorMinimo: null,
            valorMaximo: null,
            somenteSolicitacoesAtivas: null,
            pageNumber: 1,
            pageSize: 10
        );
        var csv = Encoding.UTF8.GetString(bytes);

        // Assert
        bytes.Should().NotBeEmpty();
        csv.Should().Contain("SolicitacaoId");
        csv.Should().Contain("Armario");
        csv.Should().Contain("Resistor");
        csv.Should().Contain("an***@universidade.edu");
        csv.Should().NotContain("ana.silva@universidade.edu");
    }

    private static DadosPublicosService CriarServico(AppDbContext context)
    {
        var loggerMock = new Mock<ILogger<DadosPublicosService>>();
        return new DadosPublicosService(context, loggerMock.Object);
    }

    private static AppDbContext CriarContexto(string nomeBanco)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;
        return new AppDbContext(options);
    }

    private static async Task SeedDadosAsync(AppDbContext context)
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
            Nome = "Ana Silva",
            Email = "ana.silva@universidade.edu",
            Telefone = "11999998888",
            CPF = "12345678901",
            DataAtualizacao = DateTime.UtcNow,
            PasswordHash = "hash",
            Role = "Solicitante",
            IsActive = true,
        };
        var pessoa2 = new Pessoa
        {
            Id = 2,
            Nome = "Bruno Costa",
            Email = "bruno.costa@universidade.edu",
            Telefone = "11999997777",
            CPF = "98765432100",
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

        var solicitacaoPatrimonial = new SolicitacaoPatrimonial
        {
            Id = 1,
            SolicitanteId = 1,
            Solicitante = solicitante1,
            StatusId = StatusConsts.Pendente,
            Status = context.StatusSolicitacoes.Local.First(s => s.Id == StatusConsts.Pendente),
            DataCriacao = new DateTime(2026, 1, 10),
            ExternalId = "SOL-2026-0001",
        };

        var solicitacaoGeral = new SolicitacaoGeral
        {
            Id = 2,
            SolicitanteId = 2,
            Solicitante = solicitante2,
            StatusId = StatusConsts.Aprovada,
            Status = context.StatusSolicitacoes.Local.First(s => s.Id == StatusConsts.Aprovada),
            DataCriacao = new DateTime(2026, 3, 10),
            ExternalId = "SOL-2026-0002",
            JustificativaGeral = "Material de aula",
        };

        context.Centros.Add(centro);
        context.Departamentos.AddRange(deptoComputacao, deptoQuimica);
        context.Pessoas.AddRange(pessoa1, pessoa2);
        context.Servidores.AddRange(servidor1, servidor2);
        context.Solicitantes.AddRange(solicitante1, solicitante2);
        context.Categorias.AddRange(categoriaPatrimonial, categoriaGeral);
        context.Items.AddRange(itemPatrimonial, itemGeral);
        context.Solicitacoes.AddRange(solicitacaoPatrimonial, solicitacaoGeral);

        context.SolicitacaoItens.AddRange(
            new SolicitacaoItem
            {
                SolicitacaoId = 1,
                Solicitacao = solicitacaoPatrimonial,
                ItemId = 1,
                Item = itemPatrimonial,
                Quantidade = 2,
                ValorUnitario = 100,
                Justificativa = "Uso em laboratorio",
            },
            new SolicitacaoItem
            {
                SolicitacaoId = 2,
                Solicitacao = solicitacaoGeral,
                ItemId = 2,
                Item = itemGeral,
                Quantidade = 4,
                ValorUnitario = 5,
                Justificativa = null,
            }
        );

        await context.SaveChangesAsync();
    }
}
