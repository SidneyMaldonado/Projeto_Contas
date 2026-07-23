using Contas_Db.Repository.Interface;

namespace Contas_Core.UseCase.Divida;

public class ObterTodosDividaUseCase
{
    private readonly IRepository<Contas_Db.Model.Divida> _repository;

    public ObterTodosDividaUseCase(IRepository<Contas_Db.Model.Divida> repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Contas_Db.Model.Divida>> ExecuteAsync() => _repository.GetAllAsync();
}
