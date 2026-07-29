using Contas_Db.Model;
using Contas_Db.Repository.Interface;

namespace Contas_Db.Repository;

public class HistoricoRepository : Repository<Historico>, IHistoricoRepository
{
    public HistoricoRepository(ContasDbContext context) : base(context)
    {
    }
}
