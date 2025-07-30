using Database;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Services
{
    public class RoleService : IRoleService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RoleService> _logger;

        public RoleService(AppDbContext context, ILogger<RoleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> AtribuirRoleAsync(string emailUsuario, string novaRole)
        {
            _logger.LogInformation(
                "Tentando atribuir a role '{NovaRole}' para o usuário '{Email}'",
                novaRole,
                emailUsuario
            );

            var usuario = await _context.Pessoas.FirstOrDefaultAsync(p => p.Email == emailUsuario);
            if (usuario == null)
            {
                _logger.LogWarning("Usuário com email '{Email}' não encontrado.", emailUsuario);
                return false;
            }

            // Valida se a role é uma das permitidas
            var rolesPermitidas = new[] { "Admin", "Gestor", "Solicitante" };
            if (!rolesPermitidas.Contains(novaRole))
            {
                _logger.LogWarning(
                    "Tentativa de atribuir uma role inválida '{NovaRole}'.",
                    novaRole
                );
                return false;
            }

            usuario.Role = novaRole;
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Role '{NovaRole}' atribuída com sucesso para '{Email}'.",
                novaRole,
                emailUsuario
            );
            return true;
        }
    }
}
