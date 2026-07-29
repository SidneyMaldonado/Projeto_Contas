using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Historico;

public class AtualizarHistoricoUseCase
{
    private readonly IRepository<Contas_Db.Model.Historico> _repository;

    public AtualizarHistoricoUseCase(IRepository<Contas_Db.Model.Historico> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Historico entity) => _repository.UpdateAsync(entity);
}
