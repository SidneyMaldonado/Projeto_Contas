using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Historico;

public class InativarHistoricoUseCase
{
    private readonly IRepository<Contas_Db.Model.Historico> _repository;

    public InativarHistoricoUseCase(IRepository<Contas_Db.Model.Historico> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.SoftDeleteAsync(id);
}
