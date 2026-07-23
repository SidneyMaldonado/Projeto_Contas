using Contas_Db.Model;

namespace Contas_Db.Repository.Interface;

public interface IContaRepository : IRepository<Conta>
{
    Task AtualizarSaldosAsync(IEnumerable<(int Id, decimal Saldo)> saldos);
}
