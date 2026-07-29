using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Historico;

public class ExcluirHistoricoUseCase
{
    private readonly IRepository<Contas_Db.Model.Historico> _repository;

    public ExcluirHistoricoUseCase(IRepository<Contas_Db.Model.Historico> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.DeleteAsync(id);
}
