using Contas_Db.Repository;

namespace Contas_Core.UseCase.Credor;

public class AdicionarCredorUseCase
{
    private readonly IRepository<Contas_Db.Model.Credor> _repository;

    public AdicionarCredorUseCase(IRepository<Contas_Db.Model.Credor> repository)
    {
        _repository = repository;
    }

    public Task ExecuteAsync(Contas_Db.Model.Credor entity) => _repository.AddAsync(entity);
}
