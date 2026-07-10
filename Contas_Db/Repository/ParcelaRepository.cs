using Contas_Db.Model;

namespace Contas_Db.Repository;

public class ParcelaRepository : Repository<Parcela>, IParcelaRepository
{
    private readonly ContasDbContext _context;

    public ParcelaRepository(ContasDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task PagarAsync(int id, DateTime dataPagamento)
    {
        var parcela = await _context.Parcelas.FindAsync(id);
        if (parcela is not null)
        {
            parcela.DataPagamento = dataPagamento;
            await _context.SaveChangesAsync();
        }
    }

    public async Task DesfazerPagamentoAsync(int id)
    {
        var parcela = await _context.Parcelas.FindAsync(id);
        if (parcela is not null)
        {
            parcela.DataPagamento = null;
            await _context.SaveChangesAsync();
        }
    }
}
