using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Historico;

public class ObterPorIdHistoricoUseCase
{
    private readonly IRepository<Contas_Db.Model.Historico> _repository;

    public ObterPorIdHistoricoUseCase(IRepository<Contas_Db.Model.Historico> repository)
    {
        _repository = repository;
    }

    public Task<Contas_Db.Model.Historico?> ExecuteAsync(int id) => _repository.GetByIdAsync(id);
}
