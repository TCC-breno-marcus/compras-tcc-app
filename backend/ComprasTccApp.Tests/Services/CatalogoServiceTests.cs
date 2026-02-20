using System.Security.Claims;
using System.Text;
using ComprasTccApp.Backend.Enums;
using ComprasTccApp.Backend.Models.Entities.Items;
using ComprasTccApp.Models.Entities.Categorias;
using ComprasTccApp.Models.Entities.Historicos;
using ComprasTccApp.Models.Entities.Itens;
using ComprasTccApp.Models.Entities.Solicitacoes;
using Database;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Models.Dtos;
using Moq;
using Services;

namespace ComprasTccApp.Tests.Services;

public class CatalogoServiceTests
{
    [Fact]
    public async Task GetAllItensAsync_DeveRetornarItensPaginados_QuandoSemFiltros()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetAllItensAsync_DeveRetornarItensPaginados_QuandoSemFiltros));
        var categoria = CriarCategoria(1, "Informatica");
        context.Categorias.Add(categoria);
        context.Items.AddRange(
            CriarItem(1, "Mouse", "100", categoria.Id),
            CriarItem(2, "Teclado", "200", categoria.Id)
        );
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = await service.GetAllItensAsync(
            id: null,
            catMat: null,
            nome: null,
            descricao: null,
            categoriaId: [],
            especificacao: null,
            isActive: null,
            searchTerm: null,
            pageNumber: 1,
            pageSize: 1,
            sortOrder: null
        );

        // Assert
        resultado.TotalCount.Should().Be(2);
        resultado.Data.Should().HaveCount(1);
        resultado.Data[0].Id.Should().Be(1);
        resultado.Data[0].LinkImagem.Should().Be("https://cdn/imagem-100.png");
    }

    [Theory]
    [InlineData("asc", "Aaa")]
    [InlineData("desc", "Zzz")]
    public async Task GetAllItensAsync_DeveOrdenarPorNome_QuandoSortOrderInformado(
        string sortOrder,
        string primeiroEsperado
    )
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(GetAllItensAsync_DeveOrdenarPorNome_QuandoSortOrderInformado) + sortOrder
        );
        var categoria = CriarCategoria(1, "Informatica");
        context.Categorias.Add(categoria);
        context.Items.AddRange(
            CriarItem(1, "Zzz", "100", categoria.Id),
            CriarItem(2, "Aaa", "200", categoria.Id)
        );
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = await service.GetAllItensAsync(
            id: null,
            catMat: null,
            nome: null,
            descricao: null,
            categoriaId: [],
            especificacao: null,
            isActive: null,
            searchTerm: null,
            pageNumber: 1,
            pageSize: 10,
            sortOrder: sortOrder
        );

        // Assert
        resultado.Data.Should().NotBeEmpty();
        resultado.Data[0].Nome.Should().Be(primeiroEsperado);
    }

    [Fact]
    public async Task GetAllItensAsync_DeveAplicarFiltroDeBuscaEStatus_QuandoSearchTermInformado()
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(GetAllItensAsync_DeveAplicarFiltroDeBuscaEStatus_QuandoSearchTermInformado)
        );
        var categoria = CriarCategoria(1, "Informatica");
        context.Categorias.Add(categoria);
        context.Items.AddRange(
            CriarItem(1, "Notebook Dell", "100", categoria.Id, isActive: true),
            CriarItem(2, "Notebook HP", "200", categoria.Id, isActive: false),
            CriarItem(3, "Mouse", "300", categoria.Id, isActive: true)
        );
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = await service.GetAllItensAsync(
            id: null,
            catMat: null,
            nome: null,
            descricao: null,
            categoriaId: [],
            especificacao: null,
            isActive: true,
            searchTerm: "notebook",
            pageNumber: 1,
            pageSize: 10,
            sortOrder: null
        );

        // Assert
        resultado.TotalCount.Should().Be(1);
        resultado.Data.Should().ContainSingle();
        resultado.Data[0].Nome.Should().Be("Notebook Dell");
    }

    [Fact]
    public async Task EditarItemAsync_DeveRetornarNull_QuandoItemNaoEncontrado()
    {
        // Arrange
        await using var context = CriarContexto(nameof(EditarItemAsync_DeveRetornarNull_QuandoItemNaoEncontrado));
        var service = CriarServico(context);

        // Act
        var resultado = await service.EditarItemAsync(10, new ItemUpdateDto { Nome = "Novo Nome" }, CriarUsuario(1));

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task EditarItemAsync_DeveAtualizarItemERegistrarHistorico_QuandoHaAlteracao()
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(EditarItemAsync_DeveAtualizarItemERegistrarHistorico_QuandoHaAlteracao)
        );
        var categoriaAntiga = CriarCategoria(1, "Informatica");
        var categoriaNova = CriarCategoria(2, "Laboratorio");
        context.Categorias.AddRange(categoriaAntiga, categoriaNova);
        context.Items.Add(CriarItem(1, "Mouse", "100", categoriaAntiga.Id));
        await context.SaveChangesAsync();

        var service = CriarServico(context);
        var dto = new ItemUpdateDto
        {
            Nome = "Mouse Gamer",
            CategoriaId = categoriaNova.Id,
            IsActive = false,
        };

        // Act
        var resultado = await service.EditarItemAsync(1, dto, CriarUsuario(10));

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Nome.Should().Be("Mouse Gamer");
        resultado.IsActive.Should().BeFalse();
        resultado.Categoria.Id.Should().Be(categoriaNova.Id);
        context.HistoricoItens.Should().ContainSingle();
        context.HistoricoItens.First().Acao.Should().Be(AcaoHistoricoEnum.Edicao);
    }

    [Fact]
    public async Task ImportarItensAsync_DeveInserirEAtualizarItens_QuandoListaMista()
    {
        // Arrange
        await using var context = CriarContexto(nameof(ImportarItensAsync_DeveInserirEAtualizarItens_QuandoListaMista));
        var categoria = CriarCategoria(1, "Informatica");
        context.Categorias.Add(categoria);
        context.Items.Add(CriarItem(1, "Item Antigo", "A1", categoria.Id, isActive: false));
        await context.SaveChangesAsync();

        var service = CriarServico(context);
        var itens = new List<ItemImportacaoDto>
        {
            new()
            {
                Codigo = "A1",
                Nome = "Item Atualizado",
                Descricao = "Descricao Atualizada",
                CategoriaId = (int)categoria.Id,
                Especificacao = "Nova",
                LinkImagem = "atualizada.png",
            },
            new()
            {
                Codigo = "B2",
                Nome = "Item Novo",
                Descricao = "Descricao Novo",
                CategoriaId = (int)categoria.Id,
                Especificacao = "Nova",
                LinkImagem = "novo.png",
            },
        };

        // Act
        await service.ImportarItensAsync(itens);

        // Assert
        context.Items.Should().HaveCount(2);
        context.Items.Single(i => i.CatMat == "A1").Nome.Should().Be("Item Atualizado");
        context.Items.Single(i => i.CatMat == "A1").IsActive.Should().BeTrue();
        context.Items.Should().Contain(i => i.CatMat == "B2");
    }

    [Fact]
    public async Task PopularImagensAsync_DeveLancarExcecao_QuandoDiretorioNaoExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(PopularImagensAsync_DeveLancarExcecao_QuandoDiretorioNaoExiste));
        var service = CriarServico(context);

        // Act
        var act = () => service.PopularImagensAsync(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

        // Assert
        await act.Should().ThrowAsync<DirectoryNotFoundException>();
    }

    [Fact]
    public async Task PopularImagensAsync_DeveAtualizarProdutosERetornarResumo_QuandoArquivosCorrespondem()
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(PopularImagensAsync_DeveAtualizarProdutosERetornarResumo_QuandoArquivosCorrespondem)
        );
        var categoria = CriarCategoria(1, "Informatica");
        context.Categorias.Add(categoria);
        context.Items.Add(CriarItem(1, "Mouse", "100", categoria.Id));
        await context.SaveChangesAsync();

        var pastaTemp = Path.Combine(Path.GetTempPath(), "catalogo-imagens-" + Guid.NewGuid());
        Directory.CreateDirectory(pastaTemp);
        await File.WriteAllTextAsync(Path.Combine(pastaTemp, "100.png"), "fake");
        await File.WriteAllTextAsync(Path.Combine(pastaTemp, "999.png"), "fake");

        var service = CriarServico(context);

        try
        {
            // Act
            var resumo = await service.PopularImagensAsync(pastaTemp);

            // Assert
            context.Items.Single().LinkImagem.Should().Be("100.png");
            resumo.Should().Contain("SUCESSO!");
            resumo.Should().Contain("1 imagens não encontraram");
        }
        finally
        {
            Directory.Delete(pastaTemp, true);
        }
    }

    [Fact]
    public async Task GetItemByIdAsync_DeveRetornarNull_QuandoNaoEncontrado()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetItemByIdAsync_DeveRetornarNull_QuandoNaoEncontrado));
        var service = CriarServico(context);

        // Act
        var resultado = await service.GetItemByIdAsync(1);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GetItemByIdAsync_DeveRetornarItem_QuandoEncontrado()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetItemByIdAsync_DeveRetornarItem_QuandoEncontrado));
        var categoria = CriarCategoria(1, "Informatica");
        context.Categorias.Add(categoria);
        context.Items.Add(CriarItem(1, "Mouse", "100", categoria.Id));
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = await service.GetItemByIdAsync(1);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Id.Should().Be(1);
        resultado.LinkImagem.Should().Be("https://cdn/imagem-100.png");
    }

    [Fact]
    public async Task CriarItemAsync_DeveLancarExcecao_QuandoCatMatJaExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(CriarItemAsync_DeveLancarExcecao_QuandoCatMatJaExiste));
        var categoria = CriarCategoria(1, "Informatica");
        context.Categorias.Add(categoria);
        context.Items.Add(CriarItem(1, "Mouse", "100", categoria.Id));
        await context.SaveChangesAsync();

        var service = CriarServico(context);
        var dto = new CreateItemDto
        {
            Nome = "Novo",
            Descricao = "Descricao",
            CatMat = "100",
            CategoriaId = categoria.Id,
            LinkImagem = "novo.png",
            Especificacao = "Especificacao",
            PrecoSugerido = 10,
            IsActive = true,
        };

        // Act
        var act = () => service.CriarItemAsync(dto, CriarUsuario(1));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task CriarItemAsync_DeveCriarItemEHistorico_QuandoDadosValidos()
    {
        // Arrange
        await using var context = CriarContexto(nameof(CriarItemAsync_DeveCriarItemEHistorico_QuandoDadosValidos));
        var categoria = CriarCategoria(1, "Informatica");
        context.Categorias.Add(categoria);
        await context.SaveChangesAsync();

        var service = CriarServico(context);
        var dto = new CreateItemDto
        {
            Nome = "Novo Item",
            Descricao = "Descricao",
            CatMat = "N100",
            CategoriaId = categoria.Id,
            LinkImagem = "novo.png",
            Especificacao = "Especificacao",
            PrecoSugerido = 150,
            IsActive = true,
        };

        // Act
        var resultado = await service.CriarItemAsync(dto, CriarUsuario(20));

        // Assert
        resultado.Should().NotBeNull();
        resultado.CatMat.Should().Be("N100");
        context.Items.Should().ContainSingle(i => i.CatMat == "N100");
        context.HistoricoItens.Should().ContainSingle(h => h.Acao == AcaoHistoricoEnum.Criacao);
    }

    [Fact]
    public async Task DeleteItemAsync_DeveRetornarFalha_QuandoItemNaoEncontrado()
    {
        // Arrange
        await using var context = CriarContexto(nameof(DeleteItemAsync_DeveRetornarFalha_QuandoItemNaoEncontrado));
        var service = CriarServico(context);

        // Act
        var resultado = await service.DeleteItemAsync(50, CriarUsuario(1));

        // Assert
        resultado.sucesso.Should().BeFalse();
        resultado.mensagem.Should().Be("Item não encontrado.");
    }

    [Fact]
    public async Task DeleteItemAsync_DeveDesativarItem_QuandoItemJaFoiSolicitado()
    {
        // Arrange
        await using var context = CriarContexto(nameof(DeleteItemAsync_DeveDesativarItem_QuandoItemJaFoiSolicitado));
        var categoria = CriarCategoria(1, "Informatica");
        context.Categorias.Add(categoria);
        var item = CriarItem(1, "Mouse", "100", categoria.Id);
        context.Items.Add(item);
        context.SolicitacaoItens.Add(
            new SolicitacaoItem
            {
                ItemId = item.Id,
                Item = item,
                SolicitacaoId = 1,
                Solicitacao = new Solicitacao
                {
                    Id = 1,
                    SolicitanteId = 1,
                    StatusId = 1,
                    DataCriacao = DateTime.UtcNow,
                    Solicitante = null!,
                    Status = null!,
                },
                Quantidade = 1,
                ValorUnitario = 10,
            }
        );
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = await service.DeleteItemAsync(item.Id, CriarUsuario(2));

        // Assert
        resultado.sucesso.Should().BeTrue();
        resultado.mensagem.Should().Contain("desativado");
        context.Items.Single(i => i.Id == item.Id).IsActive.Should().BeFalse();
        context.HistoricoItens.Should().ContainSingle(h => h.Acao == AcaoHistoricoEnum.Remocao);
    }

    [Fact]
    public async Task DeleteItemAsync_DeveRemoverPermanentemente_QuandoItemNaoFoiSolicitado()
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(DeleteItemAsync_DeveRemoverPermanentemente_QuandoItemNaoFoiSolicitado)
        );
        var categoria = CriarCategoria(1, "Informatica");
        context.Categorias.Add(categoria);
        context.Items.Add(CriarItem(1, "Mouse", "100", categoria.Id));
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = await service.DeleteItemAsync(1, CriarUsuario(3));

        // Assert
        resultado.sucesso.Should().BeTrue();
        resultado.mensagem.Should().Contain("removido permanentemente");
        context.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetItensSemelhantesAsync_DeveRetornarNull_QuandoItemOriginalNaoExiste()
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(GetItensSemelhantesAsync_DeveRetornarNull_QuandoItemOriginalNaoExiste)
        );
        var service = CriarServico(context);

        // Act
        var resultado = await service.GetItensSemelhantesAsync(1);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GetItensSemelhantesAsync_DeveRetornarItens_QuandoMesmoNomeExiste()
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(GetItensSemelhantesAsync_DeveRetornarItens_QuandoMesmoNomeExiste)
        );
        var categoria = CriarCategoria(1, "Informatica");
        context.Categorias.Add(categoria);
        context.Items.AddRange(
            CriarItem(1, "Mouse", "100", categoria.Id),
            CriarItem(2, "Mouse", "200", categoria.Id),
            CriarItem(3, "Teclado", "300", categoria.Id)
        );
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = await service.GetItensSemelhantesAsync(1);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Should().ContainSingle();
        resultado.First().Id.Should().Be(2);
    }

    [Fact]
    public async Task AtualizarImagemAsync_DeveRetornarNull_QuandoItemNaoEncontrado()
    {
        // Arrange
        await using var context = CriarContexto(nameof(AtualizarImagemAsync_DeveRetornarNull_QuandoItemNaoEncontrado));
        var service = CriarServico(context);
        var bytes = Encoding.UTF8.GetBytes("conteudo");
        await using var stream = new MemoryStream(bytes);
        IFormFile arquivo = new FormFile(stream, 0, bytes.Length, "imagem", "foto.png");

        // Act
        var resultado = await service.AtualizarImagemAsync(1, arquivo, CriarUsuario(1));

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task RemoverImagemAsync_DeveRetornarFalse_QuandoItemNaoEncontrado()
    {
        // Arrange
        await using var context = CriarContexto(nameof(RemoverImagemAsync_DeveRetornarFalse_QuandoItemNaoEncontrado));
        var service = CriarServico(context);

        // Act
        var resultado = await service.RemoverImagemAsync(1, CriarUsuario(1));

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public async Task RemoverImagemAsync_DeveRemoverLinkERegistrarHistorico_QuandoItemExiste()
    {
        // Arrange
        await using var context = CriarContexto(
            nameof(RemoverImagemAsync_DeveRemoverLinkERegistrarHistorico_QuandoItemExiste)
        );
        var categoria = CriarCategoria(1, "Informatica");
        context.Categorias.Add(categoria);
        context.Items.Add(CriarItem(1, "Mouse", "100", categoria.Id, linkImagem: "imagem-antiga.png"));
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = await service.RemoverImagemAsync(1, CriarUsuario(7));

        // Assert
        resultado.Should().BeTrue();
        context.Items.Single(i => i.Id == 1).LinkImagem.Should().Be("");
        context.HistoricoItens.Should().ContainSingle(h => h.Acao == AcaoHistoricoEnum.Edicao);
    }

    [Fact]
    public async Task GetHistoricoItemAsync_DeveRetornarNull_QuandoItemNaoEncontrado()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetHistoricoItemAsync_DeveRetornarNull_QuandoItemNaoEncontrado));
        var service = CriarServico(context);

        // Act
        var resultado = await service.GetHistoricoItemAsync(1);

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task GetHistoricoItemAsync_DeveRetornarHistoricoOrdenado_QuandoItemExiste()
    {
        // Arrange
        await using var context = CriarContexto(nameof(GetHistoricoItemAsync_DeveRetornarHistoricoOrdenado_QuandoItemExiste));
        var categoria = CriarCategoria(1, "Informatica");
        var pessoa = new ComprasTccApp.Models.Entities.Pessoas.Pessoa
        {
            Id = 1,
            Nome = "Usuario Teste",
            Email = "usuario@teste.com",
            Telefone = "11999999999",
            CPF = "12345678901",
            DataAtualizacao = DateTime.UtcNow,
            PasswordHash = "hash",
            Role = "Admin",
            IsActive = true,
        };

        context.Pessoas.Add(pessoa);
        context.Categorias.Add(categoria);
        context.Items.Add(CriarItem(1, "Mouse", "100", categoria.Id));
        context.HistoricoItens.AddRange(
            new HistoricoItem
            {
                ItemId = 1,
                PessoaId = 1,
                Pessoa = pessoa,
                DataOcorrencia = DateTime.UtcNow.AddMinutes(-10),
                Acao = AcaoHistoricoEnum.Criacao,
                Detalhes = "Criado",
            },
            new HistoricoItem
            {
                ItemId = 1,
                PessoaId = 1,
                Pessoa = pessoa,
                DataOcorrencia = DateTime.UtcNow,
                Acao = AcaoHistoricoEnum.Edicao,
                Detalhes = "Editado",
            }
        );
        await context.SaveChangesAsync();

        var service = CriarServico(context);

        // Act
        var resultado = await service.GetHistoricoItemAsync(1);

        // Assert
        resultado.Should().NotBeNull();
        resultado!.Should().HaveCount(2);
        resultado[0].Acao.Should().Be(AcaoHistoricoEnum.Edicao.ToString());
        resultado[1].Acao.Should().Be(AcaoHistoricoEnum.Criacao.ToString());
    }

    private static CatalogoService CriarServico(AppDbContext context)
    {
        var loggerMock = new Mock<ILogger<CatalogoService>>();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?> { ["IMAGE_BASE_URL"] = "https://cdn/" }
            )
            .Build();

        return new CatalogoService(context, loggerMock.Object, config);
    }

    private static AppDbContext CriarContexto(string nomeBanco)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(nomeBanco)
            .Options;

        return new AppDbContext(options);
    }

    private static ClaimsPrincipal CriarUsuario(long id)
    {
        return new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, id.ToString())], "Teste")
        );
    }

    private static Categoria CriarCategoria(long id, string nome)
    {
        return new Categoria
        {
            Id = id,
            Nome = nome,
            Descricao = $"Categoria {nome}",
            IsActive = true,
        };
    }

    private static Item CriarItem(
        long id,
        string nome,
        string catMat,
        long categoriaId,
        bool isActive = true,
        string? linkImagem = null
    )
    {
        return new Item
        {
            Id = id,
            Nome = nome,
            CatMat = catMat,
            Descricao = $"Descricao {nome}",
            CategoriaId = categoriaId,
            Categoria = null!,
            LinkImagem = linkImagem ?? $"imagem-{catMat}.png",
            Especificacao = $"Especificacao {nome}",
            PrecoSugerido = 10,
            IsActive = isActive,
        };
    }
}
