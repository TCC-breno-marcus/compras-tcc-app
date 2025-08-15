using ComprasTccApp.Backend.Models.Entities.Items;
using ComprasTccApp.Models.Entities.Categorias;
using ComprasTccApp.Models.Entities.Gestores;
using ComprasTccApp.Models.Entities.Itens;
using ComprasTccApp.Models.Entities.Pessoas;
using ComprasTccApp.Models.Entities.Servidores;
using ComprasTccApp.Models.Entities.Solicitacoes;
using ComprasTccApp.Models.Entities.Solicitantes;
using Microsoft.EntityFrameworkCore;

namespace Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<Pessoa> Pessoas { get; set; }
    public DbSet<Servidor> Servidores { get; set; }
    public DbSet<Solicitante> Solicitantes { get; set; }
    public DbSet<Gestor> Gestores { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Solicitacao> Solicitacoes { get; set; }
    public DbSet<SolicitacaoGeral> SolicitacoesGerais { get; set; }
    public DbSet<SolicitacaoPatrimonial> SolicitacoesPatrimoniais { get; set; }
    public DbSet<SolicitacaoItem> SolicitacaoItens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder
            .Entity<Solicitacao>()
            .HasDiscriminator<string>("TipoSolicitacao")
            .HasValue<SolicitacaoGeral>("GERAL")
            .HasValue<SolicitacaoPatrimonial>("PATRIMONIAL");

        modelBuilder.Entity<Solicitacao>().Property("TipoSolicitacao").HasMaxLength(20);

        // Relações 1-para-1 (Pessoa -> Servidor -> Solicitante/Gestor)
        modelBuilder
            .Entity<Servidor>()
            .HasOne(servidor => servidor.Pessoa)
            .WithOne()
            .HasForeignKey<Servidor>(servidor => servidor.PessoaId)
            .IsRequired();

        modelBuilder
            .Entity<Solicitante>()
            .HasOne(solicitante => solicitante.Servidor)
            .WithOne()
            .HasForeignKey<Solicitante>(solicitante => solicitante.ServidorId)
            .IsRequired();

        modelBuilder
            .Entity<Gestor>()
            .HasOne(gestor => gestor.Servidor)
            .WithOne()
            .HasForeignKey<Gestor>(gestor => gestor.ServidorId)
            .IsRequired();

        modelBuilder
            .Entity<Solicitacao>()
            .HasOne(s => s.Solicitante)
            .WithMany(sol => sol.Solicitacoes)
            .HasForeignKey(s => s.SolicitanteId)
            .IsRequired();

        modelBuilder
            .Entity<Solicitacao>()
            .HasOne(s => s.Gestor)
            .WithMany(g => g.Solicitacoes)
            .HasForeignKey(s => s.GestorId);

        modelBuilder.Entity<SolicitacaoItem>(entity =>
        {
            entity.HasKey(si => new { si.SolicitacaoId, si.ItemId });
            entity
                .HasOne(si => si.Solicitacao)
                .WithMany(s => s.ItemSolicitacao)
                .HasForeignKey(si => si.SolicitacaoId);
            entity
                .HasOne(si => si.Item)
                .WithMany(i => i.SolicitacoesItem)
                .HasForeignKey(si => si.ItemId);
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasIndex(c => c.Nome).IsUnique();

            entity
                .HasMany(c => c.Itens)
                .WithOne(i => i.Categoria)
                .HasForeignKey(i => i.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasData(
                new Categoria
                {
                    Id = 1,
                    Nome = "Componentes Eletrônicos",
                    Descricao =
                        "Componentes discretos e integrados para montagem e prototipagem de circuitos. Inclui resistores, capacitores, transistores, MCUs, LEDs e PCBs.",
                    IsActive = true,
                },
                new Categoria
                {
                    Id = 2,
                    Nome = "Eletrodomésticos",
                    Descricao =
                        "Equipamentos elétricos de uso doméstico, em cozinhas ou escritórios. Abrange linha branca, portáteis e aparelhos de climatização.",
                    IsActive = true,
                },
                new Categoria
                {
                    Id = 3,
                    Nome = "Ferramentas",
                    Descricao =
                        "Instrumentos manuais e elétricos para manutenção, montagem, reparos e medições. Inclui chaves de fenda, alicates, furadeiras e multímetros.",
                    IsActive = true,
                },
                new Categoria
                {
                    Id = 4,
                    Nome = "Reagentes Químicos",
                    Descricao =
                        "Substâncias e compostos químicos utilizados em análises e sínteses. Inclui ácidos, bases, solventes, sais e padrões analíticos.",
                    IsActive = true,
                },
                new Categoria
                {
                    Id = 5,
                    Nome = "Materiais de Laboratório",
                    Descricao =
                        "Utensílios, consumíveis e pequenos equipamentos para uso geral em laboratório que não são vidrarias ou reagentes.",
                    IsActive = true,
                },
                new Categoria
                {
                    Id = 6,
                    Nome = "Mobiliário",
                    Descricao =
                        "Móveis para ambientes de escritório, laboratórios ou áreas comuns, como mesas, cadeiras, armários e bancadas de trabalho.",
                    IsActive = true,
                },
                new Categoria
                {
                    Id = 7,
                    Nome = "Diversos",
                    Descricao =
                        "Categoria residual para itens que não se enquadram claramente em nenhuma outra classificação. Ideal para materiais de escritório ou de consumo geral.",
                    IsActive = true,
                }
            );
        });
    }
}
