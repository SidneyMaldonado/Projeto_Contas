using Contas_Db.Model;
using Contas_Db.Repository.Interface;

namespace Contas_Db.Repository;

public class CarteiraRepository : Repository<Carteira>, ICarteiraRepository
{
    public CarteiraRepository(ContasDbContext context) : base(context)
    {
    }
}
