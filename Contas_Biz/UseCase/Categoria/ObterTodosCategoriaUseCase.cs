using Contas_Db.Repository;

namespace Contas_Biz.UseCase.Categoria;

public class ObterTodosCategoriaUseCase
{
    private readonly IRepository<Contas_Db.Model.Categoria> _repository;

    public ObterTodosCategoriaUseCase(IRepository<Contas_Db.Model.Categoria> repository)
    {
        _repository = repository;
    }

    public Task<IEnumerable<Contas_Db.Model.Categoria>> ExecuteAsync() => _repository.GetAllAsync();
}
