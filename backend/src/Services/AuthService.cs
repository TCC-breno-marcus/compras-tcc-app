using System.Security.Claims;
using ComprasTccApp.Backend.Enums;
using ComprasTccApp.Backend.Extensions;
using ComprasTccApp.Models.Entities.Pessoas;
using ComprasTccApp.Models.Entities.Servidores;
using ComprasTccApp.Models.Entities.Solicitantes;
using ComprasTccApp.Services.Interfaces;
using Database;
using Microsoft.EntityFrameworkCore;
using Models.Dtos;

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

            var departamentoEnum = registerDto.Departamento.FromString<DepartamentoEnum>();

            var novoServidor = new Servidor
            {
                Pessoa = novaPessoa,
                IdentificadorInterno = "TEMP-" + registerDto.CPF,
                IsGestor = false,
            };

            var novoSolicitante = new Solicitante
            {
                Servidor = novoServidor,
                Unidade = departamentoEnum,
                DataUltimaSolicitacao = DateTime.UtcNow,
            };

            await _context.Solicitantes.AddAsync(novoSolicitante);
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

        public async Task<UserProfileDto?> GetMyProfileAsync(ClaimsPrincipal user)
        {
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out var userId))
                return null;

            var pessoa = await _context.Pessoas.FindAsync(userId);
            if (pessoa == null)
                return null;

            var servidor = await _context.Servidores.FirstOrDefaultAsync(s => s.PessoaId == userId);

            string? unidadeDoSolicitante = null;
            if (servidor != null)
            {
                var solicitante = await _context.Solicitantes.FirstOrDefaultAsync(sol =>
                    sol.ServidorId == servidor.Id
                );

                if (solicitante != null)
                    unidadeDoSolicitante = solicitante.Unidade.ToFriendlyString();
            }

            var userProfile = new UserProfileDto
            {
                Id = pessoa.Id,
                Nome = pessoa.Nome,
                Email = pessoa.Email,
                Telefone = pessoa.Telefone,
                CPF = pessoa.CPF,
                Role = pessoa.Role,
                Departamento = unidadeDoSolicitante ?? "não disponível",
            };

            return userProfile;
        }
    }
}
