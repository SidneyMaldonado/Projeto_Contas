using Contas_Db.Model;
using Contas_Db.Repository.Interface;

namespace Contas_Db.Repository;

public class OperacaoRepository : Repository<Operacao>, IOperacaoRepository
{
    public OperacaoRepository(ContasDbContext context) : base(context)
    {
    }
}
