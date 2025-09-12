using ComprasTccApp.Backend.Enums;
using ComprasTccApp.Backend.Extensions;
using ComprasTccApp.Models.Entities.Categorias;
using ComprasTccApp.Models.Entities.Gestores;
using ComprasTccApp.Models.Entities.Pessoas;
using ComprasTccApp.Models.Entities.Servidores;
using ComprasTccApp.Models.Entities.Solicitantes;
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
                Nome = "Admin Padrão",
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
            var solicitantePessoa = new Pessoa
            {
                Nome = "Solicitante Padrão",
                Email = "solicitante@sistema.com",
                CPF = "46205290014",
                Telefone = "11111111111",
                DataAtualizacao = DateTime.UtcNow,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = "Solicitante",
            };

            var solicitanteServidor = new Servidor
            {
                Pessoa = solicitantePessoa,
                IdentificadorInterno = "TEMP-" + solicitantePessoa.CPF,
                IsGestor = false,
            };

            var solicitante = new Solicitante
            {
                Servidor = solicitanteServidor,
                Unidade = "DEPARTAMENTO DE COMPUTAÇÃO".FromString<DepartamentoEnum>(),
                DataUltimaSolicitacao = DateTime.UtcNow,
            };

            await context.Solicitantes.AddAsync(solicitante);
        }

        // Seed do Gestor
        if (!await context.Pessoas.AnyAsync(p => p.Email == "gestor@sistema.com"))
        {
            var gestorPessoa = new Pessoa
            {
                Nome = "Gestor Padrão",
                Email = "gestor@sistema.com",
                CPF = "37636136090",
                Telefone = "22222222222",
                DataAtualizacao = DateTime.UtcNow,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"),
                Role = "Gestor",
            };

            var gestorServidor = new Servidor
            {
                Pessoa = gestorPessoa,
                IdentificadorInterno = "TEMP-" + gestorPessoa.CPF,
                IsGestor = true,
            };

            var gestor = new Gestor
            {
                Servidor = gestorServidor,
                DataUltimaSolicitacao = DateTime.UtcNow,
            };
            await context.Gestores.AddAsync(gestor);
        }

        await context.SaveChangesAsync();
    }
}
