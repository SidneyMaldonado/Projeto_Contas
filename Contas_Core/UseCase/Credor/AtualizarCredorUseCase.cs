using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Credor;

public class AtualizarCredorUseCase
{
    private readonly IRepository<Contas_Db.Model.Credor> _repository;

    public AtualizarCredorUseCase(IRepository<Contas_Db.Model.Credor> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Credor entity) => _repository.UpdateAsync(entity);
}
