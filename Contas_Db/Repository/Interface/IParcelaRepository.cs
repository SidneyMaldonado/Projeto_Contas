using Contas_Db.Model;

namespace Contas_Db.Repository.Interface;

public interface IParcelaRepository : IRepository<Parcela>
{
    Task PagarAsync(int id, DateTime dataPagamento);
    Task DesfazerPagamentoAsync(int id);
}
