using ComprasTccApp.Models.Entities.Categorias;
using ComprasTccApp.Models.Entities.Pessoas;
using Microsoft.EntityFrameworkCore;

namespace Database;

public static class DataSeeder
{
    public static async Task SeedUsers(AppDbContext context)
    {
        await context.Database.MigrateAsync();

        // Seed do Admin
        if (!await context.Pessoas.AnyAsync(p => p.Email == "admin@sistema.com"))
        {
            var adminUser = new Pessoa
            {
                Nome = "Admin do Sistema",
                Email = "admin@sistema.com",
                CPF = "12345678909",
                Telefone = "00000000000",
                DataAtualizacao = DateTime.UtcNow,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = "Admin",
            };
            await context.Pessoas.AddAsync(adminUser);
        }

        // Seed do Solicitante
        if (!await context.Pessoas.AnyAsync(p => p.Email == "solicitante@sistema.com"))
        {
            var solicitanteUser = new Pessoa
            {
                Nome = "Usuário Solicitante Padrão",
                Email = "solicitante@sistema.com",
                CPF = "46205290014",
                Telefone = "11111111111",
                DataAtualizacao = DateTime.UtcNow,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = "Solicitante",
            };
            await context.Pessoas.AddAsync(solicitanteUser);
        }

        // Seed do Gestor
        if (!await context.Pessoas.AnyAsync(p => p.Email == "gestor@sistema.com"))
        {
            var gestorUser = new Pessoa
            {
                Nome = "Usuário Gestor Padrão",
                Email = "gestor@sistema.com",
                CPF = "37636136090",
                Telefone = "22222222222",
                DataAtualizacao = DateTime.UtcNow,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = "Gestor",
            };
            await context.Pessoas.AddAsync(gestorUser);
        }

        await context.SaveChangesAsync();
    }
}
