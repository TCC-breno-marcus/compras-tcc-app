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

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateJwtToken(Pessoa pessoa)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, pessoa.Id.ToString()),
                new Claim(ClaimTypes.Name, pessoa.Nome),
                new Claim(ClaimTypes.Email, pessoa.Email),
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
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}