using ComprasTccApp.Models.Entities.Centros;
using Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.Dtos;
using Moq;

namespace ComprasTccApp.Tests.Services;

public class CentroServiceTests
{
    [Fact]
    public async Task GetAllCentrosAsync_DeveRetornarTodosOrdenados_QuandoSemFiltros()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetAllCentrosAsync_DeveRetornarTodosOrdenados_QuandoSemFiltros));
        context.Centros.AddRange(
            CriarCentro(1, "Zoologia", "ZOO"),
            CriarCentro(2, "Administracao", "ADM")
        );
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = (await service.GetAllCentrosAsync(null, null)).ToList();

        // Assert
        resultado.Should().HaveCount(2);
        resultado[0].Nome.Should().Be("Administracao");
        resultado[1].Nome.Should().Be("Zoologia");
    }

    [Fact]
    public async Task GetAllCentrosAsync_DeveFiltrarPorNome_QuandoNomeInformado()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetAllCentrosAsync_DeveFiltrarPorNome_QuandoNomeInformado));
        context.Centros.AddRange(
            CriarCentro(1, "Centro de Ciencias Exatas", "CCE"),
            CriarCentro(2, "Centro de Artes", "CART")
        );
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = (await service.GetAllCentrosAsync("ciencias", null)).ToList();

        // Assert
        resultado.Should().ContainSingle();
        resultado[0].Sigla.Should().Be("CCE");
    }

    [Theory]
    [InlineData("cce")]
    [InlineData("CCE")]
    [InlineData("CcE")]
    public async Task GetAllCentrosAsync_DeveFiltrarPorSiglaCaseInsensitive_QuandoSiglaInformada(
        string sigla
    )
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetAllCentrosAsync_DeveFiltrarPorSiglaCaseInsensitive_QuandoSiglaInformada) + sigla);
        context.Centros.AddRange(
            CriarCentro(1, "Centro de Ciencias Exatas", "CCE"),
            CriarCentro(2, "Centro de Artes", "CART")
        );
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = (await service.GetAllCentrosAsync(null, sigla)).ToList();

        // Assert
        resultado.Should().ContainSingle();
        resultado[0].Nome.Should().Be("Centro de Ciencias Exatas");
    }

    [Fact]
    public async Task GetCentroByIdAsync_DeveRetornarNull_QuandoCentroNaoExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetCentroByIdAsync_DeveRetornarNull_QuandoCentroNaoExiste));
        var service = CriarServico(context);

        // Act
        var resultado = await service.GetCentroByIdAsync(999);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GetCentroByIdAsync_DeveRetornarCentro_QuandoCentroExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetCentroByIdAsync_DeveRetornarCentro_QuandoCentroExiste));
        context.Centros.Add(CriarCentro(1, "Centro de Ciencias Exatas", "CCE"));
        await context.SaveChangesAsync();
        var service = CriarServico(context);

        // Act
        var resultado = await service.GetCentroByIdAsync(1);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(1);
        resultado.Nome.Should().Be("Centro de Ciencias Exatas");
        resultado.Sigla.Should().Be("CCE");
    }

    private static CentroService CriarServico(AppDbContext context)
    {
        var loggerMock = new Mock<ILogger<DepartamentoService>>();
        return new CentroService(context, loggerMock.Object);
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
}
