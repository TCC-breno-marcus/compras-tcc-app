using Microsoft.EntityFrameworkCore;

namespace Database;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Futuramente, nossas tabelas serão representadas aqui.
    // Por exemplo:
    // public DbSet<Produto> Produtos { get; set; }
}