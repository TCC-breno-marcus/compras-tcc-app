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

        public ResendEmailService(IResend resend, IConfiguration configuration, ILogger<ResendEmailService> logger)
        {
            _resend = resend;
            _logger = logger;

            _emailRemetente = "onboarding@resend.dev";

            // todo: mover para variavel de ambiente e consultar front
            _urlBaseFrontend = configuration["FRONTEND_BASE_URL"] ?? "http://localhost:5173";
        }

        public Task EnviarEmailRedefinicaoSenhaAsync(string emailDestinatario, string nomeUsuario, string token)
        {
            throw new NotImplementedException();
        }

        public async Task EnviarEmailHealthCheckAsync(string emailDestinatario)
        {
            try
            {
                var dataHoraAtual = DateTime.UtcNow;
                var ambiente = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Indefinido";

                var mensagem = new EmailMessage()
                {
                    From = _emailRemetente,
                    Subject = $"SIGAM - Teste de Saúde do Serviço de E-mail ({ambiente})",
                    HtmlBody = $@"
                        <div style='font-family: Arial, sans-serif;'>
                            <h2 style='color: #007bff;'>✅ Serviço de E-mail Operacional</h2>
                            <p>Se você está recebendo este e-mail, a integração com o Resend está funcionando.</p>
                            <p><strong>Data (UTC):</strong> {dataHoraAtual:o}</p>
                        </div>"
                };
                mensagem.To.Add(emailDestinatario);

                await _resend.EmailSendAsync(mensagem);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha no Health Check de e-mail para {Email}.", emailDestinatario);
                throw;
            }
        }
    }
}