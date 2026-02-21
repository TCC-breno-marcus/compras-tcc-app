using Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Models.Dtos;
using Moq;
using Services.Interfaces;

namespace ComprasTccApp.Tests.Controllers;

public class DadosPublicosControllerTests
{
    [Fact]
    public async Task ConsultarSolicitacoes_DeveRetornarOkComJson_QuandoFormatoArquivoPadrao()
    {
        // Arrange
        var serviceMock = new Mock<IDadosPublicosService>();
        var loggerMock = new Mock<ILogger<DadosPublicosController>>();
        var retorno = new PublicoSolicitacaoConsultaResultDto { TotalCount = 1, PageNumber = 1, PageSize = 25, TotalPages = 1 };

        serviceMock
            .Setup(s =>
                s.ConsultarSolicitacoesAsync(
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<int?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()
                )
            )
            .ReturnsAsync(retorno);

        var controller = new DadosPublicosController(serviceMock.Object, loggerMock.Object);

        // Act
        var actionResult = await controller.ConsultarSolicitacoes(
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
            pageSize: 25,
            formatoArquivo: "json"
        );

        // Assert
        actionResult.Should().BeOfType<OkObjectResult>();
        serviceMock.Verify(
            s =>
                s.ConsultarSolicitacoesAsync(
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<int?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()
                ),
            Times.Once
        );
        serviceMock.Verify(
            s =>
                s.ExportarSolicitacoesCsvAsync(
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<int?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task ConsultarSolicitacoes_DeveRetornarArquivoCsv_QuandoFormatoArquivoCsv()
    {
        // Arrange
        var serviceMock = new Mock<IDadosPublicosService>();
        var loggerMock = new Mock<ILogger<DadosPublicosController>>();
        var csvBytes = "SolicitacaoId\n1"u8.ToArray();

        serviceMock
            .Setup(s =>
                s.ExportarSolicitacoesCsvAsync(
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<int?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()
                )
            )
            .ReturnsAsync(csvBytes);

        var controller = new DadosPublicosController(serviceMock.Object, loggerMock.Object);

        // Act
        var actionResult = await controller.ConsultarSolicitacoes(
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
            pageSize: 25,
            formatoArquivo: "csv"
        );

        // Assert
        actionResult.Should().BeOfType<FileContentResult>();
        var fileResult = (FileContentResult)actionResult;
        fileResult.ContentType.Should().Be("text/csv; charset=utf-8");
        fileResult.FileContents.Should().BeEquivalentTo(csvBytes);
        fileResult.FileDownloadName.Should().StartWith("dados-publicos-solicitacoes-");
        serviceMock.Verify(
            s =>
                s.ExportarSolicitacoesCsvAsync(
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<int?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ConsultarSolicitacoes_DeveRetornarArquivoPdf_QuandoFormatoArquivoPdf()
    {
        // Arrange
        var serviceMock = new Mock<IDadosPublicosService>();
        var loggerMock = new Mock<ILogger<DadosPublicosController>>();
        var pdfBytes = "%PDF-1.7"u8.ToArray();

        serviceMock
            .Setup(s =>
                s.ExportarSolicitacoesPdfAsync(
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<int?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()
                )
            )
            .ReturnsAsync(pdfBytes);

        var controller = new DadosPublicosController(serviceMock.Object, loggerMock.Object);

        // Act
        var actionResult = await controller.ConsultarSolicitacoes(
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
            pageSize: 25,
            formatoArquivo: "pdf"
        );

        // Assert
        actionResult.Should().BeOfType<FileContentResult>();
        var fileResult = (FileContentResult)actionResult;
        fileResult.ContentType.Should().Be("application/pdf");
        fileResult.FileContents.Should().BeEquivalentTo(pdfBytes);
        fileResult.FileDownloadName.Should().StartWith("dados-publicos-solicitacoes-");
        serviceMock.Verify(
            s =>
                s.ExportarSolicitacoesPdfAsync(
                    It.IsAny<DateTime?>(),
                    It.IsAny<DateTime?>(),
                    It.IsAny<int?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<decimal?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<int>(),
                    It.IsAny<int>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ConsultarSolicitacoes_DeveRetornarBadRequest_QuandoFormatoArquivoInvalido()
    {
        // Arrange
        var serviceMock = new Mock<IDadosPublicosService>();
        var loggerMock = new Mock<ILogger<DadosPublicosController>>();
        var controller = new DadosPublicosController(serviceMock.Object, loggerMock.Object);

        // Act
        var actionResult = await controller.ConsultarSolicitacoes(
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
            pageSize: 25,
            formatoArquivo: "xml"
        );

        // Assert
        actionResult.Should().BeOfType<BadRequestObjectResult>();
        serviceMock.VerifyNoOtherCalls();
    }
}
