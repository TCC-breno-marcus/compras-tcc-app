using ComprasTccApp.Backend.Extensions;
using ComprasTccApp.Models.Entities.Servidores;
using ComprasTccApp.Models.Entities.Solicitantes;
using ComprasTccApp.Services.Interfaces;
using Database;
using Microsoft.EntityFrameworkCore;
using Models.Dtos;

namespace ComprasTccApp.Backend.Services
{
    public class UsuarioService : IUsuarioService
    {
        private readonly AppDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger<UsuarioService> _logger;

        public UsuarioService(
            AppDbContext context,
            ITokenService tokenService,
            ILogger<UsuarioService> logger
        )
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<PaginatedResultDto<UserProfileDto>> GetAllUsersAsync(
            string? role,
            int pageNumber,
            int pageSize,
            string? sortOrder,
            bool? isActive
        )
        {
            var query = _context.Pessoas.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(role))
            {
                var validRoles = new[] { "Admin", "Gestor", "Solicitante" };
                if (validRoles.Contains(role, StringComparer.OrdinalIgnoreCase))
                {
                    query = query.Where(p => p.Role == role);
                }
            }

            if (sortOrder?.ToLower() == "desc")
            {
                query = query.OrderByDescending(p => p.Nome);
            }
            else
            {
                query = query.OrderBy(p => p.Nome);
            }

            if (isActive.HasValue)
            {
                query = query.Where(p => p.IsActive == isActive.Value);
            }

            var totalCount = await query.CountAsync();

            var pessoasPaginadas = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var pessoaIds = pessoasPaginadas.Select(p => p.Id).ToList();

            var solicitantesInfo = await _context
                .Solicitantes.Include(s => s.Servidor)
                .Where(s => pessoaIds.Contains(s.Servidor.PessoaId))
                .Include(s => s.Departamento)
                .ToDictionaryAsync(s => s.Servidor.PessoaId);

            var gestoresInfo = await _context
                .Gestores.Include(s => s.Servidor)
                .Where(g => pessoaIds.Contains(g.Servidor.PessoaId))
                .Include(g => g.Centro)
                .ToDictionaryAsync(g => g.Servidor.PessoaId);

            var userProfiles = pessoasPaginadas
                .Select(pessoa =>
                {
                    UnidadeOrganizacionalDto? unidadeDto = null;

                    if (
                        pessoa.Role == "Solicitante"
                        && solicitantesInfo.TryGetValue(pessoa.Id, out var solicitante)
                    )
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
                    else if (
                        (pessoa.Role == "Gestor" || pessoa.Role == "Admin")
                        && gestoresInfo.TryGetValue(pessoa.Id, out var gestor)
                    )
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

                    return new UserProfileDto
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
                })
                .ToList();

            return new PaginatedResultDto<UserProfileDto>(
                userProfiles,
                totalCount,
                pageNumber,
                pageSize
            );
        }

        public async Task<(Servidor servidor, Solicitante solicitante)> GetSolicitanteInfoAsync(
            long pessoaId
        )
        {
            var servidor = await _context
                .Servidores.Include(s => s.Pessoa)
                .FirstOrDefaultAsync(s => s.PessoaId == pessoaId);

            if (servidor == null)
                throw new Exception($"Pessoa com ID {pessoaId} não encontrada na tabela Servidor.");

            var solicitante = await _context
                .Solicitantes.Include(s => s.Departamento)
                .FirstOrDefaultAsync(s => s.ServidorId == servidor.Id);

            if (solicitante == null)
                throw new Exception(
                    $"Servidor com ID {servidor.Id} não encontrado na tabela Solicitante."
                );

            return (servidor, solicitante);
        }

        public async Task<bool> InativarUsuarioAsync(long id)
        {
            var pessoa = await _context.Pessoas.FindAsync(id);
            if (pessoa == null)
            {
                return false;
            }

            pessoa.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuário com ID {Id} foi inativado.", id);
            return true;
        }

        public async Task<bool> AtivarUsuarioAsync(long id)
        {
            var pessoa = await _context.Pessoas.FindAsync(id);
            if (pessoa == null)
            {
                return false;
            }

            pessoa.IsActive = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Usuário com ID {Id} foi ativado.", id);
            return true;
        }
    }
}
