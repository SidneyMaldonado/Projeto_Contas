using Microsoft.EntityFrameworkCore;

namespace Contas_Db.Model;

public class ContasDbContext : DbContext
{
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Conta> Contas { get; set; }
    public DbSet<Credor> Credores { get; set; }
    public DbSet<Divida> Dividas { get; set; }
    public DbSet<Parcela> Parcelas { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

    public ContasDbContext()
    {
    }

    public ContasDbContext(DbContextOptions<ContasDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(
                "Server=MS211,1434;Database=test_fin;Integrated Security=True;TrustServerCertificate=True");
        }
    }
}
