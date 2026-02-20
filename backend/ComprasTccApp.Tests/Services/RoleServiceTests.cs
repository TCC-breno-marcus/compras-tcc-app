using ComprasTccApp.Models.Entities.Pessoas;
using Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Services;

namespace ComprasTccApp.Tests.Services;

public class RoleServiceTests
{
    [Theory]
    [InlineData("Admin")]
    [InlineData("Gestor")]
    [InlineData("Solicitante")]
    public async Task AtribuirRoleAsync_DeveAtualizarRoleERetornarTrue_QuandoRoleValidaEUsuarioExiste(string novaRole)
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(AtribuirRoleAsync_DeveAtualizarRoleERetornarTrue_QuandoRoleValidaEUsuarioExiste) + novaRole
        );
        context.Pessoas.Add(CriarPessoa("usuario@universidade.edu", "Solicitante"));
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = await service.AtribuirRoleAsync("usuario@universidade.edu", novaRole);

        // Assert
        resultado.Should().BeTrue();
        var usuarioAtualizado = await context.Pessoas.FirstAsync(p => p.Email == "usuario@universidade.edu");
        usuarioAtualizado.Role.Should().Be(novaRole);
    }

    [Fact]
    public async Task AtribuirRoleAsync_DeveRetornarFalse_QuandoUsuarioNaoExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(AtribuirRoleAsync_DeveRetornarFalse_QuandoUsuarioNaoExiste));
        var service = CriarServico(context);

        // Act
        var resultado = await service.AtribuirRoleAsync("inexistente@universidade.edu", "Gestor");

        // Assert
        resultado.Should().BeFalse();
        context.Pessoas.Should().BeEmpty();
    }

    [Theory]
    [InlineData("Administrador")]
    [InlineData("admin")]
    [InlineData("")]
    public async Task AtribuirRoleAsync_DeveRetornarFalse_QuandoRoleForInvalida(string novaRole)
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(AtribuirRoleAsync_DeveRetornarFalse_QuandoRoleForInvalida) + novaRole
        );
        context.Pessoas.Add(CriarPessoa("usuario@universidade.edu", "Solicitante"));
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = await service.AtribuirRoleAsync("usuario@universidade.edu", novaRole);

        // Assert
        resultado.Should().BeFalse();
        var usuario = await context.Pessoas.FirstAsync(p => p.Email == "usuario@universidade.edu");
        usuario.Role.Should().Be("Solicitante");
    }

    private static RoleService CriarServico(AppDbContext context)
    {
        var loggerMock = new Mock<ILogger<RoleService>>();
        return new RoleService(context, loggerMock.Object);
    }

    private static AppDbContext CriarContexto(string nomeBanco)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;

        return new AppDbContext(options);
    }

    private static Pessoa CriarPessoa(string email, string role)
    {
        return new Pessoa
        {
            Nome = "Pessoa Teste",
            Email = email,
            Telefone = "11999999999",
            CPF = "12345678901",
            Role = role,
            DataAtualizacao = DateTime.UtcNow,
            PasswordHash = "hash",
            IsActive = true,
        };
    }
}
