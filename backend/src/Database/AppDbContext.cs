using Microsoft.EntityFrameworkCore;
using ComprasTccApp.Models.Entities.Pessoas;
using ComprasTccApp.Models.Entities.Servidores;
using ComprasTccApp.Models.Entities.Solicitantes;
using ComprasTccApp.Models.Entities.Gestores;
using ComprasTccApp.Models.Entities.Solicitacoes;
using ComprasTccApp.Backend.Models.Entities.Items;
using ComprasTccApp.Models.Entities.Itens;


namespace Database;

public class AppDbContext : DbContext
{

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }


    public DbSet<Pessoa> Pessoas { get; set; }
    public DbSet<Servidor> Servidores { get; set; }
    public DbSet<Solicitante> Solicitantes { get; set; }
    public DbSet<Gestor> Gestores { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Solicitacao> Solicitacoes { get; set; }
    public DbSet<SolicitacaoItem> SolicitacaoItens { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SolicitacaoItem>()
            .HasKey(si => new { si.SolicitacaoId, si.ItemId });

        modelBuilder.Entity<Servidor>()
            .HasOne(servidor => servidor.Pessoa)
            .WithOne()
            .HasForeignKey<Servidor>(servidor => servidor.PessoaId)
            .IsRequired();

        modelBuilder.Entity<Solicitante>()
            .HasOne(solicitante => solicitante.Servidor)
            .WithOne()
            .HasForeignKey<Solicitante>(solicitante => solicitante.ServidorId)
            .IsRequired();

        modelBuilder.Entity<Gestor>()
            .HasOne(gestor => gestor.Servidor)
            .WithOne()
            .HasForeignKey<Gestor>(gestor => gestor.ServidorId)
            .IsRequired();

        modelBuilder.Entity<Solicitacao>()
            .HasOne(solicitacao => solicitacao.Solicitante)
            .WithMany(solicitante => solicitante.Solicitacoes)
            .HasForeignKey("SolicitanteId")
            .IsRequired();

        modelBuilder.Entity<Solicitacao>()
            .HasOne(solicitacao => solicitacao.Gestor)
            .WithMany(gestor => gestor.Solicitacoes)
            .HasForeignKey("GestorId")
            .IsRequired();

        modelBuilder.Entity<SolicitacaoItem>()
            .HasOne(si => si.Solicitacao)
            .WithMany(s => s.ItemSolicitacao)
            .HasForeignKey(si => si.SolicitacaoId)
            .IsRequired();


        modelBuilder.Entity<SolicitacaoItem>()
            .HasOne(si => si.Item)
            .WithMany(i => i.SolicitacoesItem)
            .HasForeignKey(si => si.ItemId)
            .IsRequired();
    }
}