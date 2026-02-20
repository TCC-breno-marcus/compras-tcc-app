using System.Security.Claims;
using ComprasTccApp.Backend.Services;
using ComprasTccApp.Models.Entities.Centros;
using ComprasTccApp.Models.Entities.Departamentos;
using ComprasTccApp.Models.Entities.Gestores;
using ComprasTccApp.Models.Entities.Pessoas;
using ComprasTccApp.Models.Entities.Servidores;
using ComprasTccApp.Models.Entities.Solicitantes;
using ComprasTccApp.Services.Interfaces;
using Database;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Models.Dtos;
using Moq;

namespace ComprasTccApp.Tests.Services;

public class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_DeveRegistrarSolicitante_QuandoDadosValidos()
    {
        // Arrange
        await using var context = CriarContexto(nameof(RegisterAsync_DeveRegistrarSolicitante_QuandoDadosValidos));
        context.Departamentos.Add(
            new Departamento
            {
                Id = 1,
                Nome = "Departamento de Tecnologia",
                Sigla = "DTEC",
                Email = "dtec@universidade.edu",
                Telefone = "11999999999",
                CentroId = 10,
            }
        );
        await context.SaveChangesAsync();

        var service = CriarServico(context);
        var dto = new RegisterDto
        {
            Nome = "Maria Silva",
            Email = "maria.silva@universidade.edu",
            Telefone = "11988887777",
            CPF = "12345678901",
            Password = "Senha@123",
            Role = "Solicitante",
            DepartamentoSigla = "DTEC",
        };

        // Act
        var pessoaCriada = await service.RegisterAsync(dto);

        // Assert
        pessoaCriada.Should().NotBeNull();
        pessoaCriada.Role.Should().Be("Solicitante");
        pessoaCriada.PasswordHash.Should().NotBe(dto.Password);
        BCrypt.Net.BCrypt.Verify(dto.Password, pessoaCriada.PasswordHash).Should().BeTrue();

        context.Pessoas.Should().ContainSingle(p => p.Email == dto.Email);
        context.Solicitantes.Should().ContainSingle();
        context.Gestores.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterAsync_DeveRegistrarGestor_QuandoDadosValidos()
    {
        // Arrange
        await using var context = CriarContexto(nameof(RegisterAsync_DeveRegistrarGestor_QuandoDadosValidos));
        context.Centros.Add(
            new Centro
            {
                Id = 1,
                Nome = "Centro de Ciencias Exatas",
                Sigla = "CCE",
                Email = "cce@universidade.edu",
                Telefone = "11888887777",
            }
        );
        await context.SaveChangesAsync();

        var service = CriarServico(context);
        var dto = new RegisterDto
        {
            Nome = "Carlos Lima",
            Email = "carlos.lima@universidade.edu",
            Telefone = "11777776666",
            CPF = "98765432100",
            Password = "Senha@123",
            Role = "Gestor",
            CentroSigla = "CCE",
        };

        // Act
        var pessoaCriada = await service.RegisterAsync(dto);

        // Assert
        pessoaCriada.Should().NotBeNull();
        pessoaCriada.Role.Should().Be("Gestor");
        context.Pessoas.Should().ContainSingle(p => p.Email == dto.Email);
        context.Gestores.Should().ContainSingle();
        context.Solicitantes.Should().BeEmpty();
    }

    [Fact]
    public async Task RegisterAsync_DeveLancarExcecao_QuandoEmailJaExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(RegisterAsync_DeveLancarExcecao_QuandoEmailJaExiste));
        context.Pessoas.Add(CriarPessoa(email: "email.repetido@universidade.edu", cpf: "11111111111"));
        context.Centros.Add(
            new Centro
            {
                Id = 1,
                Nome = "Centro Teste",
                Sigla = "CT",
                Email = "ct@universidade.edu",
                Telefone = "11555554444",
            }
        );
        await context.SaveChangesAsync();

        var service = CriarServico(context);
        var dto = new RegisterDto
        {
            Nome = "Novo Usuario",
            Email = "email.repetido@universidade.edu",
            Telefone = "11444443333",
            CPF = "22222222222",
            Password = "Senha@123",
            Role = "Gestor",
            CentroSigla = "CT",
        };

        // Act
        var act = () => service.RegisterAsync(dto);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("*email já está em uso*");
    }

    [Fact]
    public async Task RegisterAsync_DeveLancarExcecao_QuandoCpfJaExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(RegisterAsync_DeveLancarExcecao_QuandoCpfJaExiste));
        context.Pessoas.Add(CriarPessoa(email: "existente@universidade.edu", cpf: "11111111111"));
        context.Centros.Add(
            new Centro
            {
                Id = 1,
                Nome = "Centro Teste",
                Sigla = "CT",
                Email = "ct@universidade.edu",
                Telefone = "11555554444",
            }
        );
        await context.SaveChangesAsync();

        var service = CriarServico(context);
        var dto = new RegisterDto
        {
            Nome = "Novo Usuario",
            Email = "novo@universidade.edu",
            Telefone = "11444443333",
            CPF = "11111111111",
            Password = "Senha@123",
            Role = "Gestor",
            CentroSigla = "CT",
        };

        // Act
        var act = () => service.RegisterAsync(dto);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("*CPF já está em uso*");
    }

    [Theory]
    [InlineData("Solicitante", "departamento")]
    [InlineData("Gestor", "centro")]
    public async Task RegisterAsync_DeveLancarArgumentException_QuandoSiglaObrigatoriaNaoInformada(
        string role,
        string trechoEsperado
    )
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(RegisterAsync_DeveLancarArgumentException_QuandoSiglaObrigatoriaNaoInformada) + role
        );
        await context.SaveChangesAsync();
        var service = CriarServico(context);
        var dto = new RegisterDto
        {
            Nome = "Usuario Sem Sigla",
            Email = $"{role.ToLowerInvariant()}@universidade.edu",
            Telefone = "11444443333",
            CPF = role == "Solicitante" ? "33333333333" : "44444444444",
            Password = "Senha@123",
            Role = role,
        };

        // Act
        var act = () => service.RegisterAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>().WithMessage($"*{trechoEsperado}*");
    }

    [Theory]
    [InlineData("Solicitante")]
    [InlineData("Gestor")]
    public async Task RegisterAsync_DeveLancarExcecao_QuandoUnidadeOrganizacionalNaoExiste(string role)
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(RegisterAsync_DeveLancarExcecao_QuandoUnidadeOrganizacionalNaoExiste) + role
        );
        var service = CriarServico(context);

        var dto = new RegisterDto
        {
            Nome = "Usuario Sem Unidade",
            Email = $"{role.ToLowerInvariant()}.sem.unidade@universidade.edu",
            Telefone = "11333332222",
            CPF = role == "Solicitante" ? "55555555555" : "66666666666",
            Password = "Senha@123",
            Role = role,
            DepartamentoSigla = role == "Solicitante" ? "NAOEXISTE" : null,
            CentroSigla = role == "Gestor" ? "NAOEXISTE" : null,
        };

        // Act
        var act = () => service.RegisterAsync(dto);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("*não encontrado*");
    }

    [Fact]
    public async Task LoginAsync_DeveRetornarToken_QuandoCredenciaisValidas()
    {
        // Arrange
        await using var context = CriarContexto(nameof(LoginAsync_DeveRetornarToken_QuandoCredenciaisValidas));
        var pessoa = CriarPessoa(
            email: "login.ok@universidade.edu",
            cpf: "77777777777",
            senha: "Senha@123"
        );
        context.Pessoas.Add(pessoa);
        await context.SaveChangesAsync();

        var tokenServiceMock = new Mock<ITokenService>();
        tokenServiceMock.Setup(t => t.GenerateJwtToken(It.IsAny<Pessoa>())).Returns("jwt.token.teste");

        var service = CriarServico(context, tokenServiceMock);
        var dto = new LoginDto { Email = "login.ok@universidade.edu", Password = "Senha@123" };

        // Act
        var resultado = await service.LoginAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Token.Should().Be("jwt.token.teste");
        resultado.Message.Should().Be("Login bem-sucedido!");
        tokenServiceMock.Verify(t => t.GenerateJwtToken(It.IsAny<Pessoa>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_DeveRetornarNull_QuandoEmailNaoEncontrado()
    {
        // Arrange
        await using var context = CriarContexto(nameof(LoginAsync_DeveRetornarNull_QuandoEmailNaoEncontrado));
        var tokenServiceMock = new Mock<ITokenService>();
        var service = CriarServico(context, tokenServiceMock);
        var dto = new LoginDto { Email = "inexistente@universidade.edu", Password = "Senha@123" };

        // Act
        var resultado = await service.LoginAsync(dto);

        // Assert
        resultado.Should().BeNull();
        tokenServiceMock.Verify(t => t.GenerateJwtToken(It.IsAny<Pessoa>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_DeveRetornarNull_QuandoSenhaInvalida()
    {
        // Arrange
        await using var context = CriarContexto(nameof(LoginAsync_DeveRetornarNull_QuandoSenhaInvalida));
        context.Pessoas.Add(
            CriarPessoa(email: "usuario@universidade.edu", cpf: "88888888888", senha: "Senha@123")
        );
        await context.SaveChangesAsync();

        var tokenServiceMock = new Mock<ITokenService>();
        var service = CriarServico(context, tokenServiceMock);
        var dto = new LoginDto { Email = "usuario@universidade.edu", Password = "SenhaErrada@123" };

        // Act
        var resultado = await service.LoginAsync(dto);

        // Assert
        resultado.Should().BeNull();
        tokenServiceMock.Verify(t => t.GenerateJwtToken(It.IsAny<Pessoa>()), Times.Never);
    }

    [Fact]
    public async Task LoginAsync_DeveLancarInvalidOperationException_QuandoUsuarioInativo()
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(LoginAsync_DeveLancarInvalidOperationException_QuandoUsuarioInativo)
        );
        var pessoa = CriarPessoa(email: "inativo@universidade.edu", cpf: "99999999999", senha: "Senha@123");
        pessoa.IsActive = false;
        context.Pessoas.Add(pessoa);
        await context.SaveChangesAsync();

        var service = CriarServico(context);
        var dto = new LoginDto { Email = "inativo@universidade.edu", Password = "Senha@123" };

        // Act
        var act = () => service.LoginAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetMyProfileAsync_DeveRetornarNull_QuandoClaimNameIdentifierForInvalido()
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(GetMyProfileAsync_DeveRetornarNull_QuandoClaimNameIdentifierForInvalido)
        );
        var service = CriarServico(context);
        var principal = new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "abc")], "Teste")
        );

        // Act
        var resultado = await service.GetMyProfileAsync(principal);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GetMyProfileAsync_DeveRetornarNull_QuandoPessoaNaoEncontrada()
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(GetMyProfileAsync_DeveRetornarNull_QuandoPessoaNaoEncontrada)
        );
        var service = CriarServico(context);
        var principal = CriarPrincipalComId(123);

        // Act
        var resultado = await service.GetMyProfileAsync(principal);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GetMyProfileAsync_DeveRetornarPerfilComDepartamento_QuandoUsuarioForSolicitante()
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(GetMyProfileAsync_DeveRetornarPerfilComDepartamento_QuandoUsuarioForSolicitante)
        );

        var departamento = new Departamento
        {
            Id = 1,
            Nome = "Departamento de Quimica",
            Sigla = "DQ",
            Email = "dq@universidade.edu",
            Telefone = "1130303030",
            CentroId = 9,
        };
        var pessoa = CriarPessoa(email: "solicitante@universidade.edu", cpf: "10101010101", role: "Solicitante");
        var servidor = new Servidor
        {
            Pessoa = pessoa,
            IdentificadorInterno = "TEMP-10101010101",
            IsGestor = false,
        };
        var solicitante = new Solicitante
        {
            Servidor = servidor,
            Departamento = departamento,
            DataUltimaSolicitacao = DateTime.UtcNow,
        };

        context.Solicitantes.Add(solicitante);
        await context.SaveChangesAsync();

        var service = CriarServico(context);
        var principal = CriarPrincipalComId(pessoa.Id);

        // Act
        var resultado = await service.GetMyProfileAsync(principal);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Role.Should().Be("Solicitante");
        resultado.Unidade.Should().NotBeNull();
        resultado.Unidade!.Tipo.Should().Be("Departamento");
        resultado.Unidade.Sigla.Should().Be("DQ");
    }

    [Fact]
    public async Task GetMyProfileAsync_DeveRetornarPerfilComCentro_QuandoUsuarioForGestor()
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(GetMyProfileAsync_DeveRetornarPerfilComCentro_QuandoUsuarioForGestor)
        );

        var centro = new Centro
        {
            Id = 1,
            Nome = "Centro de Humanidades",
            Sigla = "CH",
            Email = "ch@universidade.edu",
            Telefone = "1140404040",
        };
        var pessoa = CriarPessoa(email: "gestor@universidade.edu", cpf: "20202020202", role: "Gestor");
        var servidor = new Servidor
        {
            Pessoa = pessoa,
            IdentificadorInterno = "TEMP-20202020202",
            IsGestor = false,
        };
        var gestor = new Gestor
        {
            Servidor = servidor,
            Centro = centro,
            DataUltimaSolicitacao = DateTime.UtcNow,
        };

        context.Gestores.Add(gestor);
        await context.SaveChangesAsync();

        var service = CriarServico(context);
        var principal = CriarPrincipalComId(pessoa.Id);

        // Act
        var resultado = await service.GetMyProfileAsync(principal);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Role.Should().Be("Gestor");
        resultado.Unidade.Should().NotBeNull();
        resultado.Unidade!.Tipo.Should().Be("Centro");
        resultado.Unidade.Sigla.Should().Be("CH");
    }

    private static AuthService CriarServico(AppDbContext context, Mock<ITokenService>? tokenServiceMock = null)
    {
        tokenServiceMock ??= new Mock<ITokenService>();
        var loggerMock = new Mock<ILogger<AuthService>>();
        return new AuthService(context, tokenServiceMock.Object, loggerMock.Object);
    }

    private static AppDbContext CriarContexto(string nomeBanco)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;

        return new AppDbContext(options);
    }

    private static ClaimsPrincipal CriarPrincipalComId(long userId)
    {
        return new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, userId.ToString())], "Teste")
        );
    }

    private static Pessoa CriarPessoa(
        string email,
        string cpf,
        string role = "Gestor",
        string senha = "SenhaPadrao@123"
    )
    {
        return new Pessoa
        {
            Nome = "Pessoa Teste",
            Email = email,
            Telefone = "11911112222",
            CPF = cpf,
            Role = role,
            DataAtualizacao = DateTime.UtcNow,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(senha),
            IsActive = true,
        };
    }
}
