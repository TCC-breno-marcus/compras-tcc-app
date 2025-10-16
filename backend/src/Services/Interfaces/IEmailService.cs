namespace Services.Interfaces
{
    public interface IEmailService
    {
        Task EnviarEmailRedefinicaoSenhaAsync(string emailDestinatario, string nomeUsuario, string token);
        Task EnviarEmailHealthCheckAsync(string emailDestinatario);
    }
}