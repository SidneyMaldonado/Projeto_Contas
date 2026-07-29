using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Historico;

public class ObterTodosHistoricoUseCase
{
    private readonly IRepository<Contas_Db.Model.Historico> _repository;

    public ObterTodosHistoricoUseCase(IRepository<Contas_Db.Model.Historico> repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Contas_Db.Model.Historico>> ExecuteAsync() => _repository.GetAllAsync();
}
