using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Credor;

public class ExcluirCredorUseCase
{
    private readonly IRepository<Contas_Db.Model.Credor> _repository;

    public ExcluirCredorUseCase(IRepository<Contas_Db.Model.Credor> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(int id) => _repository.DeleteAsync(id);
}
