using Database;
using Microsoft.EntityFrameworkCore;
using Services.Interfaces;

namespace Services
{
    public class RoleService : IRoleService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RoleService> _logger;

        /// <summary>
        /// Inicializa o serviço responsável por atribuição de roles aos usuários.
        /// </summary>
        /// <param name="context">Contexto de dados usado para consultar e persistir alterações de usuários.</param>
        /// <param name="logger">Logger usado para registrar eventos de auditoria e falhas de atribuição.</param>
        public RoleService(AppDbContext context, ILogger<RoleService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Atribui uma nova role ao usuário identificado por e-mail, desde que o usuário exista e a role esteja na lista permitida.
        /// </summary>
        /// <param name="emailUsuario">E-mail do usuário que terá a role atualizada.</param>
        /// <param name="novaRole">Nova role a ser atribuída. Valores permitidos: Admin, Gestor e Solicitante.</param>
        /// <returns>
        /// <c>true</c> quando a role é válida, o usuário é encontrado e a alteração é persistida; caso contrário, <c>false</c>.
        /// </returns>
        /// <exception cref="DbUpdateException">Lançada quando ocorre falha ao persistir a alteração no banco de dados.</exception>
        /// <exception cref="DbUpdateConcurrencyException">Lançada quando ocorre conflito de concorrência durante a persistência.</exception>
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
