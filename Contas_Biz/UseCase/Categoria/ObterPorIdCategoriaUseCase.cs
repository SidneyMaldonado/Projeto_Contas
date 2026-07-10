using Contas_Db.Repository;

namespace Contas_Biz.UseCase.Categoria;

public class ObterPorIdCategoriaUseCase
{
    private readonly IRepository<Contas_Db.Model.Categoria> _repository;

    public ObterPorIdCategoriaUseCase(IRepository<Contas_Db.Model.Categoria> repository)
    {
        _repository = repository;
    }

    public Task<Contas_Db.Model.Categoria?> ExecuteAsync(int id) => _repository.GetByIdAsync(id);
}
