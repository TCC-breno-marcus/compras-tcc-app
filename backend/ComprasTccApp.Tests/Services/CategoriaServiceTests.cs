using ComprasTccApp.Backend.Models.Entities.Items;
using ComprasTccApp.Models.Entities.Categorias;
using Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.Dtos;
using Moq;
using Services;

namespace ComprasTccApp.Tests.Services;

public class CategoriaServiceTests
{
    [Fact]
    public async Task GetAllCategoriasAsync_DeveRetornarTodasCategorias_QuandoSemFiltros()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetAllCategoriasAsync_DeveRetornarTodasCategorias_QuandoSemFiltros));
        context.Categorias.AddRange(
            CriarCategoria(1, "Informatica", "Equipamentos", true),
            CriarCategoria(2, "Laboratorio", "Reagentes", false)
        );
        await context.SaveChangesAsync();

        var service = CriarService(context);

        // Act
        var resultado = await service.GetAllCategoriasAsync([], [], null, null);

        // Assert
        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllCategoriasAsync_DeveFiltrarPorIdDescricaoEStatus_QuandoFiltrosInformados()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetAllCategoriasAsync_DeveFiltrarPorIdDescricaoEStatus_QuandoFiltrosInformados));
        context.Categorias.AddRange(
            CriarCategoria(1, "Informatica", "Equipamentos de TI", true),
            CriarCategoria(2, "Laboratorio", "Equipamentos de teste", true),
            CriarCategoria(3, "Quimica", "Reagentes", false)
        );
        await context.SaveChangesAsync();

        var service = CriarService(context);

        // Act
        var resultado = await service.GetAllCategoriasAsync(
            [1, 2],
            [],
            "TI",
            true
        );

        // Assert
        resultado.Should().ContainSingle();
        resultado.First().Id.Should().Be(1);
    }

    [Theory]
    [InlineData(true, 2)]
    [InlineData(false, 1)]
    public async Task GetAllCategoriasAsync_DeveFiltrarPorStatus_QuandoIsActiveInformado(
        bool isActive,
        int quantidadeEsperada
    )
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetAllCategoriasAsync_DeveFiltrarPorStatus_QuandoIsActiveInformado) + isActive);
        context.Categorias.AddRange(
            CriarCategoria(1, "Informatica", "Equipamentos", true),
            CriarCategoria(2, "Laboratorio", "Reagentes", true),
            CriarCategoria(3, "Quimica", "Insumos", false)
        );
        await context.SaveChangesAsync();

        var service = CriarService(context);

        // Act
        var resultado = await service.GetAllCategoriasAsync([], [], null, isActive);

        // Assert
        resultado.Should().HaveCount(quantidadeEsperada);
    }

    [Fact]
    public async Task EditarCategoriaAsync_DeveRetornarNull_QuandoCategoriaNaoExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(EditarCategoriaAsync_DeveRetornarNull_QuandoCategoriaNaoExiste));
        var service = CriarService(context);

        // Act
        var resultado = await service.EditarCategoriaAsync(10, new CategoriaUpdateDto { Nome = "Novo Nome" });

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task EditarCategoriaAsync_DeveAtualizarCategoria_QuandoCategoriaExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(EditarCategoriaAsync_DeveAtualizarCategoria_QuandoCategoriaExiste));
        context.Categorias.Add(CriarCategoria(1, "Informatica", "Equipamentos", true));
        await context.SaveChangesAsync();
        var service = CriarService(context);

        // Act
        var resultado = await service.EditarCategoriaAsync(
            1,
            new CategoriaUpdateDto
            {
                Nome = "Tecnologia",
                Descricao = "Equipamentos de TI",
                IsActive = false,
            }
        );

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Nome.Should().Be("Tecnologia");
        resultado.Descricao.Should().Be("Equipamentos de TI");
        resultado.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task GetCategoriaByIdAsync_DeveRetornarNull_QuandoCategoriaNaoExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetCategoriaByIdAsync_DeveRetornarNull_QuandoCategoriaNaoExiste));
        var service = CriarService(context);

        // Act
        var resultado = await service.GetCategoriaByIdAsync(99);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GetCategoriaByIdAsync_DeveRetornarCategoria_QuandoCategoriaExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetCategoriaByIdAsync_DeveRetornarCategoria_QuandoCategoriaExiste));
        context.Categorias.Add(CriarCategoria(1, "Informatica", "Equipamentos", true));
        await context.SaveChangesAsync();
        var service = CriarService(context);

        // Act
        var resultado = await service.GetCategoriaByIdAsync(1);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(1);
        resultado.Nome.Should().Be("Informatica");
    }

    [Fact]
    public async Task CriarCategoriaAsync_DeveLancarExcecao_QuandoNomeDuplicadoNormalizado()
    {
        // Arrange
        await using var context = CriarContexto(nameof(CriarCategoriaAsync_DeveLancarExcecao_QuandoNomeDuplicadoNormalizado));
        context.Categorias.Add(CriarCategoria(1, "informatica", "Equipamentos", true));
        await context.SaveChangesAsync();
        var service = CriarService(context);

        // Act
        var act = () => service.CriarCategoriaAsync(
            new CategoriaDto
            {
                Id = 0,
                Nome = "  INFORMATICA  ",
                Descricao = "Outra descrição",
                IsActive = false,
            }
        );

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CriarCategoriaAsync_DeveCriarCategoriaAtiva_QuandoNomeInedito()
    {
        // Arrange
        await using var context = CriarContexto(nameof(CriarCategoriaAsync_DeveCriarCategoriaAtiva_QuandoNomeInedito));
        var service = CriarService(context);

        // Act
        var resultado = await service.CriarCategoriaAsync(
            new CategoriaDto
            {
                Id = 0,
                Nome = "  Mobiliario  ",
                Descricao = "Moveis",
                IsActive = false,
            }
        );

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().BeGreaterThan(0);
        resultado.Nome.Should().Be("Mobiliario");
        resultado.IsActive.Should().BeTrue();
        context.Categorias.Should().ContainSingle(c => c.Nome == "Mobiliario");
    }

    [Fact]
    public async Task DeleteCategoriaAsync_DeveRetornarFalse_QuandoCategoriaNaoExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(DeleteCategoriaAsync_DeveRetornarFalse_QuandoCategoriaNaoExiste));
        var service = CriarService(context);

        // Act
        var resultado = await service.DeleteCategoriaAsync(1);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteCategoriaAsync_DeveLancarExcecao_QuandoCategoriaEmUso()
    {
        // Arrange
        await using var context = CriarContexto(nameof(DeleteCategoriaAsync_DeveLancarExcecao_QuandoCategoriaEmUso));
        var categoria = CriarCategoria(1, "Informatica", "Equipamentos", true);
        context.Categorias.Add(categoria);
        context.Items.Add(
            new Item
            {
                Nome = "Mouse",
                CatMat = "100",
                Descricao = "Mouse USB",
                LinkImagem = "mouse.png",
                Especificacao = "Optico",
                PrecoSugerido = 10,
                IsActive = true,
                CategoriaId = categoria.Id,
                Categoria = categoria,
            }
        );
        await context.SaveChangesAsync();

        var service = CriarService(context);

        // Act
        var act = () => service.DeleteCategoriaAsync(1);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteCategoriaAsync_DeveRetornarTrue_QuandoCategoriaSemUso()
    {
        // Arrange
        await using var context = CriarContexto(nameof(DeleteCategoriaAsync_DeveRetornarTrue_QuandoCategoriaSemUso));
        context.Categorias.Add(CriarCategoria(1, "Informatica", "Equipamentos", true));
        await context.SaveChangesAsync();
        var service = CriarService(context);

        // Act
        var resultado = await service.DeleteCategoriaAsync(1);

        // Assert
        resultado.Should().BeTrue();
        context.Categorias.Should().BeEmpty();
    }

    private static CategoriaService CriarService(AppDbContext context)
    {
        var loggerMock = new Mock<ILogger<CategoriaService>>();
        return new CategoriaService(context, loggerMock.Object);
    }

    private static AppDbContext CriarContexto(string nomeBanco)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;

        return new AppDbContext(options);
    }

    private static Categoria CriarCategoria(long id, string nome, string descricao, bool isActive)
    {
        return new Categoria
        {
            Id = id,
            Nome = nome,
            Descricao = descricao,
            IsActive = isActive,
        };
    }
}
