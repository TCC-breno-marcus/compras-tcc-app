using System.Security.Claims;
using ComprasTccApp.Backend.Enums;
using ComprasTccApp.Backend.Extensions;
using ComprasTccApp.Models.Entities.Gestores;
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

            if (await _context.Pessoas.AnyAsync(p => p.CPF == registerDto.CPF))
            {
                _logger.LogWarning(
                    "Tentativa de registro com CPF já existente: {CPF}",
                    registerDto.Email
                );
                throw new Exception("Este CPF já está em uso.");
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
                Role = registerDto.Role,
            };

            var novoServidor = new Servidor
            {
                Pessoa = novaPessoa,
                IdentificadorInterno = "TEMP-" + registerDto.CPF,
                IsGestor = false,
            };

            if (registerDto.Role == "Solicitante")
            {
                if (string.IsNullOrWhiteSpace(registerDto.DepartamentoSigla))
                    throw new ArgumentException(
                        "A sigla do departamento é obrigatória para o perfil Solicitante."
                    );

                var depto = await _context.Departamentos.FirstOrDefaultAsync(d =>
                    d.Sigla == registerDto.DepartamentoSigla
                );
                if (depto == null)
                    throw new Exception(
                        $"Departamento com sigla '{registerDto.DepartamentoSigla}' não encontrado."
                    );

                var novoSolicitante = new Solicitante
                {
                    Servidor = novoServidor,
                    DepartamentoId = depto.Id,
                    DataUltimaSolicitacao = DateTime.UtcNow,
                };

                await _context.Solicitantes.AddAsync(novoSolicitante);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(registerDto.CentroSigla))
                    throw new ArgumentException(
                        "A sigla do centro é obrigatória para o perfil Gestor."
                    );

                var centro = await _context.Centros.FirstOrDefaultAsync(c =>
                    c.Sigla == registerDto.CentroSigla
                );

                if (centro == null)
                    throw new Exception(
                        $"Centro com sigla '{registerDto.CentroSigla}' não encontrado."
                    );

                var novoGestor = new Gestor
                {
                    Servidor = novoServidor,
                    CentroId = centro.Id,
                    DataUltimaSolicitacao = DateTime.UtcNow,
                };

                await _context.Gestores.AddAsync(novoGestor);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Usuário {Email} registrado com sucesso com a role {Role}.",
                registerDto.Email,
                registerDto.Role
            );
            return novaPessoa;
        }

        public async Task<LoginResponseDto?> LoginAsync(LoginDto loginDto)
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

            if (!pessoa.IsActive)
            {
                _logger.LogWarning(
                    "Tentativa de login de usuário inativo: {Email}",
                    loginDto.Email
                );
                throw new InvalidOperationException(
                    "O usuário está inativo e não possui mais acesso ao sistema."
                );
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
            var token = _tokenService.GenerateJwtToken(pessoa);
            return new LoginResponseDto { Token = token, Message = "Login bem-sucedido!" };
        }

        public async Task<UserProfileDto?> GetMyProfileAsync(ClaimsPrincipal user)
        {
            var userIdString = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!long.TryParse(userIdString, out var userId))
            {
                return null;
            }

            _logger.LogInformation("Buscando perfil para o usuário ID: {UserId}", userId);

            var pessoa = await _context
                .Pessoas.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == userId);

            if (pessoa == null)
            {
                _logger.LogWarning("Pessoa com ID: {UserId} não encontrada.", userId);
                return null;
            }

            UnidadeOrganizacionalDto? unidadeDto = null;

            if (pessoa.Role == "Solicitante")
            {
                var solicitante = await _context
                    .Solicitantes.Include(sol => sol.Departamento)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(sol => sol.Servidor.PessoaId == userId);

                if (solicitante?.Departamento != null)
                {
                    unidadeDto = new UnidadeOrganizacionalDto
                    {
                        Id = solicitante.Departamento.Id,
                        Nome = solicitante.Departamento.Nome,
                        Sigla = solicitante.Departamento.Sigla,
                        Email = solicitante.Departamento.Email,
                        Telefone = solicitante.Departamento.Telefone,
                        Tipo = "Departamento",
                    };
                }
            }
            else if (pessoa.Role == "Gestor")
            {
                var gestor = await _context
                    .Gestores.Include(g => g.Centro)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(g => g.Servidor.PessoaId == userId);

                if (gestor?.Centro != null)
                {
                    unidadeDto = new UnidadeOrganizacionalDto
                    {
                        Id = gestor.Centro.Id,
                        Nome = gestor.Centro.Nome,
                        Sigla = gestor.Centro.Sigla,
                        Email = gestor.Centro.Email,
                        Telefone = gestor.Centro.Telefone,
                        Tipo = "Centro",
                    };
                }
            }

            var userProfile = new UserProfileDto
            {
                Id = pessoa.Id,
                Nome = pessoa.Nome,
                Email = pessoa.Email,
                Telefone = pessoa.Telefone,
                CPF = pessoa.CPF,
                Role = pessoa.Role,
                IsActive = pessoa.IsActive,
                Unidade = unidadeDto,
            };

            return userProfile;
        }
    }
}
