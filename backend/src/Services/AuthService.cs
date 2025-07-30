using ComprasTccApp.Backend.DTOs;
using ComprasTccApp.Models.Entities.Pessoas;
using ComprasTccApp.Services.Interfaces;
using Database;
using Microsoft.EntityFrameworkCore;

namespace ComprasTccApp.Backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            AppDbContext context,
            ITokenService tokenService,
            ILogger<AuthService> logger
        )
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<Pessoa> RegisterAsync(RegisterDto registerDto)
        {
            if (await _context.Pessoas.AnyAsync(p => p.Email == registerDto.Email))
            {
                _logger.LogWarning(
                    "Tentativa de registro com email já existente: {Email}",
                    registerDto.Email
                );
                throw new Exception("Este email já está em uso.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

            var novaPessoa = new Pessoa
            {
                Nome = registerDto.Nome,
                Email = registerDto.Email,
                Telefone = registerDto.Telefone,
                CPF = registerDto.CPF,
                DataAtualizacao = DateTime.UtcNow,
                PasswordHash = passwordHash,
                Role = "Solicitante",
            };

            await _context.Pessoas.AddAsync(novaPessoa);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Usuário {Email} registrado com sucesso com a role 'Solicitante'.",
                registerDto.Email
            );
            return novaPessoa;
        }

        public async Task<string?> LoginAsync(LoginDto loginDto)
        {
            var pessoa = await _context.Pessoas.FirstOrDefaultAsync(p => p.Email == loginDto.Email);
            if (pessoa == null)
            {
                _logger.LogWarning(
                    "Tentativa de login para email não encontrado: {Email}",
                    loginDto.Email
                );
                return null;
            }

            var passwordIsValid = BCrypt.Net.BCrypt.Verify(loginDto.Password, pessoa.PasswordHash);

            if (!passwordIsValid)
            {
                _logger.LogWarning(
                    "Tentativa de login com senha inválida para o email: {Email}",
                    loginDto.Email
                );
                return null;
            }

            _logger.LogInformation(
                "Login bem-sucedido para {Email}. Gerando token.",
                loginDto.Email
            );
            return _tokenService.GenerateJwtToken(pessoa);
        }
    }
}
