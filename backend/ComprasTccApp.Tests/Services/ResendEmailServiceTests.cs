using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Resend;
using Services;

namespace ComprasTccApp.Tests.Services;

public class ResendEmailServiceTests
{
    [Theory]
    [InlineData("usuario1@universidade.edu")]
    [InlineData("usuario2@universidade.edu")]
    public async Task EnviarEmailHealthCheckAsync_DeveEnviarEmail_QuandoDestinatarioValido(string emailDestinatario)
    {
        // Arrange
        EmailMessage? mensagemCapturada = null;
        var resendMock = new Mock<IResend>();
        resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .Callback<EmailMessage, CancellationToken>((mensagem, _) => mensagemCapturada = mensagem)
            .ReturnsAsync(new ResendResponse<Guid>(Guid.NewGuid(), new ResendRateLimit()));

        var loggerMock = new Mock<ILogger<ResendEmailService>>();
        var service = CriarServico(resendMock, loggerMock);

        // Act
        await service.EnviarEmailHealthCheckAsync(emailDestinatario);

        // Assert
        resendMock.Verify(
            r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
        mensagemCapturada.Should().NotBeNull();
        mensagemCapturada!.From.Email.Should().Be("onboarding@resend.dev");
        mensagemCapturada.Subject.Should().Contain("SIGAM - Teste de Saúde do Serviço de E-mail");
        mensagemCapturada.To.Should().ContainSingle(destinatario => destinatario.Email == emailDestinatario);
        mensagemCapturada.HtmlBody.Should().Contain("Serviço de E-mail Operacional");
    }

    [Fact]
    public async Task EnviarEmailHealthCheckAsync_DevePropagarExcecaoELogarErro_QuandoResendFalhar()
    {
        // Arrange
        var resendMock = new Mock<IResend>();
        var excecaoEsperada = new InvalidOperationException("Falha no provedor de e-mail.");
        resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(excecaoEsperada);

        var loggerMock = new Mock<ILogger<ResendEmailService>>();
        var service = CriarServico(resendMock, loggerMock);

        // Act
        var act = () => service.EnviarEmailHealthCheckAsync("destinatario@universidade.edu");

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Falha no provedor de e-mail.");
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state.ToString()!.Contains("Falha no Health Check de e-mail")
                ),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task EnviarEmailRedefinicaoSenhaAsync_DeveLancarNotImplementedException_QuandoMetodoNaoImplementado()
    {
        // Arrange
        var resendMock = new Mock<IResend>();
        var loggerMock = new Mock<ILogger<ResendEmailService>>();
        var service = CriarServico(resendMock, loggerMock);

        // Act
        var act = () => service.EnviarEmailRedefinicaoSenhaAsync(
            "usuario@universidade.edu",
            "Usuario Teste",
            "token-de-teste"
        );

        // Assert
        await act.Should().ThrowAsync<NotImplementedException>();
    }

    private static ResendEmailService CriarServico(
        Mock<IResend> resendMock,
        Mock<ILogger<ResendEmailService>> loggerMock
    )
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?> { ["FRONTEND_BASE_URL"] = "http://localhost:5173" }
            )
            .Build();

        return new ResendEmailService(resendMock.Object, configuration, loggerMock.Object);
    }
}
