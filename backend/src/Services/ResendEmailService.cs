using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;
using Services.Interfaces;

namespace Services
{
    public class ResendEmailService : IEmailService
    {
        private readonly ILogger<ResendEmailService> _logger;
        private readonly IResend _resend;
        private readonly string _emailRemetente;
        private readonly string _urlBaseFrontend;

        /// <summary>
        /// Inicializa o serviço de envio de e-mails via Resend com dependências de integração e configuração.
        /// </summary>
        /// <param name="resend">Cliente responsável por enviar e-mails pela API Resend.</param>
        /// <param name="configuration">Fonte de configurações da aplicação para parâmetros de ambiente.</param>
        /// <param name="logger">Logger usado para registrar falhas operacionais do serviço.</param>
        public ResendEmailService(IResend resend, IConfiguration configuration, ILogger<ResendEmailService> logger)
        {
            _resend = resend;
            _logger = logger;

            _emailRemetente = "onboarding@resend.dev";

            // todo: mover para variavel de ambiente e consultar front
            _urlBaseFrontend = configuration["FRONTEND_BASE_URL"] ?? "http://localhost:5173";
        }

        /// <summary>
        /// Envia o e-mail de redefinição de senha para o usuário informado.
        /// </summary>
        /// <param name="emailDestinatario">E-mail do usuário que receberá o link de redefinição.</param>
        /// <param name="nomeUsuario">Nome do usuário usado na personalização da mensagem.</param>
        /// <param name="token">Token de redefinição de senha que será incorporado ao link.</param>
        /// <returns>Uma tarefa assíncrona que representa a operação de envio.</returns>
        /// <exception cref="NotImplementedException">Lançada enquanto a implementação de envio de redefinição de senha não estiver concluída.</exception>
        public Task EnviarEmailRedefinicaoSenhaAsync(string emailDestinatario, string nomeUsuario, string token)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Envia um e-mail genérico utilizando os parâmetros de destinatário, assunto e corpo HTML.
        /// Este método centraliza o envio para ser reutilizado por métodos específicos do serviço.
        /// </summary>
        /// <param name="emailDestinatario">Destinatário principal da mensagem.</param>
        /// <param name="assunto">Assunto do e-mail.</param>
        /// <param name="corpoHtml">Corpo HTML do e-mail.</param>
        /// <param name="cancellationToken">Token opcional para cancelamento da operação.</param>
        private async Task EnviarEmailGenericoAsync(
            string emailDestinatario,
            string assunto,
            string corpoHtml,
            CancellationToken cancellationToken = default
        )
        {
            var mensagem = new EmailMessage()
            {
                From = _emailRemetente,
                Subject = assunto,
                HtmlBody = corpoHtml,
            };

            mensagem.To.Add(emailDestinatario);
            await _resend.EmailSendAsync(mensagem, cancellationToken);
        }

        /// <summary>
        /// Envia um e-mail de health check para validar a integração de envio no ambiente atual.
        /// </summary>
        /// <param name="emailDestinatario">E-mail que receberá a mensagem de validação do serviço.</param>
        /// <returns>Uma tarefa assíncrona que representa a operação de envio.</returns>
        /// <exception cref="Exception">Propaga qualquer erro retornado pela camada de envio após registrar o log de falha.</exception>
        public async Task EnviarEmailHealthCheckAsync(string emailDestinatario)
        {
            try
            {
                var dataHoraAtual = DateTime.UtcNow;
                var ambiente = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Indefinido";

                var assunto = $"SIGAM - Teste de Saúde do Serviço de E-mail ({ambiente})";
                var corpoHtml = $@"
                        <div style='font-family: Arial, sans-serif;'>
                            <h2 style='color: #007bff;'>✅ Serviço de E-mail Operacional</h2>
                            <p>Se você está recebendo este e-mail, a integração com o Resend está funcionando.</p>
                            <p><strong>Data (UTC):</strong> {dataHoraAtual:o}</p>
                        </div>";

                await EnviarEmailGenericoAsync(emailDestinatario, assunto, corpoHtml);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha no Health Check de e-mail para {Email}.", emailDestinatario);
                throw;
            }
        }
    }
}
