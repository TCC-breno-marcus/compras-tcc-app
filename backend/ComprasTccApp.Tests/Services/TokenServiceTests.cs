using System.IdentityModel.Tokens.Jwt;
using ComprasTccApp.Models.Entities.Pessoas;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Services;

namespace ComprasTccApp.Tests.Services;

public class TokenServiceTests
{
    [Fact]
    public void GenerateJwtToken_DeveGerarTokenValido_QuandoPessoaEConfiguracoesValidas()
    {
        // Arrange
        var pessoa = CriarPessoa();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock
            .Setup(c => c["Jwt:Key"])
            .Returns("minha-chave-super-secreta-com-tamanho-suficiente-para-hmac-sha512-2026-token-app");
        configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("ComprasTccApp");
        configurationMock.Setup(c => c["Jwt:Audience"]).Returns("ComprasTccApp.Client");
        var service = new TokenService(configurationMock.Object);

        // Act
        var token = service.GenerateJwtToken(pessoa);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        jwt.Issuer.Should().Be("ComprasTccApp");
        jwt.Audiences.Should().ContainSingle(a => a == "ComprasTccApp.Client");
        jwt.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == pessoa.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == "unique_name" && c.Value == pessoa.Nome);
        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == pessoa.Email);
        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == pessoa.Role);
        jwt.ValidTo.Should().BeAfter(DateTime.UtcNow.AddHours(7));
        jwt.ValidTo.Should().BeBefore(DateTime.UtcNow.AddHours(9));
    }

    [Fact]
    public void GenerateJwtToken_DeveLancarArgumentNullException_QuandoJwtKeyNaoConfigurada()
    {
        // Arrange
        var pessoa = CriarPessoa();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Jwt:Key"]).Returns((string?)null);
        configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("ComprasTccApp");
        configurationMock.Setup(c => c["Jwt:Audience"]).Returns("ComprasTccApp.Client");
        var service = new TokenService(configurationMock.Object);

        // Act
        var act = () => service.GenerateJwtToken(pessoa);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("curta")]
    public void GenerateJwtToken_DeveLancarArgumentException_QuandoJwtKeyInvalida(string jwtKey)
    {
        // Arrange
        var pessoa = CriarPessoa();
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(c => c["Jwt:Key"]).Returns(jwtKey);
        configurationMock.Setup(c => c["Jwt:Issuer"]).Returns("ComprasTccApp");
        configurationMock.Setup(c => c["Jwt:Audience"]).Returns("ComprasTccApp.Client");
        var service = new TokenService(configurationMock.Object);

        // Act
        var act = () => service.GenerateJwtToken(pessoa);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    private static Pessoa CriarPessoa()
    {
        return new Pessoa
        {
            Id = 10,
            Nome = "Maria Silva",
            Email = "maria.silva@universidade.edu",
            Telefone = "11999999999",
            CPF = "12345678901",
            DataAtualizacao = DateTime.UtcNow,
            PasswordHash = "hash",
            Role = "Solicitante",
            IsActive = true,
        };
    }
}
