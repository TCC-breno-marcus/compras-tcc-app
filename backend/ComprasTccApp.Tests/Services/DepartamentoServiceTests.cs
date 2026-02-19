using ComprasTccApp.Models.Entities.Centros;
using ComprasTccApp.Models.Entities.Departamentos;
using Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace ComprasTccApp.Tests.Services;

public class DepartamentoServiceTests
{
    [Fact]
    public async Task GetAllDepartamentosAsync_DeveRetornarTodosOrdenados_QuandoSemFiltros()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetAllDepartamentosAsync_DeveRetornarTodosOrdenados_QuandoSemFiltros));
        var centro = CriarCentro(1, "Centro de Tecnologia", "CT");
        context.Centros.Add(centro);
        context.Departamentos.AddRange(
            CriarDepartamento(1, "Quimica", "DQ", centro),
            CriarDepartamento(2, "Biologia", "DB", centro)
        );
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = (await service.GetAllDepartamentosAsync(null, null, null)).ToList();

        // Assert
        resultado.Should().HaveCount(2);
        resultado[0].Nome.Should().Be("Biologia");
        resultado[1].Nome.Should().Be("Quimica");
    }

    [Fact]
    public async Task GetAllDepartamentosAsync_DeveFiltrarPorNome_QuandoNomeInformado()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetAllDepartamentosAsync_DeveFiltrarPorNome_QuandoNomeInformado));
        var centro = CriarCentro(1, "Centro de Tecnologia", "CT");
        context.Centros.Add(centro);
        context.Departamentos.AddRange(
            CriarDepartamento(1, "Departamento de Computacao", "DC", centro),
            CriarDepartamento(2, "Departamento de Quimica", "DQ", centro)
        );
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = (await service.GetAllDepartamentosAsync("comput", null, null)).ToList();

        // Assert
        resultado.Should().ContainSingle();
        resultado[0].Sigla.Should().Be("DC");
    }

    [Theory]
    [InlineData("dc")]
    [InlineData("DC")]
    [InlineData("Dc")]
    public async Task GetAllDepartamentosAsync_DeveFiltrarPorSiglaCaseInsensitive_QuandoSiglaInformada(
        string sigla
    )
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(GetAllDepartamentosAsync_DeveFiltrarPorSiglaCaseInsensitive_QuandoSiglaInformada)
                + sigla
        );
        var centro = CriarCentro(1, "Centro de Tecnologia", "CT");
        context.Centros.Add(centro);
        context.Departamentos.AddRange(
            CriarDepartamento(1, "Departamento de Computacao", "DC", centro),
            CriarDepartamento(2, "Departamento de Quimica", "DQ", centro)
        );
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = (await service.GetAllDepartamentosAsync(null, sigla, null)).ToList();

        // Assert
        resultado.Should().ContainSingle();
        resultado[0].Nome.Should().Be("Departamento de Computacao");
    }

    [Theory]
    [InlineData("ct")]
    [InlineData("CT")]
    [InlineData("Ct")]
    public async Task GetAllDepartamentosAsync_DeveFiltrarPorSiglaCentroCaseInsensitive_QuandoSiglaCentroInformada(
        string siglaCentro
    )
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(
                GetAllDepartamentosAsync_DeveFiltrarPorSiglaCentroCaseInsensitive_QuandoSiglaCentroInformada
            ) + siglaCentro
        );
        var centroA = CriarCentro(1, "Centro de Tecnologia", "CT");
        var centroB = CriarCentro(2, "Centro de Saude", "CS");
        context.Centros.AddRange(centroA, centroB);
        context.Departamentos.AddRange(
            CriarDepartamento(1, "Departamento de Computacao", "DC", centroA),
            CriarDepartamento(2, "Departamento de Medicina", "DM", centroB)
        );
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = (await service.GetAllDepartamentosAsync(null, null, siglaCentro)).ToList();

        // Assert
        resultado.Should().ContainSingle();
        resultado[0].Centro.Sigla.Should().Be("CT");
    }

    [Fact]
    public async Task GetDepartamentoByIdAsync_DeveRetornarNull_QuandoDepartamentoNaoExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetDepartamentoByIdAsync_DeveRetornarNull_QuandoDepartamentoNaoExiste));
        var service = CriarServico(context);

        // Act
        var resultado = await service.GetDepartamentoByIdAsync(999);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GetDepartamentoByIdAsync_DeveRetornarDepartamentoComCentro_QuandoDepartamentoExiste()
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(GetDepartamentoByIdAsync_DeveRetornarDepartamentoComCentro_QuandoDepartamentoExiste)
        );
        var centro = CriarCentro(1, "Centro de Tecnologia", "CT");
        context.Centros.Add(centro);
        context.Departamentos.Add(CriarDepartamento(1, "Departamento de Computacao", "DC", centro));
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = await service.GetDepartamentoByIdAsync(1);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(1);
        resultado.Nome.Should().Be("Departamento de Computacao");
        resultado.Centro.Sigla.Should().Be("CT");
    }

    private static DepartamentoService CriarServico(AppDbContext context)
    {
        var loggerMock = new Mock<ILogger<DepartamentoService>>();
        return new DepartamentoService(context, loggerMock.Object);
    }

    private static AppDbContext CriarContexto(string nomeBanco)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;
        return new AppDbContext(options);
    }

    private static Centro CriarCentro(int id, string nome, string sigla)
    {
        return new Centro
        {
            Id = id,
            Nome = nome,
            Sigla = sigla,
            Email = $"{sigla.ToLowerInvariant()}@universidade.edu",
            Telefone = "1133334444",
        };
    }

    private static Departamento CriarDepartamento(int id, string nome, string sigla, Centro centro)
    {
        return new Departamento
        {
            Id = id,
            Nome = nome,
            Sigla = sigla,
            Email = $"{sigla.ToLowerInvariant()}@universidade.edu",
            Telefone = "1144445555",
            CentroId = centro.Id,
            Centro = centro,
        };
    }
}
