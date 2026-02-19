using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ComprasTccApp.Models.Entities.Pessoas;
using ComprasTccApp.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Inicializa o serviço responsável por geração de tokens JWT para autenticação.
        /// </summary>
        /// <param name="config">Configurações da aplicação contendo chave, emissor e audiência do JWT.</param>
        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Gera um token JWT assinado com os dados de identidade e role da pessoa informada.
        /// </summary>
        /// <param name="pessoa">Entidade da pessoa autenticada usada para composição dos claims do token.</param>
        /// <returns>Token JWT serializado em formato de string.</returns>
        /// <exception cref="ArgumentNullException">Lançada quando os valores de configuração obrigatórios para o JWT não estão definidos.</exception>
        /// <exception cref="ArgumentException">Lançada quando a chave do JWT está vazia ou possui tamanho inválido para assinatura HMAC.</exception>
        public string GenerateJwtToken(Pessoa pessoa)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, pessoa.Id.ToString()),
                new Claim(ClaimTypes.Name, pessoa.Nome),
                new Claim(ClaimTypes.Email, pessoa.Email),
                new Claim(ClaimTypes.Role, pessoa.Role),
                // Futuramente, podemos adicionar perfis (Roles) aqui
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = creds,
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
