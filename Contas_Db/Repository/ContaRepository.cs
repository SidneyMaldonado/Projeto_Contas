using Contas_Db.Model;

namespace Contas_Db.Repository;

public class ContaRepository : Repository<Conta>, IContaRepository
{
    private readonly ContasDbContext _context;

    public ContaRepository(ContasDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task AtualizarSaldosAsync(IEnumerable<(int Id, decimal Saldo)> saldos)
    {
        foreach (var (id, saldo) in saldos)
        {
            var conta = await _context.Contas.FindAsync(id);
            if (conta is not null)
                conta.Saldo = saldo;
        }

        await _context.SaveChangesAsync();
    }
}
