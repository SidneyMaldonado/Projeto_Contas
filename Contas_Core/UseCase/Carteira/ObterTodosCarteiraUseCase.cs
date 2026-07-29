using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Carteira;

public class ObterTodosCarteiraUseCase
{
    private readonly IRepository<Contas_Db.Model.Carteira> _repository;

    public ObterTodosCarteiraUseCase(IRepository<Contas_Db.Model.Carteira> repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Contas_Db.Model.Carteira>> ExecuteAsync() => _repository.GetAllAsync();
}
