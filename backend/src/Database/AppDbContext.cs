using ComprasTccApp.Backend.Models.Entities.Items;
using ComprasTccApp.Models.Entities.Gestores;
using ComprasTccApp.Models.Entities.Pessoas;
using ComprasTccApp.Models.Entities.Servidores;
using ComprasTccApp.Models.Entities.Solicitacoes;
using ComprasTccApp.Models.Entities.Solicitantes;
using Microsoft.EntityFrameworkCore;

namespace Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}


    public DbSet<Gestor> Gestores { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<Pessoa> Pessoas { get; set; }
    public DbSet<Servidor> Servidores { get; set; }
    public DbSet<Solicitacao> Solicitacoes { get; set; }
    public DbSet<Solicitante> Solicitantes { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Servidor>()
            .HasOne(s => s.Pessoa)
            .WithOne()
            .HasForeignKey<Servidor>(s => s.PessoaId);

        modelBuilder.Entity<Solicitante>()
            .HasOne(s => s.Servidor)
            .WithOne()
            .HasForeignKey<Solicitante>(s => s.ServidorId);

        modelBuilder.Entity<Gestor>()
            .HasOne(g => g.Servidor)
            .WithOne()
            .HasForeignKey<Gestor>(g => g.ServidorId);

        modelBuilder.Entity<Solicitacao>()
            .HasOne(s => s.Solicitante)
            .WithMany(s => s.Solicitacoes)
            .IsRequired();

        modelBuilder.Entity<Solicitacao>()
            .HasOne(s => s.Gestor)
            .WithMany(g => g.Solicitacoes)
            .IsRequired();

        modelBuilder.Entity<Solicitacao>()
            .HasMany(s => s.Items)
            .WithMany(i => i.Solicitacoes);
    }

}