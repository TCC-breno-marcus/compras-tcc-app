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
using Moq;

namespace ComprasTccApp.Tests.Services;

public class UsuarioServiceTests
{
    [Theory]
    [InlineData("asc", "Ana")]
    [InlineData("desc", "Carlos")]
    public async Task GetAllUsersAsync_DeveOrdenarPorNome_QuandoSortOrderInformado(
        string sortOrder,
        string primeiroNomeEsperado
    )
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(GetAllUsersAsync_DeveOrdenarPorNome_QuandoSortOrderInformado) + sortOrder
        );
        await SeedUsuariosAsync(context);
        var service = CriarServico(context);

        // Act
        var resultado = await service.GetAllUsersAsync(
            role: null,
            pageNumber: 1,
            pageSize: 10,
            sortOrder: sortOrder,
            isActive: null
        );

        // Assert
        resultado.TotalCount.Should().Be(3);
        resultado.Data.Should().HaveCount(3);
        resultado.Data[0].Nome.Should().Be(primeiroNomeEsperado);
    }

    [Fact]
    public async Task GetAllUsersAsync_DeveFiltrarPorRoleEAtivo_QuandoFiltrosValidos()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetAllUsersAsync_DeveFiltrarPorRoleEAtivo_QuandoFiltrosValidos));
        await SeedUsuariosAsync(context);
        var service = CriarServico(context);

        // Act
        var resultado = await service.GetAllUsersAsync(
            role: "Solicitante",
            pageNumber: 1,
            pageSize: 10,
            sortOrder: "asc",
            isActive: true
        );

        // Assert
        resultado.TotalCount.Should().Be(1);
        resultado.Data.Should().ContainSingle();
        resultado.Data[0].Role.Should().Be("Solicitante");
        resultado.Data[0].Unidade.Should().NotBeNull();
        resultado.Data[0].Unidade!.Tipo.Should().Be("Departamento");
    }

    [Fact]
    public async Task GetAllUsersAsync_DeveIgnorarFiltro_QuandoRoleInvalida()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetAllUsersAsync_DeveIgnorarFiltro_QuandoRoleInvalida));
        await SeedUsuariosAsync(context);
        var service = CriarServico(context);

        // Act
        var resultado = await service.GetAllUsersAsync(
            role: "RoleInexistente",
            pageNumber: 1,
            pageSize: 10,
            sortOrder: "asc",
            isActive: null
        );

        // Assert
        resultado.TotalCount.Should().Be(3);
        resultado.Data.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetSolicitanteInfoAsync_DeveRetornarServidorESolicitante_QuandoPessoaExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetSolicitanteInfoAsync_DeveRetornarServidorESolicitante_QuandoPessoaExiste));
        var ids = await SeedUsuariosAsync(context);
        var service = CriarServico(context);

        // Act
        var (servidor, solicitante) = await service.GetSolicitanteInfoAsync(ids.PessoaSolicitanteId);

        // Assert
        servidor.Should().NotBeNull();
        solicitante.Should().NotBeNull();
        servidor.PessoaId.Should().Be(ids.PessoaSolicitanteId);
        solicitante.ServidorId.Should().Be(servidor.Id);
    }

    [Fact]
    public async Task GetSolicitanteInfoAsync_DeveLancarExcecao_QuandoServidorNaoExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetSolicitanteInfoAsync_DeveLancarExcecao_QuandoServidorNaoExiste));
        var service = CriarServico(context);

        // Act
        var act = () => service.GetSolicitanteInfoAsync(999);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("*não encontrada na tabela Servidor*");
    }

    [Fact]
    public async Task GetSolicitanteInfoAsync_DeveLancarExcecao_QuandoSolicitanteNaoExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetSolicitanteInfoAsync_DeveLancarExcecao_QuandoSolicitanteNaoExiste));
        var pessoa = CriarPessoa(10, "Pessoa Sem Solicitante", "Gestor", true);
        var servidor = new Servidor
        {
            Id = 10,
            PessoaId = pessoa.Id,
            Pessoa = pessoa,
            IdentificadorInterno = "TEMP-10",
            IsGestor = false,
        };
        context.Pessoas.Add(pessoa);
        context.Servidores.Add(servidor);
        await context.SaveChangesAsync();
        var service = CriarServico(context);

        // Act
        var act = () => service.GetSolicitanteInfoAsync(pessoa.Id);

        // Assert
        await act.Should().ThrowAsync<Exception>().WithMessage("*não encontrado na tabela Solicitante*");
    }

    [Fact]
    public async Task InativarUsuarioAsync_DeveRetornarTrueEInativar_QuandoUsuarioExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(InativarUsuarioAsync_DeveRetornarTrueEInativar_QuandoUsuarioExiste));
        var ids = await SeedUsuariosAsync(context);
        var service = CriarServico(context);

        // Act
        var resultado = await service.InativarUsuarioAsync(ids.PessoaGestorId);

        // Assert
        resultado.Should().BeTrue();
        var pessoa = await context.Pessoas.FindAsync(ids.PessoaGestorId);
        pessoa!.IsActive.Should().BeFalse();
    }

    [Fact]
    public async Task InativarUsuarioAsync_DeveRetornarFalse_QuandoUsuarioNaoExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(InativarUsuarioAsync_DeveRetornarFalse_QuandoUsuarioNaoExiste));
        var service = CriarServico(context);

        // Act
        var resultado = await service.InativarUsuarioAsync(999);

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task AtivarUsuarioAsync_DeveRetornarTrueEAtivar_QuandoUsuarioExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(AtivarUsuarioAsync_DeveRetornarTrueEAtivar_QuandoUsuarioExiste));
        var ids = await SeedUsuariosAsync(context);
        var service = CriarServico(context);

        // Act
        var resultado = await service.AtivarUsuarioAsync(ids.PessoaAdminId);

        // Assert
        resultado.Should().BeTrue();
        var pessoa = await context.Pessoas.FindAsync(ids.PessoaAdminId);
        pessoa!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task AtivarUsuarioAsync_DeveRetornarFalse_QuandoUsuarioNaoExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(AtivarUsuarioAsync_DeveRetornarFalse_QuandoUsuarioNaoExiste));
        var service = CriarServico(context);

        // Act
        var resultado = await service.AtivarUsuarioAsync(999);

        // Assert
        resultado.Should().BeFalse();
    }

    private static UsuarioService CriarServico(AppDbContext context)
    {
        var tokenServiceMock = new Mock<ITokenService>();
        var loggerMock = new Mock<ILogger<UsuarioService>>();
        return new UsuarioService(context, tokenServiceMock.Object, loggerMock.Object);
    }

    private static AppDbContext CriarContexto(string nomeBanco)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;
        return new AppDbContext(options);
    }

    private static async Task<(long PessoaSolicitanteId, long PessoaGestorId, long PessoaAdminId)> SeedUsuariosAsync(
        AppDbContext context
    )
    {
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
            CentroId = centro.Id,
            Centro = centro,
        };

        var solicitantePessoa = CriarPessoa(1, "Ana", "Solicitante", true);
        var gestorPessoa = CriarPessoa(2, "Bruno", "Gestor", true);
        var adminPessoa = CriarPessoa(3, "Carlos", "Admin", false);

        var solicitanteServidor = new Servidor
        {
            Id = 1,
            PessoaId = solicitantePessoa.Id,
            Pessoa = solicitantePessoa,
            IdentificadorInterno = "TEMP-1",
            IsGestor = false,
        };

        var gestorServidor = new Servidor
        {
            Id = 2,
            PessoaId = gestorPessoa.Id,
            Pessoa = gestorPessoa,
            IdentificadorInterno = "TEMP-2",
            IsGestor = true,
        };

        var adminServidor = new Servidor
        {
            Id = 3,
            PessoaId = adminPessoa.Id,
            Pessoa = adminPessoa,
            IdentificadorInterno = "TEMP-3",
            IsGestor = true,
        };

        var solicitante = new Solicitante
        {
            Id = 1,
            ServidorId = solicitanteServidor.Id,
            Servidor = solicitanteServidor,
            DepartamentoId = departamento.Id,
            Departamento = departamento,
            DataUltimaSolicitacao = DateTime.UtcNow,
        };

        var gestor = new Gestor
        {
            Id = 1,
            ServidorId = gestorServidor.Id,
            Servidor = gestorServidor,
            CentroId = centro.Id,
            Centro = centro,
            DataUltimaSolicitacao = DateTime.UtcNow,
        };

        var adminGestor = new Gestor
        {
            Id = 2,
            ServidorId = adminServidor.Id,
            Servidor = adminServidor,
            CentroId = centro.Id,
            Centro = centro,
            DataUltimaSolicitacao = DateTime.UtcNow,
        };

        context.Centros.Add(centro);
        context.Departamentos.Add(departamento);
        context.Pessoas.AddRange(solicitantePessoa, gestorPessoa, adminPessoa);
        context.Servidores.AddRange(solicitanteServidor, gestorServidor, adminServidor);
        context.Solicitantes.Add(solicitante);
        context.Gestores.AddRange(gestor, adminGestor);
        await context.SaveChangesAsync();

        return (solicitantePessoa.Id, gestorPessoa.Id, adminPessoa.Id);
    }

    private static Pessoa CriarPessoa(long id, string nome, string role, bool isActive)
    {
        return new Pessoa
        {
            Id = id,
            Nome = nome,
            Email = $"{nome.ToLowerInvariant()}@universidade.edu",
            Telefone = "11999999999",
            CPF = id.ToString("00000000000"),
            DataAtualizacao = DateTime.UtcNow,
            PasswordHash = "hash",
            Role = role,
            IsActive = isActive,
        };
    }
}
