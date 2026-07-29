using Contas_Db.Model;
using Contas_Db.Repository.Interface;

namespace Contas_Db.Repository;

public class InvestimentoRepository : Repository<Investimento>, IInvestimentoRepository
{
    public InvestimentoRepository(ContasDbContext context) : base(context)
    {
    }
}
