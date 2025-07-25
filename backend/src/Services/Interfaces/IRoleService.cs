namespace Services.Interfaces
{
    public interface IRoleService
    {
        Task<bool> AtribuirRoleAsync(string emailUsuario, string novaRole);
    }
}