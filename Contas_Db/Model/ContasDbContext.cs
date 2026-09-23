using Microsoft.EntityFrameworkCore;

namespace Contas_Db.Model;

public class ContasDbContext : DbContext
{
    public DbSet<Carteira> Carteiras { get; set; }
    public DbSet<Categoria> Categorias { get; set; }
    public DbSet<Conta> Contas { get; set; }
    public DbSet<Credor> Credores { get; set; }
    public DbSet<Divida> Dividas { get; set; }
    public DbSet<Historico> Historicos { get; set; }
    public DbSet<Investimento> Investimentos { get; set; }
    public DbSet<Operacao> Operacoes { get; set; }
    public DbSet<Parcela> Parcelas { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

    public ContasDbContext()
    {
    }

    public ContasDbContext(DbContextOptions<ContasDbContext> options) : base(options)
    {
    }

    // Usadas apenas quando o contexto e criado sem DI (testes, ferramentas do EF).
    // A Api configura o contexto pelo appsettings.{Ambiente}.json.
    private const string ConexaoDesenvolvimento =
        "Server=MS211,1434;Database=test_fin;Integrated Security=True;TrustServerCertificate=True";
    private const string ConexaoPublicacao =
        "Server=MS211,1434;Database=test_fin;User Id=user_db_dev;Password=dev@M1lt3c#;TrustServerCertificate=True";

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer(ObterStringConexao());
        }
    }

    // Ordem: variavel ConnectionStrings__DefaultConnection (mesma do appsettings/Docker);
    // senao o ambiente (ASPNETCORE_ENVIRONMENT/DOTNET_ENVIRONMENT);
    // sem ambiente definido, Debug = desenvolvimento e Release = publicacao.
    public static string ObterStringConexao()
    {
        var conexao = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
        if (!string.IsNullOrWhiteSpace(conexao))
        {
            return conexao;
        }

        var ambiente = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

        if (!string.IsNullOrWhiteSpace(ambiente))
        {
            return string.Equals(ambiente, "Development", StringComparison.OrdinalIgnoreCase)
                ? ConexaoDesenvolvimento
                : ConexaoPublicacao;
        }

#if DEBUG
        return ConexaoDesenvolvimento;
#else
        return ConexaoPublicacao;
#endif
    }
}
