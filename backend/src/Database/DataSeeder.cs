using ComprasTccApp.Backend.Enums;
using ComprasTccApp.Backend.Extensions;
using ComprasTccApp.Models.Entities.Categorias;
using ComprasTccApp.Models.Entities.Centros;
using ComprasTccApp.Models.Entities.Departamentos;
using ComprasTccApp.Models.Entities.Gestores;
using ComprasTccApp.Models.Entities.Pessoas;
using ComprasTccApp.Models.Entities.Servidores;
using ComprasTccApp.Models.Entities.Solicitantes;
using Microsoft.EntityFrameworkCore;

namespace Database;

public static class DataSeeder
{
    public static async Task SeedCentrosAsync(AppDbContext context)
    {
        if (await context.Centros.AnyAsync())
            return; // Já populado

        var centro = new Centro
        {
            Nome = "Centro de Ciências Exatas e Tecnologia",
            Sigla = "CCET",
            Email = "ccet@academico.ufs.br",
            Telefone = "7931946684",
        };

        await context.Centros.AddAsync(centro);
        await context.SaveChangesAsync();
    }

    public static async Task SeedDepartamentosAsync(AppDbContext context)
    {
        if (await context.Departamentos.AnyAsync())
            return;

        var ccet = await context.Centros.FirstOrDefaultAsync(c => c.Sigla == "CCET");
        if (ccet == null)
        {
            throw new Exception("O Centro 'CCET' precisa ser semeado antes dos departamentos.");
        }

        var departamentos = new List<Departamento>
        {
            new()
            {
                Nome = "Departamento de Ciência e Engenharia de Materiais",
                Sigla = "DCEM",
                Email = "dcem@academico.ufs.br",
                Telefone = "7931946888",
                CentroId = ccet.Id,
            },
            new()
            {
                Nome = "Departamento de Engenharia Mecânica",
                Sigla = "DMEC",
                Email = "dmec@academico.ufs.br",
                Telefone = "7931946310",
                CentroId = ccet.Id,
            },
            new()
            {
                Nome = "Departamento de Tecnologia de Alimentos",
                Sigla = "DTA",
                Email = "dta@academico.ufs.br",
                Telefone = "7931946903",
                CentroId = ccet.Id,
            },
            new()
            {
                Nome = "Departamento de Engenharia Civil",
                Sigla = "DEC",
                Email = "dec@academico.ufs.br",
                Telefone = "7931946700",
                CentroId = ccet.Id,
            },
            new()
            {
                Nome = "Departamento de Engenharia Química",
                Sigla = "DEQ",
                Email = "deq@academico.ufs.br",
                Telefone = "7931946676",
                CentroId = ccet.Id,
            },
            new()
            {
                Nome = "Departamento de Computação",
                Sigla = "DCOMP",
                Email = "secretaria@dcomp.ufs.br",
                Telefone = "7931946678",
                CentroId = ccet.Id,
            },
            new()
            {
                Nome = "Departamento de Engenharia de Produção",
                Sigla = "DEPRO",
                Email = "depro@academico.ufs.br",
                Telefone = "7931946320",
                CentroId = ccet.Id,
            },
            new()
            {
                Nome = "Departamento de Matemática",
                Sigla = "DMA",
                Email = "dma@mat.ufs.br",
                Telefone = "7931946707",
                CentroId = ccet.Id,
            },
            new()
            {
                Nome = "Departamento de Química",
                Sigla = "DQI",
                Email = "dqi@academico.ufs.br",
                Telefone = "7931946650",
                CentroId = ccet.Id,
            },
            new()
            {
                Nome = "Departamento de Engenharia Ambiental",
                Sigla = "DEAM",
                Email = "deam@academico.ufs.br",
                Telefone = "7931946896",
                CentroId = ccet.Id,
            },
            new()
            {
                Nome = "Departamento de Geologia",
                Sigla = "DGEOL",
                Email = "dgeol@academico.ufs.br",
                Telefone = "7931947500",
                CentroId = ccet.Id,
            },
            new()
            {
                Nome = "Departamento de Física",
                Sigla = "DFI",
                Email = "dfi@academico.ufs.br",
                Telefone = "7931946630",
                CentroId = ccet.Id,
            },
            new()
            {
                Nome = "Departamento de Estatística e Ciências Atuariais",
                Sigla = "DECAT",
                Email = "decat@academico.ufs.br",
                Telefone = "7931946729",
                CentroId = ccet.Id,
            },
            new()
            {
                Nome = "Departamento de Engenharia Elétrica",
                Sigla = "DEL",
                Email = "del@academico.ufs.br",
                Telefone = "7931946837",
                CentroId = ccet.Id,
            },
            new()
            {
                Nome = "Departamento de Engenharia de Petróleo",
                Sigla = "DEPET",
                Email = "depet@academico.ufs.br",
                Telefone = "7931946598",
                CentroId = ccet.Id,
            },
        };

        await context.Departamentos.AddRangeAsync(departamentos);
        await context.SaveChangesAsync();
    }

    public static async Task SeedUsersAsync(AppDbContext context)
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

            var deptoComputacao = await context.Departamentos.FirstOrDefaultAsync(d =>
                d.Sigla == "DCOMP"
            );

            if (deptoComputacao == null)
            {
                throw new Exception(
                    "Departamento de Computação (DCOMP) não encontrado. Rode o seeder de departamentos primeiro."
                );
            }

            var solicitante = new Solicitante
            {
                Servidor = solicitanteServidor,
                DepartamentoId = deptoComputacao.Id,
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

            var centroCcet = await context.Centros.FirstOrDefaultAsync(c => c.Sigla == "CCET");
            if (centroCcet == null)
            {
                throw new Exception(
                    "Centro CCET não encontrado. Rode o seeder de centros primeiro."
                );
            }

            // ... (criação da Pessoa e do Servidor para o gestor)

            var gestor = new Gestor
            {
                Servidor = gestorServidor,
                CentroId = centroCcet.Id, // <-- Atribui o ID do centro
                DataUltimaSolicitacao = DateTime.UtcNow,
            };
            await context.Gestores.AddAsync(gestor);
        }

        await context.SaveChangesAsync();
    }
}
